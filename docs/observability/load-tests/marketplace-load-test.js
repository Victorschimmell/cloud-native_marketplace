import http from 'k6/http';
import { check, group, sleep } from 'k6';
import exec from 'k6/execution';
import { Counter, Rate } from 'k6/metrics';

const baseUrl = (__ENV.BASE_URL || 'http://localhost').replace(/\/$/, '');
const requestedCurrency = __ENV.CURRENCY || 'BRL';
const userPoolSize = parseInt(__ENV.USER_POOL_SIZE || '8', 10);
const productPageSize = parseInt(__ENV.PRODUCT_PAGE_SIZE || '50', 10);
const sleepMinSeconds = parseFloat(__ENV.SLEEP_MIN_SECONDS || '0.2');
const sleepMaxSeconds = parseFloat(__ENV.SLEEP_MAX_SECONDS || '1.2');
const loadProfile = __ENV.LOAD_PROFILE || 'case-study';

const profiles = {
  smoke: [
    { duration: '10s', target: 1 },
    { duration: '20s', target: 2 },
    { duration: '10s', target: 0 },
  ],
  'case-study': [
    { duration: '30s', target: 5 },
    { duration: '1m', target: 15 },
    { duration: '1m', target: 30 },
    { duration: '30s', target: 0 },
  ],
  stress: [
    { duration: '1m', target: 20 },
    { duration: '2m', target: 50 },
    { duration: '2m', target: 80 },
    { duration: '1m', target: 0 },
  ],
};

export const options = {
  scenarios: {
    marketplace_case_study: {
      executor: 'ramping-vus',
      stages: profiles[loadProfile] || profiles['case-study'],
      gracefulRampDown: '20s',
    },
  },
  thresholds: {
    http_req_duration: ['p(95)<3000'],
    checks: ['rate>0.85'],
  },
};

const browseFlows = new Counter('marketplace_browse_flows_total');
const checkoutSuccessFlows = new Counter('marketplace_checkout_success_flows_total');
const checkoutFailureFlows = new Counter('marketplace_checkout_failure_flows_total');
const controlledFailureRate = new Rate('marketplace_controlled_failure_rate');

export function setup() {
  const runId = `${Date.now()}-${Math.floor(Math.random() * 100000)}`;
  const currencyInfo = getCurrency(requestedCurrency);
  const products = getProducts(currencyInfo.code);
  const users = createUsers(runId);

  return {
    runId,
    products,
    currency: currencyInfo,
    users,
  };
}

export default function (data) {
  const flowRoll = Math.random();

  if (flowRoll < 0.55) {
    browseScenario(data);
  } else if (flowRoll < 0.85) {
    checkoutSuccessScenario(data);
  } else {
    checkoutFailureScenario(data);
  }

  sleep(randomBetween(sleepMinSeconds, sleepMaxSeconds));
}

function browseScenario(data) {
  group('browse', () => {
    const flowId = correlationId(data.runId, 'browse');
    const product = pick(data.products);
    const currencyCode = data.currency.code;

    const categories = http.get(`${baseUrl}/api/categories`, requestParams(null, flowId));
    check(categories, {
      'browse categories returned 200': response => response.status === 200,
    });

    const products = http.get(
      `${baseUrl}/api/products?sort=newest&currency=${encodeURIComponent(currencyCode)}&page=1&pageSize=15`,
      requestParams(null, flowId),
    );
    check(products, {
      'browse products returned 200': response => response.status === 200,
    });

    const details = http.get(
      `${baseUrl}/api/products/${product.productId}?listingId=${product.listingId}&currency=${encodeURIComponent(currencyCode)}`,
      requestParams(null, flowId),
    );
    check(details, {
      'product details returned 200': response => response.status === 200,
    });

    const reviews = http.get(`${baseUrl}/api/products/${product.productId}/reviews`, requestParams(null, flowId));
    check(reviews, {
      'product reviews returned success': response => response.status >= 200 && response.status < 500,
    });

    browseFlows.add(1);
  });
}

