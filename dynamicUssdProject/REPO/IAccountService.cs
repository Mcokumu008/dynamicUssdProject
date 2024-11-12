namespace dynamicUssdProject.REPO
{
   public interface IAccountService
    {
    Task<decimal> GetBalanceAsync(string phoneNumber, string pin);
    Task<string> DepositAsync(string phoneNumber, string pin, decimal amount);
    Task<string> GetMiniStatementAsync(string phoneNumber);
    Task<string> TransferToMobileAsync(string phoneNumber, string recipientNumber, decimal amount);
    Task<string> TransferToBankAsync(string phoneNumber, string bankAccountNumber, decimal amount);
    Task<string> TransferViaPesalinkAsync(string phoneNumber, string pesalinkId, decimal amount);
    Task<string> PaybillAsync(string phoneNumber, string paybillNumber, decimal amount);
    Task<string> TillAsync(string phoneNumber, string tillNumber, decimal amount);
    Task<string> BuyAirtimeAsync(string phoneNumber, decimal amount);
    Task<string> BuyAirtimeForNumberAsync(string phoneNumber, string otherNumber, decimal amount);
}
}
