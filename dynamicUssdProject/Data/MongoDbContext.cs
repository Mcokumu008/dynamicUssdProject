using MongoDB.Driver;
using Microsoft.Extensions.Options;
using dynamicUssdProject.Models;

namespace dynamicUssdProject.Data
{
    public class MongoDbContext
    {
        private readonly IMongoDatabase _database;

        public MongoDbContext(IOptions<MongoDbSettings> settings)
        {
            var client = new MongoClient(settings.Value.ConnectionString);
            _database = client.GetDatabase(settings.Value.DatabaseName);
        }

        public IMongoCollection<UserPin> UserPins => _database.GetCollection<UserPin>("UserPins");
    }
}
