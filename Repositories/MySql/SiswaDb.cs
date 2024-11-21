using Dapper;
using Google.Rpc;
using net_test_generator_svc.Config;
using net_test_generator_svc.Models;
using MySql.Data.MySqlClient;

namespace net_test_generator_svc.Repositories.MySql
{
	public interface ISiswaDb
	{
		Task<Models.Siswa> Add(Models.Siswa o);
		Task<Models.Siswa> Update(Models.Siswa o);
		Task<Models.Siswa> Delete(Models.Siswa o);
		Task<Models.Siswa> GetByCode(string code);
		Task<List<Models.Siswa>> GetAll();
		Task<List<Models.Siswa>> GetByPage(Models.Siswa o, int page, int pageSize, int totalRec);
	}

	public class SiswaDb : ISiswaDb
	{
		#region SqlCommand
		string table = "mst_siswa";
		string fields = "Id ,Nama ,Alamat ,Telepon ";
		string fields_insert = "@Id ,@Nama ,@Alamat ,@Telepon ";
		string fields_update = "Nama = @Nama, Alamat = @Alamat, Telepon = @Telepon where Id = @Id";

		#endregion

		private readonly IDbConnectionFactory _conFactory;
		private readonly MySqlConnection _conn;

		public SiswaDb(IDbConnectionFactory connectionFactory)
		{
			_conFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
			_conn = (MySqlConnection)_conFactory.CreateConnectionAsync().Result;
		}

		public async Task<Models.Siswa> Add(Models.Siswa o)
		{
			var dt = DateTime.UtcNow;
			string sqlQuery = $"insert into {table} ({fields}) values ({fields_insert})";
			try
			{
				_ = await _conn.ExecuteAsync(sqlQuery, new
				{
					Id = o.Id,
					Nama = o.Nama,
					Alamat = o.Alamat,
					Telepon = o.Telepon
				});
				return o;
			}
			catch
			{
				throw;
			}
		}
		public async Task<Models.Siswa> Update(Models.Siswa o)
		{
			var dt = DateTime.UtcNow;
			string sqlQuery = $"update {table} set {fields_update}";
			try
			{
				_ = await _conn.ExecuteAsync(sqlQuery, new
				{
					Id = o.Id
				});
				return o;
			}
			catch
			{
				throw;
			}
		}
		public async Task<Models.Siswa> Delete(Models.Siswa o)
		{
			var dt = DateTime.UtcNow;
			string sqlQuery = $"delete from {table} where  Id = @Id";
			try
			{
				_ = await _conn.ExecuteAsync(sqlQuery, new
				{
					Id = o.Id
				});
				return o;
			}
			catch
			{
				throw;
			}
		}

		public async Task<Models.Siswa> GetByCode(string code)
		{
			throw new NotImplementedException();
		}

		public async Task<List<Models.Siswa>> GetAll()
		{
			string sqlQuery = $"SELECT {fields} FROM {table}";
			try
			{
				var result = await _conn.QueryAsync<Models.Siswa>(sqlQuery);
				return result.ToList();
			}
			catch (Exception ex)
			{
				// Log the exception (if logging is implemented)
				throw new Exception("Error fetching all records", ex);
			}
		}


		public async Task<List<Models.Siswa>> GetByPage(Models.Siswa o, int page, int pageSize, int totalRec)
		{
			throw new NotImplementedException();
		}
	}
}
