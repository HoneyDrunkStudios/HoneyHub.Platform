namespace HoneyHub.Users.AppService.Services.SecurityServices;

public interface IPasswordService
{
	string CreateSalt();
	string HashPassword(string password, string salt);
	bool VerifyPassword(string password, string hash, string salt);
}
