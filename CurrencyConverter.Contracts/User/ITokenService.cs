using CurrencyConverter.Dto.Shared;
using CurrencyConverter.Dto.User.Response;

namespace CurrencyConverter.Contract.User;

public interface ITokenService
{
    ApiResponseDto<string> GenerateToken(UserDto user);
}
