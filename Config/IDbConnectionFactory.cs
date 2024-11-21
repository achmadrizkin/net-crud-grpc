using System.Data;

namespace net_test_generator_svc.Config
{
    public interface IDbConnectionFactory
    {
        public Task<IDbConnection> CreateConnectionAsync();
    }
}