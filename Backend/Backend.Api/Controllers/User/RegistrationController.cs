using Backend.Api.Contracts.User.Registration;
using Backend.Api.Mappings.User.Registration;
using Backend.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Api.Controllers.User;

[Route("api/registration")]
public class RegistrationController : ApiControllerBase
{
    private readonly IRegistrationService _registrationService;

    public RegistrationController(IRegistrationService registrationService)
    {
        _registrationService = registrationService;
    }

    [HttpPost("customer")]
    public async Task<ActionResult<RegistrationResponse>> RegisterCustomer(RegisterCustomerRequest request, CancellationToken cancellationToken)
    {
        var result = await _registrationService.RegisterCustomerAsync(request.ToDto(), cancellationToken);

        return HandleResult(result, response => response.ToResponse());
    }

    [HttpPost("seller")]
    public async Task<ActionResult<RegistrationResponse>> RegisterSeller(RegisterSellerRequest request, CancellationToken cancellationToken)
    {
        var result = await _registrationService.RegisterSellerAsync(request.ToDto(), cancellationToken);

        return HandleResult(result, response => response.ToResponse());
    }
}
