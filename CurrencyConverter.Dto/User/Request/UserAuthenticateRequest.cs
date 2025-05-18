using System;

namespace CurrencyConverter.Dto.User.Request;

public class UserAuthenticateRequest
{
    public string UserName { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
