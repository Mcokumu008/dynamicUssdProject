using System.Net.NetworkInformation;

namespace dynamicUssdProject.REPO
{
    public class AccountService : IAccountService
    {
        private readonly UserRepository _userRepository;

        public AccountService(UserRepository userRepository)
        {
            _userRepository = userRepository;
        }
        public async Task<decimal> GetBalanceAsync(string phoneNumber, string pin)
        {
            return await _userRepository.GetBalanceAsync(phoneNumber, pin);
        }
        public async Task<string> DepositAsync(string phoneNumber, string pin, decimal amount)
        {
            try
            {
                // Verify the user first
                bool isUserVerified = await _userRepository.VerifyUserAsync(phoneNumber, pin);

                if (!isUserVerified)
                {
                    return "Invalid phone number or PIN.";
                }

                // If verified, proceed with the deposit
                var depositResult = await _userRepository.DepositAsync(phoneNumber, pin, amount);

                // Return the result (e.g., success or failure message)
                return depositResult;
            }
            catch (Exception ex)
            {
                return $"An error occurred: {ex.Message}";
            }
        }

        public async Task<string> GetMiniStatementAsync(string phoneNumber)
        {
            try
            {
                var miniStatement = await _userRepository.GetMiniStatementAsync(phoneNumber);
                return miniStatement;
            }
            catch (Exception ex)
            {
                // Log the exception if needed, and return an error message
                return $"An error occurred: {ex.Message}";
            }
        }

        public async Task<string> TransferToMobileAsync(string phoneNumber, string recipientNumber, decimal amount)
        {
            try
            {
                var transferResult = await _userRepository.TransferToMobileAsync(phoneNumber, recipientNumber, amount);
                return transferResult;
            }
            catch (Exception ex)
            {
                return $"An error occurred: {ex.Message}";
            }
        }

        public async Task<string> TransferToBankAsync(string phoneNumber, string bankAccountNumber, decimal amount)
        {
            try
            {
                var transferResult = await _userRepository.TransferToBankAsync(phoneNumber, bankAccountNumber, amount);
                return transferResult; // This includes the new balance in the message
            }
            catch (Exception ex)
            {
                return $"An error occurred: {ex.Message}";
            }
        }

        public async Task<string> TransferViaPesalinkAsync(string phoneNumber, string pesalinkId, decimal amount)
        {
            try
            {
                var transferResult = await _userRepository.TransferViaPesalinkAsync(phoneNumber, pesalinkId, amount);
                return transferResult;
            }
            catch (Exception ex)
            {
                return $"An error occurred: {ex.Message}";
            }
        }

        public async Task<string> PaybillAsync(string phoneNumber, string paybillNumber, decimal amount)
        {
            try
            {
                var paybillResult = await _userRepository.PaybillAsync(phoneNumber, paybillNumber, amount);
                return paybillResult;
            }
            catch (Exception ex)
            {
                return $"An error occurred: {ex.Message}";
            }
        }

        public async Task<string> TillAsync(string phoneNumber, string tillNumber, decimal amount)
        {
            try
            {
                var tillResult = await _userRepository.TillAsync(phoneNumber, tillNumber, amount);
                return tillResult;
            }
            catch (Exception ex)
            {
                return $"An error occurred: {ex.Message}";
            }
        }

        public async Task<string> BuyAirtimeAsync(string phoneNumber, decimal amount)
        {
            try
            {
                var airtimeResult = await _userRepository.BuyAirtimeAsync(phoneNumber, amount);
                return airtimeResult;
            }
            catch (Exception ex)
            {
                return $"An error occurred: {ex.Message}";
            }
        }

        public async Task<string> BuyAirtimeForNumberAsync(string phoneNumber, string otherNumber, decimal amount)
        {
            try
            {
                var airtimeResult = await _userRepository.BuyAirtimeForNumberAsync(phoneNumber, otherNumber, amount);
                return airtimeResult;
            }
            catch (Exception ex)
            {
                return $"An error occurred: {ex.Message}";
            }
        }
    }
}
