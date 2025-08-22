using MongoDB.Driver;

namespace Entities.DataContext
{
    public interface IDataMongoDbContext
    {
        IMongoCollection<T> Set<T>() where T : class;
        IMongoClient Client { get; }
        IMongoDatabase Database { get; }
    }

    public class DataMongoDbContext: IDataMongoDbContext
    {
        public IMongoClient Client { get; }
        public IMongoDatabase Database { get; }

        public DataMongoDbContext(string connectionString, string databaseName)
        {
            Client = new MongoClient(connectionString);
            Database = Client.GetDatabase(databaseName);
        }

        public IMongoCollection<T> Set<T>() where T : class
        {
            string collectionName = typeof(T).Name;
            return Database.GetCollection<T>(collectionName);
        }
    }
}
