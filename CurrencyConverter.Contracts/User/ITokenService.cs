using CurrencyConverter.Dto.Shared;
using CurrencyConverter.Dto.User.Response;
using System;

namespace CurrencyConverter.Contract.User;

public interface ITokenService
{
    ApiResponseDto<string> GenerateToken(UserDto user);
}
