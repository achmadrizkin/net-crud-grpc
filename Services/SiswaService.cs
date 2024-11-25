using Common.Logging;
using Common.Helpers;
using Grpc.Core;
using net_test_generator_svc.Protos;
using net_test_generator_svc.UseCases;
using net_test_generator_svc.Validators;
using Serilog;

namespace net_test_generator_svc.Services
{
	public class SiswaService : Protos.SiswaGrpcService.SiswaGrpcServiceBase
	{
		private readonly ISiswaUseCase _uc;
		private readonly ILogger<SiswaService> _log;

		public SiswaService(ISiswaUseCase uc, ILogger<SiswaService> log)
		{
			_uc = uc ?? throw new ArgumentNullException(nameof(uc));
			_log = log ?? throw new ArgumentNullException(nameof(log));
		}

		//Tambahkan modeling di proto sesuai kebutuhan
		public async override Task<resSiswaMessage> Add(SiswaModel request, ServerCallContext context)
		{
			// Initialize basic log information
			BasicLog basicLog = new BasicLog
			{
				TraceId = ContextHelper.GetTraceId(context),
				TrxId = ContextHelper.GetTrxId(context),
				Message = "Starting Add operation"
			};
			SDLogging.Log(basicLog.ToString());

			try
			{
				// Perform the Add operation using the UseCase
				var response = await _uc.Add(request, context);

				// Log success and elapsed time
				basicLog.Message = $"Add operation completed successfully. ElapsedTime: {basicLog.GetTimeSpan()}";
				SDLogging.Log(basicLog.ToString());

				return response;
			}
			catch (Exception ex)
			{
				// Log the error
				SDLogging.Log($"Error during Add operation: {ex.Message}", SDLogging.ERROR);

				// Set gRPC context status and propagate the error as an RpcException
				throw new RpcException(new Status(StatusCode.Aborted, $"Failed to insert new Siswa: {ex.Message}"));
			}
		}

		public override async Task<resSiswaMessage> Update(SiswaModel request, ServerCallContext context)
		{
			try
			{
				// Log the operation start
				SDLogging.Log("Starting Update operation.");

				// Perform the update via UseCase
				var response = await _uc.Update(request, context);

				SDLogging.Log("Update operation completed successfully.");
				return response;
			}
			catch (RpcException ex)
			{
				SDLogging.Log($"Update failed: {ex.Message}", SDLogging.ERROR);
				throw;
			}
		}

		public override async Task<resSiswaMessage> Delete(SiswaId request, ServerCallContext context)
		{
			try
			{
				// Log the operation start
				SDLogging.Log("Starting Delete operation.");

				// Perform the delete via UseCase
				var response = await _uc.Delete(request, context);

				SDLogging.Log("Delete operation completed successfully.");
				return response;
			}
			catch (RpcException ex)
			{
				SDLogging.Log($"Delete failed: {ex.Message}", SDLogging.ERROR);
				throw;
			}
		}

		public override async Task<resSiswa> Get(SiswaId request, ServerCallContext context)
		{
			try
			{
				SDLogging.Log($"Fetching Siswa with ID: {request.Id}");

				var response = await _uc.Get(request, context);

				SDLogging.Log($"Siswa with ID {request.Id} fetched successfully.");
				return response;
			}
			catch (RpcException ex)
			{
				SDLogging.Log($"Error fetching Siswa with ID {request.Id}: {ex.Message}", SDLogging.ERROR);
				throw;
			}
		}

		// Get all Siswa records
		public async override Task<resSiswaAll> GetAll(SiswaEmpty request, ServerCallContext context)
		{
			BasicLog basicLog = new BasicLog
			{
				TraceId = ContextHelper.GetTraceId(context),
				TrxId = ContextHelper.GetTrxId(context)
			};
			basicLog.Message = "Fetching all Siswa records.";
			SDLogging.Log(basicLog.ToString());

			try
			{
				// Call UseCase to fetch all records
				var response = await _uc.GetAll(request, context);

				basicLog.Message = $"Fetched {response.ListSiswa.Count} records. elapsedTime:{basicLog.GetTimeSpan()}";
				SDLogging.Log(basicLog.ToString());

				return response;
			}
			catch (Exception ex)
			{
				SDLogging.Log($"Error {ex.Message}", SDLogging.ERROR);
				context.Status = new Status(StatusCode.Internal, "Failed to fetch Siswa records, error " + ex.Message);
				throw new RpcException(new Status(StatusCode.Internal, $"Error fetching records: {ex.Message}"), ex.Message);
			}
		}
	}
}
