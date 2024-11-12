using System.Threading.Tasks;
using MongoDB.Driver;
using BCrypt.Net;
using dynamicUssdProject.Data;
using dynamicUssdProject.Models;
using Microsoft.EntityFrameworkCore;

namespace dynamicUssdProject.REPO
{
    public class UserPinRepository
    {
        private readonly MongoDbContext _mongoContext;
        private readonly ApplicationDbContext _sqlContext;

        public UserPinRepository(MongoDbContext mongoContext, ApplicationDbContext sqlContext)
        {
            _mongoContext = mongoContext;
            _sqlContext = sqlContext;
        }

        public async Task<bool> VerifyPinAsync(string phoneNumber, string pin)
        {
            var userPin = await _mongoContext.UserPins.Find(p => p.PhoneNumber == phoneNumber).FirstOrDefaultAsync();
            return userPin != null && BCrypt.Net.BCrypt.Verify(pin, userPin.PinHash);
        }

        public async Task<bool> SetPinAsync(string phoneNumber, string pin)
        {
            // Check if user exists in SQL Server Users table
            var userExistsInSql = await _sqlContext.Users.AnyAsync(u => u.PhoneNumber == phoneNumber);

            // Check if user exists in MongoDB UserPins collection
            var userExistsInMongo = await _mongoContext.UserPins.Find(u => u.PhoneNumber == phoneNumber).AnyAsync();

            // If the user exists in either SQL or MongoDB, return false
            if (userExistsInSql || userExistsInMongo)
            {
                return false; // User already exists in either database
            }
            // Hash the PIN and store it in MongoDB
            var pinHash = BCrypt.Net.BCrypt.HashPassword(pin);
            // If the user does not exist in both, create a new user in SQL Server
            var newUser = new User
            {
                PhoneNumber = phoneNumber,
               
            };

            await _sqlContext.Users.AddAsync(newUser);
            await _sqlContext.SaveChangesAsync();

           
            var userPin = new UserPin { PhoneNumber = phoneNumber, PinHash = pinHash };

            // Insert PIN in MongoDB
            await _mongoContext.UserPins.InsertOneAsync(userPin);

            return true; 
        }
        public async Task<bool> RegisterUserAsync(string phoneNumber, string pin, decimal balance, int userId)
        {
            // Check if user already exists in SQL Server
            var userExistsInSql = await _sqlContext.Accounts.AnyAsync(u => u.PhoneNumber == phoneNumber);

            // If the user exists in SQL, return false
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
                // Add any other necessary user details here
            };
            await _sqlContext.Users.AddAsync(newUser);
            await _sqlContext.SaveChangesAsync();

            // Create a new account linked to the user in SQL Server
            var newAccount = new Account
            {
                PhoneNumber = phoneNumber,
                Balance = balance,
                UserId = newUser.Id,
                Pin = pinHash

            };
            await _sqlContext.Accounts.AddAsync(newAccount);
            await _sqlContext.SaveChangesAsync();

            // Store the hashed PIN in MongoDB
            var userPin = new UserPin { PhoneNumber = phoneNumber, PinHash = pinHash };
            await _mongoContext.UserPins.InsertOneAsync(userPin);

            return true; // Registration successful
        }

        public async Task<bool> UpdatePinAsync(string phoneNumber, string newPin)
        {
            var pinHash = BCrypt.Net.BCrypt.HashPassword(newPin);
            var update = Builders<UserPin>.Update.Set(p => p.PinHash, pinHash);
            var result = await _mongoContext.UserPins.UpdateOneAsync(p => p.PhoneNumber == phoneNumber, update);

            return result.ModifiedCount > 0;
        }
    }
}
