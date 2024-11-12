namespace dynamicUssdProject.REPO
{
    public interface IUserPinRepository
    {
        Task<bool> VerifyPinAsync(string userId, string pin);
        Task<bool> SetPinAsync(string userId, string pin);
        Task<bool> UpdatePinAsync(string userId, string newPin);
        Task<bool> RegisterUserAsync(string phoneNumber, string pin, decimal balance, int userId);
    }

}
