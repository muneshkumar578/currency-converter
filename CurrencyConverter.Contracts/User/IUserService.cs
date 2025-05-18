using CurrencyConverter.Dto.Shared;
using CurrencyConverter.Dto.User.Response;

namespace CurrencyConverter.Contract.User;

public interface IUserService
{
    ApiResponseDto<UserDto?> Authenticate(string username, string password);
}