function checkoutSuccessScenario(data) {
  group('checkout_success', () => {
    const flowId = correlationId(data.runId, 'checkout-success');
    const user = pickUser(data.users);
    const product = pick(data.products);
    const currencyCode = data.currency.code;

    const cart = addCartItem(user, product, flowId, 1, currencyCode);
    if (!cart?.id) {
      return;
    }

    const preview = getCheckoutPreview(user, cart.id, flowId, currencyCode);
    if (!preview?.totalAmount || preview.totalAmount <= 0) {
      return;
    }

    const checkout = checkoutCart(user, cart.id, preview.totalAmount, data.currency.id, flowId, currencyCode);
    const passed = check(checkout, {
      'checkout success returned 2xx': response => response.status >= 200 && response.status < 300,
    });

    if (passed) {
      checkoutSuccessFlows.add(1);
    }
  });
}

function checkoutFailureScenario(data) {
  group('checkout_failure', () => {
    if (Math.random() < 0.5) {
      invalidCurrencyFailureScenario(data);
    } else {
      paymentMismatchFailureScenario(data);
    }
  });
}

function invalidCurrencyFailureScenario(data) {
  const flowId = correlationId(data.runId, 'checkout-invalid-currency');
  const user = pickUser(data.users);

  const response = checkoutCart(user, undefined, 1, data.currency.id, flowId, 'EUR');
  const passed = check(response, {
    'invalid currency returned 400': checkoutResponse => checkoutResponse.status === 400,
  });

  controlledFailureRate.add(passed);
  checkoutFailureFlows.add(1);
}

function paymentMismatchFailureScenario(data) {
  const flowId = correlationId(data.runId, 'checkout-payment-mismatch');
  const user = pickUser(data.users);
  const product = pick(data.products);
  const currencyCode = data.currency.code;

  const cart = addCartItem(user, product, flowId, 1, currencyCode);
  if (!cart?.id) {
    return;
  }

  const preview = getCheckoutPreview(user, cart.id, flowId, currencyCode);
  if (!preview?.totalAmount || preview.totalAmount <= 1) {
    return;
  }

  const response = checkoutCart(user, cart.id, preview.totalAmount - 1, data.currency.id, flowId, currencyCode);
  const passed = check(response, {
    'payment mismatch returned 400': checkoutResponse => checkoutResponse.status === 400,
  });

  controlledFailureRate.add(passed);
  checkoutFailureFlows.add(1);
}

function getProducts(currencyCode) {
  const response = http.get(
    `${baseUrl}/api/products?sort=newest&currency=${encodeURIComponent(currencyCode)}&page=1&pageSize=${productPageSize}`,
    requestParams(null, 'load-test-setup-products'),
  );

  check(response, {
    'setup products returned 200': setupResponse => setupResponse.status === 200,
  });

  const body = parseJson(response, 'products setup response');
  const products = (body.items || []).filter(product => product.stockQuantity > 0);

  if (products.length === 0) {
    throw new Error(`No in-stock products found at ${baseUrl}. Seed the database or choose another environment before running the load test.`);
  }

  return products;
}

function getCurrency(preferredCurrency) {
  const candidates = [...new Set([preferredCurrency, 'BRL', 'DKK', 'USD'])];

  for (const currencyCode of candidates) {
    const response = http.get(
      `${baseUrl}/api/payments/currency?code=${encodeURIComponent(currencyCode)}`,
      requestParams(null, 'load-test-setup-currency'),
    );

    if (response.status !== 200) {
      continue;
    }

    const body = parseJson(response, 'currency setup response');
    if (body.id) {
      check(response, {
        'setup currency returned 200': setupResponse => setupResponse.status === 200,
      });
      return body;
    }
  }

  throw new Error(`None of the candidate currencies returned an id: ${candidates.join(', ')}.`);
}

function createUsers(runId) {
  const users = [];
  const password = 'LoadTest123!';

  for (let index = 0; index < userPoolSize; index += 1) {
    const email = `loadtest-${runId}-${index}@example.test`;
    const response = http.post(
      `${baseUrl}/api/registration/customer`,
      JSON.stringify({
        email,
        password,
        firstName: 'Load',
        lastName: `Tester${index}`,
        phone: '+4512345678',
      }),
      jsonParams(null, correlationId(runId, 'setup-register')),
    );

    check(response, {
      'setup customer registration returned success': setupResponse => setupResponse.status >= 200 && setupResponse.status < 300,
    });

    const body = parseJson(response, 'registration response');
    const accessToken = body.token?.accessToken;
    if (!accessToken) {
      throw new Error(`Registration did not return an access token for ${email}.`);
    }

    users.push({
      email,
      password,
      accessToken,
      userId: body.user?.id,
      customerId: body.customer?.id,
    });
  }

  return users;
}

