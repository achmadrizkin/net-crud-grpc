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
		Task<List<Models.Siswa>> GetAll();
		Task<Models.Siswa> Get(Models.Siswa o);
	}

	public class SiswaDb : ISiswaDb
	{
		#region SqlCommand
		string table = "mst_siswa";
		string fields = "Id ,Nama ,Alamat ,Telepon ";
		string fields_insert = "@Id ,@Nama ,@Alamat ,@Telepon ";
		string fields_update = "Nama = @Nama, Alamat = @Alamat, Telepon = @Telepon";

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
			string sqlQuery = $"UPDATE {table} SET {fields_update} WHERE Id = @Id";
			try
			{
				var result = await _conn.ExecuteAsync(sqlQuery, new
				{
					Id = o.Id,
					Nama = o.Nama,   // Ensure compatibility with Google Protobuf.StringValue
					Alamat = o.Alamat,
					Telepon = o.Telepon
				});

				if (result == 0) // If no rows are affected
				{
					throw new KeyNotFoundException($"Siswa with ID {o.Id} not found.");
				}

				return o;
			}
			catch (Exception ex)
			{
				// Log exception if logging is implemented
				throw new Exception("Error updating Siswa record", ex);
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

		public async Task<Models.Siswa> Get(Siswa o)
		{
			string sqlQuery = $"SELECT {fields} FROM {table} WHERE Id = @Id";
			try
			{
				var result = await _conn.QueryFirstOrDefaultAsync<Models.Siswa>(sqlQuery, new
				{
					Id = o.Id
				});

				if (result == null)
				{
					throw new KeyNotFoundException($"Siswa with ID {o.Id} not found.");
				}

				return result;
			}
			catch (Exception ex)
			{
				// Log exception (if logging is implemented)
				throw new Exception($"Error fetching Siswa with ID {o.Id}", ex);
			}
		}
	}
}
