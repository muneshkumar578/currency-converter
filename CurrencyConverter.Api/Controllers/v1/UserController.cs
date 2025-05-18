using CurrencyConverter.Contract.User;
using CurrencyConverter.Dto.User.Request;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CurrencyConverter.Api.Controllers.v1;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]

public class UserController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly ITokenService _tokenService;

    public UserController(IUserService userService, ITokenService tokenService)
    {
        _userService = userService;
        _tokenService = tokenService;
    }

    [AllowAnonymous]
    [HttpPost("authenticate")]
    public IActionResult Authenticate([FromBody] UserAuthenticateRequest request)
    {
        var response = _userService.Authenticate(request.UserName, request.Password);

        if (!response.Success)
        {
            return Unauthorized(response);
        }

        var tokenResponse = _tokenService.GenerateToken(response.Data!);

        return Ok(tokenResponse);
    }
}
