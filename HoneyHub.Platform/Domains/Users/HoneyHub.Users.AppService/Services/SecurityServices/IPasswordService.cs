namespace HoneyHub.Users.AppService.Services.SecurityServices;

public interface IPasswordService
{
	string CreateSalt();
	string HashPassword(string password, string salt);
}