function addCartItem(user, product, flowId, quantity, currencyCode) {
  const response = http.post(
    `${baseUrl}/api/cart/items?displayCurrency=${encodeURIComponent(currencyCode)}`,
    JSON.stringify({
      listingId: product.listingId,
      quantity,
    }),
    jsonParams(user.accessToken, flowId),
  );

  check(response, {
    'add cart item returned success': cartResponse => cartResponse.status >= 200 && cartResponse.status < 300,
  });

  if (response.status < 200 || response.status >= 300) {
    return null;
  }

  return parseJson(response, 'cart response');
}

function getCheckoutPreview(user, cartId, flowId, currencyCode) {
  const response = http.get(
    `${baseUrl}/api/checkout/preview?currency=${encodeURIComponent(currencyCode)}&cartId=${cartId}`,
    requestParams(user.accessToken, flowId),
  );

  check(response, {
    'checkout preview returned 200': previewResponse => previewResponse.status === 200,
  });

  if (response.status !== 200) {
    return null;
  }

  return parseJson(response, 'checkout preview response');
}

function checkoutCart(user, cartId, paymentValue, currencyId, flowId, checkoutCurrency) {
  const body = {
    shippingAddress: {
      addressLine1: 'Load Test Street 1',
      addressLine2: null,
      city: 'Copenhagen',
      state: 'Capital Region',
      postalCode: '1000',
      countryCode: 'DK',
    },
    saveShippingAddressAsDefault: false,
    payments: [
      {
        currencyId,
        paymentType: 'CreditCard',
        paymentInstallments: 1,
        paymentValue: roundMoney(paymentValue),
        externalPaymentReference: `k6-${flowId}`,
      },
    ],
  };

  if (cartId) {
    body.cartId = cartId;
  }

  return http.post(
    `${baseUrl}/api/checkout?currency=${encodeURIComponent(checkoutCurrency)}`,
    JSON.stringify(body),
    jsonParams(user.accessToken, flowId),
  );
}

function requestParams(accessToken, flowId) {
  const headers = {
    'X-Correlation-ID': flowId,
    'X-Load-Test-Scenario': flowId,
  };

  if (accessToken) {
    headers.Authorization = `Bearer ${accessToken}`;
  }

  return {
    headers,
    tags: scenarioTags(flowId),
  };
}

function jsonParams(accessToken, flowId) {
  const params = requestParams(accessToken, flowId);
  params.headers['Content-Type'] = 'application/json';
  return params;
}

function scenarioTags(flowId) {
  if (flowId.includes('checkout-success')) {
    return { flow: 'checkout_success' };
  }

  if (flowId.includes('checkout-invalid-currency') || flowId.includes('checkout-payment-mismatch')) {
    return { flow: 'checkout_failure' };
  }

  if (flowId.includes('browse')) {
    return { flow: 'browse' };
  }

  return { flow: 'setup' };
}

function parseJson(response, description) {
  try {
    return response.json();
  } catch (error) {
    throw new Error(`Could not parse ${description}. Status=${response.status}, body=${response.body}`);
  }
}

function pick(items) {
  return items[Math.floor(Math.random() * items.length)];
}

function pickUser(users) {
  return users[exec.vu.idInTest % users.length];
}

function correlationId(runId, scenario) {
  const vuId = executionValue(() => exec.vu.idInTest, 0);
  const iteration = executionValue(() => exec.scenario.iterationInTest, 0);
  return `k6-${scenario}-${runId}-vu${vuId}-iter${iteration}`;
}

function executionValue(getValue, fallback) {
  try {
    return getValue() || fallback;
  } catch {
    return fallback;
  }
}

function randomBetween(min, max) {
  return min + Math.random() * (max - min);
}

function roundMoney(value) {
  return Math.round(value * 100) / 100;
}
