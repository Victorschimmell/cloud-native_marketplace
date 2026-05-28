using Backend.Api.Contracts.Commerce.Payments;
using Backend.Api.Mappings.Commerce.Payments;
using Backend.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Api.Controllers.Commerce;

[Route("api/payments")]
public class PaymentsController : ApiControllerBase
{
    private readonly IPaymentService _paymentService;

    public PaymentsController(IPaymentService paymentService)
    {
        _paymentService = paymentService;
    }

    [HttpGet("currency")]
    public async Task<ActionResult<CurrencyResponse>> GetCurrencyByCodeAsync(string code, CancellationToken cancellationToken)
    {
        var result = await _paymentService.GetCurrencyByCodeAsync(code, cancellationToken);
        return HandleResult(result, currency => currency.ToResponse());
    }
}
