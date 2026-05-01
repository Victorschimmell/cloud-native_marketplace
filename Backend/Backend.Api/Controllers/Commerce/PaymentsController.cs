using Backend.Api.Attributes;
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

    [HttpGet("{orderId:guid}")]
    public async Task<ActionResult<IReadOnlyList<PaymentResponse>>> GetByOrderAsync([NotEmptyGuid] Guid orderId, CancellationToken cancellationToken)
    {
        return StatusCode(StatusCodes.Status501NotImplemented, "This endpoint is not implemented yet.");
    }

    [HttpPost]
    public async Task<ActionResult<PaymentResponse>> RecordPaymentAsync([FromBody] RecordPaymentRequest request, CancellationToken cancellationToken)
    {
        return StatusCode(StatusCodes.Status501NotImplemented, "This endpoint is not implemented yet.");
    }

    [HttpGet("currency")]
    public async Task<ActionResult<CurrencyResponse>> GetCurrencyByCodeAsync(string code, CancellationToken cancellationToken)
    {
        // TODO: Implement support for multiple currencies.
        if (code != "BRL")
        {
            return StatusCode(StatusCodes.Status501NotImplemented, "This endpoint is not implemented yet.");
        }

        var result = await _paymentService.GetCurrencyByCodeAsync(code, cancellationToken);
        return HandleResult(result, currency => currency.ToResponse());
    }
}
