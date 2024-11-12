using Azure.Core;
using dynamicUssdProject.Data;
using dynamicUssdProject.Models;
using dynamicUssdProject.Models.Request;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace dynamicUssdProject.REPO
{
    public class UserRepository : IUserRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly MongoDbContext mongoDbContext;

        public UserRepository(ApplicationDbContext context, MongoDbContext mongoContext)
        {
            _context = context;
            mongoDbContext = mongoContext;
        }

        public async Task<decimal> GetBalanceAsync(string phoneNumber, string pin)
        {
            // Step 1: Retrieve the hashed PIN from MongoDB
            var userPin = await mongoDbContext.UserPins
                .Find(up => up.PhoneNumber == phoneNumber)
                .FirstOrDefaultAsync();

            if (userPin == null)
            {
                throw new InvalidOperationException("Pin not found"); // Or handle this differently
            }

            // Step 2: Verify the PIN
            bool isPinValid = BCrypt.Net.BCrypt.Verify(pin, userPin.PinHash);
            if (!isPinValid)
            {
                throw new InvalidOperationException("Invalid PIN"); // Or handle this differently
            }

            // Step 3: Fetch the balance from SQL Server
            var account = await _context.Accounts
                .FirstOrDefaultAsync(u => u.PhoneNumber == phoneNumber);

            if (account == null)
            {
                throw new InvalidOperationException("account not found"); 
            }

            return account.Balance; // This returns a non-nullable decimal
        }
        public async Task<bool> RegisterUserAsync(string phoneNumber, string pin, decimal balance, int userId)
        {
            // Check if user already exists in SQL Server
            var userExistsInSql = await _context.Accounts.AnyAsync(u => u.PhoneNumber == phoneNumber);

            // If the user exists in either SQL or MongoDB, return false
            if (userExistsInSql)
            {
                return false;
            }

            // Hash the PIN for security
            var pinHash = BCrypt.Net.BCrypt.HashPassword(pin);

            // Create new user in SQL Server
            var newUser = new User
            {
                PhoneNumber = phoneNumber,
                Id = userId
               
            };
            await _context.Users.AddAsync(newUser);
            await _context.SaveChangesAsync();

            // Create a new account linked to the user in SQL Server
            var newAccount = new Account
            {
                PhoneNumber = phoneNumber,
                Balance = balance,
                UserId = newUser.Id,
                Pin = pinHash
            };
            await _context.Accounts.AddAsync(newAccount);
            await _context.SaveChangesAsync();

            // Store the hashed PIN in MongoDB
            var userPin = new UserPin { PhoneNumber = phoneNumber, PinHash = pinHash };
            await mongoDbContext.UserPins.InsertOneAsync(userPin);

            return true; // Registration successful
        }
        public async Task<bool> VerifyUserAsync(string phoneNumber, string pin)
        {
            // Step 1: Retrieve the hashed PIN from MongoDB based on the phone number
            var userPin = await mongoDbContext.UserPins
                .Find(up => up.PhoneNumber == phoneNumber)
                .FirstOrDefaultAsync();

            if (userPin == null)
            {
                // No entry found for the phone number in MongoDB
                return false;
            }

            // Step 2: Verify the PIN entered by the user against the stored hash
            bool isPinValid = BCrypt.Net.BCrypt.Verify(pin, userPin.PinHash);
            if (!isPinValid)
            {
                // The PIN is incorrect
                return false;
            }

            // Step 3: Ensure that the user exists in the SQL Server database
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.PhoneNumber == phoneNumber);

            if (user == null)
            {
                // No user found with this phone number
                return false;
            }

            // If PIN is valid and user exists, return true
            return true;
        }

        public async Task<User> GetUserByPhoneNumberAsync(string phoneNumber)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.PhoneNumber == phoneNumber);
        }
        public async Task<string> GetMiniStatementAsync(string phoneNumber)
        {
            // Find the account linked to the provided phone number and include its transactions
            var account = await _context.Accounts
                .Include(a => a.Transactions) // Include related transactions
                .FirstOrDefaultAsync(a => a.PhoneNumber == phoneNumber);

            if (account == null)
            {
                return "Account not found."; // Return message if account is not found
            }

            // Get the last 5 transactions (or all, if you prefer)
            var transactions = account.Transactions
                .OrderByDescending(t => t.TransactionDate) // Order by the most recent transactions
                .Take(10) // You can modify this number if you need more transactions
                .ToList();

            if (transactions.Count == 0)
            {
                return "No transactions found."; // Return a message if no transactions exist
            }

            // Build the mini statement message
            var statement = new StringBuilder();
            statement.AppendLine("Your recent transactions are:");

            foreach (var transaction in transactions)
            {
                // Check if there's a charge and append the information accordingly
                string chargeInfo = transaction.TransactionCharge == 0
                    ? "No Charge"
                    : $"Charge: {transaction.TransactionCharge:C}";

                statement.AppendLine($"Date: {transaction.TransactionDate.ToString("MM/dd/yyyy HH:mm:ss")}, " +
                                     $"Amount: {transaction.Amount:C}, " +
                                     $"Type: {transaction.TransactionType}, " +
                                     $"{chargeInfo}");
            }

            return statement.ToString(); // Return the formatted mini statement
        }



        public async Task<string> TransferToMobileAsync(string phoneNumber, string recipientNumber, decimal amount)
        {
            // Retrieve the sender and recipient accounts using their phone numbers
            var sender = await _context.Accounts
                .FirstOrDefaultAsync(a => a.PhoneNumber == phoneNumber);
            var recipient = await _context.Accounts
                .FirstOrDefaultAsync(a => a.PhoneNumber == recipientNumber);

            // Check if both sender and recipient accounts exist
            if (sender == null)
            {
                return "Sender not found."; // If sender does not exist
            }

            if (recipient == null)
            {
                return "Recipient not found."; // If recipient does not exist
            }
            // Calculate the transaction charge
            decimal transactionCharge = CalculateTransactionCharge(amount);
            // Check if the sender has sufficient balance for the transfer
            if (sender.Balance < amount + transactionCharge)
            {
                return "Insufficient balance."; // If the sender's balance is insufficient
            }

            // Perform the transfer by updating the balances
            sender.Balance -= (amount + transactionCharge); // Deduct amount from sender's balance
            recipient.Balance += amount; // Add amount to recipient's balance

            // Create a transaction record for the sender
            var senderTransaction = new Transaction
            {
                AccountId = sender.Id, // Associate transaction with sender's account
                Amount = -amount, // Negative amount for deduction
                TransactionCharge = transactionCharge,
                TransactionType = "Transfer to Mobile",
                TransactionDate = DateTime.Now
            };

            // Create a transaction record for the recipient
            var recipientTransaction = new Transaction
            {
                AccountId = recipient.Id, // Associate transaction with recipient's account
                Amount = amount, // Positive amount for receiving funds
                TransactionCharge = 0,
                TransactionType = "Transfer from Mobile",
                TransactionDate = DateTime.Now
            };

            // Add transactions to the database
            await _context.Transactions.AddAsync(senderTransaction);
            await _context.Transactions.AddAsync(recipientTransaction);

            // Save changes to the database
            await _context.SaveChangesAsync();

            return $"Transfer successful.Transaction charge of {transactionCharge:C} applied. New balance: {sender.Balance:C}"; // Return success message with sender's new balance
        }

        public async Task<string> TransferToBankAsync(string phoneNumber, string bankAccountNumber, decimal amount)
        {
            // Retrieve the user's account using the phone number
            var account = await _context.Accounts
                .FirstOrDefaultAsync(a => a.PhoneNumber == phoneNumber);

            // Check if the user exists
            if (account == null)
            {
                return "User not found."; // If the account with the provided phone number does not exist
            }

            // Check if the user has sufficient balance for the transfer
            if (account.Balance < amount)
            {
                return "Insufficient balance."; // If balance is less than the requested amount
            }

            // Deduct the amount from the user's balance
            account.Balance -= amount;

            // Record the transaction (assuming we want to record both the transaction details and the bank account number)
            var transaction = new Transaction
            {
                AccountId = account.Id, // Associate the transaction with the user's account
                Amount = -amount, // Negative amount for a debit transaction
                TransactionType = "Transfer to Bank",
                TransactionDate = DateTime.Now,
                BankAccountNumber = bankAccountNumber // Store the recipient bank account number
            };

            // Add the transaction to the database
            await _context.Transactions.AddAsync(transaction);

            // Save changes to the database
            await _context.SaveChangesAsync();

            // Return success message with the new balance
            return $"Transfer to bank account {bankAccountNumber} successful. New balance: {account.Balance:C}";
        }

        public async Task<string> TransferViaPesalinkAsync(string phoneNumber, string pesalinkId, decimal amount)
        {
            // Retrieve the user's account using the phone number
            var account = await _context.Accounts
                .FirstOrDefaultAsync(a => a.PhoneNumber == phoneNumber);

            // Check if the user exists
            if (account == null)
            {
                return "User not found."; // If the account with the provided phone number does not exist
            }

            // Check if the user has sufficient balance for the transfer
            if (account.Balance < amount)
            {
                return "Insufficient balance."; // If balance is less than the requested amount
            }

            // Deduct the amount from the user's balance
            account.Balance -= amount;

            // Record the transaction (assuming we want to record the transaction details and the Pesalink ID)
            var transaction = new Transaction
            {
                AccountId = account.Id, // Associate the transaction with the user's account
                Amount = -amount, // Negative amount for a debit transaction
                TransactionType = "Transfer via Pesalink",
                TransactionDate = DateTime.Now,
                PesalinkId = pesalinkId // Store the Pesalink ID for reference
            };

            // Add the transaction to the database
            await _context.Transactions.AddAsync(transaction);

            // Save changes to the database
            await _context.SaveChangesAsync();

            // Return success message with the new balance
            return $"Transfer via Pesalink to ID {pesalinkId} successful. New balance: {account.Balance:C}";
        }

        public async Task<string> UtilityPaymentAsync(string phoneNumber, string utilityAccountNumber, decimal amount)
        {
            // Retrieve the user's account using the phone number
            var account = await _context.Accounts
                .FirstOrDefaultAsync(a => a.PhoneNumber == phoneNumber);

            // Check if the user exists
            if (account == null)
            {
                return "User not found."; // If no account with the provided phone number exists
            }

            // Check if the user has sufficient balance
            if (account.Balance < amount)
            {
                return "Insufficient balance."; // If balance is insufficient for the transaction
            }

            // Deduct the amount from the user's balance
            account.Balance -= amount;

            // Record the transaction (assuming we want to log utility payment details)
            var transaction = new Transaction
            {
                AccountId = account.Id, // Associate the transaction with the account
                Amount = -amount, // Negative amount for a debit transaction
                TransactionType = "Utility Payment",
                TransactionDate = DateTime.Now,
                UtilityAccountNumber = utilityAccountNumber // Assuming the Transaction model has a field for UtilityAccountNumber
            };

            // Add the transaction to the database
            await _context.Transactions.AddAsync(transaction);

            // Save changes to the database
            await _context.SaveChangesAsync();

            // Return success message with the new balance
            return $"Payment to utility account {utilityAccountNumber} successful. New balance: {account.Balance:C}";
        }


        public async Task<string> DepositAsync(string phoneNumber,string pin, decimal amount)
        {
            // Find the account linked to the provided phone number
            var account = await _context.Accounts
                .Include(a => a.User)
                .FirstOrDefaultAsync(a => a.PhoneNumber == phoneNumber);

            if (account == null)
            {
                return "Account not found"; // Return message if account is not found
            }

            // Update the balance
            account.Balance += amount;
            var transaction = new Transaction
            {
                AccountId = account.Id,  // Link transaction to the correct account
                Amount = amount,         // The deposit amount
                TransactionType = "Deposit",  // Set the transaction type
                TransactionDate = DateTime.UtcNow,  // Set the current date and time
                PhoneNumber = phoneNumber,  // The phone number associated with the transaction
                                            // Set other properties as needed (e.g., BankAccountNumber, PaybillNumber)
            };
            _context.Transactions.Add(transaction);
            await _context.SaveChangesAsync();

            return "Deposit successful"; // Return message for successful deposit
        }
        public async Task<string> TillAsync(string phoneNumber, string tillNumber, decimal amount)
        {
            // Retrieve the user who is making the payment
            var account = await _context.Accounts.FirstOrDefaultAsync(u => u.PhoneNumber == phoneNumber);

            // Check if the user exists
            if (account == null)
            {
                return "User not found.";
            }

            // Check if user has sufficient balance
            if (account.Balance < amount)
            {
                return "Insufficient balance.";
            }

            // Deduct the amount from the user's balance
            account.Balance -= amount;

            // Record the transaction
            var transaction = new Transaction
            {
                AccountId = account.Id,
                TransactionType = "Payment to Till",
                Amount = amount,
                TransactionDate = DateTime.Now,
                TillNumber = tillNumber
            };
            await _context.Transactions.AddAsync(transaction);
            await _context.SaveChangesAsync();

            // Return success message with new balance
            return $"Payment to till number {tillNumber} successful. New balance: {account.Balance:C}";
        }
        public async Task<string> PaybillAsync(string phoneNumber, string paybillNumber, decimal amount)
        {
            // Retrieve the user's account using the phone number
            var account = await _context.Accounts
                .FirstOrDefaultAsync(a => a.PhoneNumber == phoneNumber);

            // Check if the user exists
            if (account == null)
            {
                return "User not found."; // If no account with the provided phone number exists
            }

            // Check if the user has sufficient balance
            if (account.Balance < amount)
            {
                return "Insufficient balance."; // If balance is insufficient for the transaction
            }

            // Deduct the amount from the user's balance
            account.Balance -= amount;

            // Record the transaction (assuming we want to log paybill details)
            var transaction = new Transaction
            {
                AccountId = account.Id, // Associate the transaction with the account
                Amount = -amount, // Negative amount for a debit transaction
                TransactionType = "Paybill",
                TransactionDate = DateTime.Now,
                PaybillNumber = paybillNumber // Assuming the Transaction model has a field for PaybillNumber
            };

            // Add the transaction to the database
            await _context.Transactions.AddAsync(transaction);

            // Save changes to the database
            await _context.SaveChangesAsync();

            // Return success message with the new balance
            return $"Payment to paybill number {paybillNumber} successful. New balance: {account.Balance:C}";
        }

        public async Task<string> BuyAirtimeAsync(string phoneNumber, decimal amount)
        {
            // Retrieve the user's account using the phone number
            var account = await _context.Accounts.FirstOrDefaultAsync(a => a.PhoneNumber == phoneNumber);

            // Check if the user exists
            if (account == null)
            {
                return "User not found."; // If no account with the provided phone number exists
            }

            // Check if the user has sufficient balance
            if (account.Balance < amount)
            {
                return "Insufficient balance."; // If balance is insufficient for the transaction
            }

            // Deduct the amount from the user's balance
            account.Balance -= amount;

            // Record the transaction
            var transaction = new Transaction
            {
                AccountId = account.Id, // Associate the transaction with the account
                Amount = -amount, // Negative amount for a debit transaction
                TransactionType = "Buy Airtime",
                TransactionDate = DateTime.Now,
                PhoneNumber = phoneNumber // Optional: to record the phone number in the transaction
            };

            // Add the transaction to the database
            await _context.Transactions.AddAsync(transaction);

            // Save changes to the database
            await _context.SaveChangesAsync();

            // Return success message with the new balance
            return $"Airtime purchase successful. New balance: {account.Balance:C}";
        }

        public async Task<string> BuyAirtimeForNumberAsync(string phoneNumber, string otherNumber, decimal amount)
        {
            // Retrieve the user who is buying airtime
            var account = await _context.Accounts.FirstOrDefaultAsync(u => u.PhoneNumber == phoneNumber);

            // Check if the user exists
            if (account == null)
            {
                return "User not found.";
            }

            // Check if user has sufficient balance
            if (account.Balance < amount)
            {
                return "Insufficient balance.";
            }
            // Check if the other phone number exists in the system (assuming Accounts table holds valid numbers)
            var otherAccount = await _context.Accounts.FirstOrDefaultAsync(u => u.PhoneNumber == otherNumber);

            // If the other number is not found, return an error message
            if (otherAccount == null)
            {
                return "The phone number for the airtime purchase does not exist.";
            }
            // Deduct the amount from the user's balance
            account.Balance -= amount;

            // Record the transaction
            var transaction = new Transaction
            {
                AccountId = account.Id,
                PhoneNumber = phoneNumber,
                TransactionType = "Buy Airtime for Another Number",
                Amount = amount,
                TransactionDate = DateTime.Now,
                OtherPhoneNumber = otherNumber
            };
            await _context.Transactions.AddAsync(transaction);
            await _context.SaveChangesAsync();

            // Return success message with new balance
            return $"Airtime purchase for {otherNumber} successful. New balance: {account.Balance:C}";
        }
        private decimal CalculateTransactionCharge(decimal amount)
        {
            if (amount < 1000)
            {
                return 20; // Charge for amounts below 1000
            }
            else if (amount >= 1001 && amount <= 5000)
            {
                return 35; // Charge for amounts between 1001 and 5000
            }
            else if (amount >= 5001 && amount <= 10000)
            {
                return 97; // Charge for amounts between 5001 and 10000
            }
            else
            {
                return 150; // Charge for amounts above 10000
            }
        }

    }

}
