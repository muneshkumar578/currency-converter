using CurrencyConverter.Contract.User;
using CurrencyConverter.Dto.Shared;
using CurrencyConverter.Dto.User.Response;

namespace CurrencyConverter.Service.User;

public class UserService : IUserService
{
    /// <summary>
    /// This method is used to authenticate a user.
    /// This is a fake implementation for demonstration purposes.
    /// </summary>
    /// <param name="username"></param>
    /// <param name="password"></param>
    /// <returns></returns>
    public ApiResponseDto<UserDto?> Authenticate(string username, string password)
    {
        var user = FakeUsers.Users.FirstOrDefault(u => u.Username == username && u.Role == password);
        
        return user != default
            ? new ApiResponseDto<UserDto?> { Data = new UserDto { UserName = user.Username, Role = user.Role }, Success = true, Message = "User authenticated successfully." }
            : new ApiResponseDto<UserDto?> { Data = null, Success = false, Message = "Invalid username or password." };
    }
}

public static class FakeUsers
{
    public static List<(string Username, string Role)> Users =
    [
        ("admin", "Admin"),
        ("user", "User")
    ];
}
