using MySql.Data.MySqlClient;
using System.Data;

namespace net_test_generator_svc.Config.MySql
{
    public class DbConnectionFactory : IDbConnectionFactory
    {
        private readonly string _connectionString;

        public DbConnectionFactory(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<IDbConnection> CreateConnectionAsync()
        {
            try
            {
                var connection = new MySqlConnection(_connectionString);
                //await connection.OpenAsync();
                return connection;
            }
            catch
            {
                throw;
            }
        }
    }
}
