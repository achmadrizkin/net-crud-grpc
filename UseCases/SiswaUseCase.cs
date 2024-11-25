using FluentValidation;
using Grpc.Core;
using AutoMapper;
using net_test_generator_svc.Repositories;
using net_test_generator_svc.Protos;

namespace net_test_generator_svc.UseCases
{
	public interface ISiswaUseCase
	{
		Task<Protos.resSiswaMessage> Add(Protos.SiswaModel o, ServerCallContext context);
		Task<Protos.resSiswaAll> GetAll(Protos.SiswaEmpty o, ServerCallContext context);

		Task<Protos.resSiswa> Get(Protos.SiswaId o, ServerCallContext context);
		Task<Protos.resSiswaMessage> Update(Protos.SiswaModel o, ServerCallContext context);
		Task<Protos.resSiswaMessage> Delete(Protos.SiswaId o, ServerCallContext context);
	}
	public class SiswaUseCase : ISiswaUseCase
	{
		private readonly ISiswaRepository _repo;
		private readonly IValidator<Models.Siswa> _validator;
		private readonly IMapper _mapper;

		public SiswaUseCase(ISiswaRepository repo, IValidator<Models.Siswa> validator, IMapper mapper)
		{
			_repo = repo ?? throw new ArgumentNullException(nameof(repo));
			_mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
			_validator = validator ?? throw new ArgumentNullException(nameof(validator));
		}
		public async Task<Protos.resSiswaMessage> Add(Protos.SiswaModel o, ServerCallContext context)
		{
			try
			{
				var oNew = _mapper.Map<Models.Siswa>(o);

				var res = await _validator.ValidateAsync(oNew);
				if (res.IsValid)
				{
					_ = await _repo.db().Add(oNew);
					_ = await _repo.cache().SetCache(oNew);
				}
				else
				{
					throw new Exception("Data Error validation");
				}

				return new Protos.resSiswaMessage { Message = "OK" };
			}
			catch
			{
				throw;
			}
		}
		public async Task<Protos.resSiswaAll> GetAll(Protos.SiswaEmpty o, ServerCallContext context)
		{
			try
			{
				// Fetch data from the repository
				var siswaList = await _repo.db().GetAll();

				// Map the data to the Protos response format
				var protoSiswaList = _mapper.Map<List<Protos.SiswaModel>>(siswaList);

				// Construct the response
				var response = new Protos.resSiswaAll();
				response.ListSiswa.AddRange(protoSiswaList); // Use the correct property

				return response;
			}
			catch (Exception ex)
			{
				// Log exception (if logging is implemented)
				throw new RpcException(new Status(StatusCode.Internal, "Error fetching all records"), ex.Message);
			}
		}

		public async Task<Protos.resSiswaMessage> Update(Protos.SiswaModel o, ServerCallContext context)
		{
			try
			{
				var updatedSiswa = _mapper.Map<Models.Siswa>(o);

				var validationResult = await _validator.ValidateAsync(updatedSiswa);
				if (!validationResult.IsValid)
				{
					throw new RpcException(new Status(StatusCode.InvalidArgument, "Validation failed"));
				}

				var siswa = await _repo.db().Update(updatedSiswa);
				return new Protos.resSiswaMessage { Message = $"Siswa with ID {siswa.Id} updated successfully" };
			}
			catch (KeyNotFoundException ex)
			{
				throw new RpcException(new Status(StatusCode.NotFound, ex.Message));
			}
			catch (Exception ex)
			{
				throw new RpcException(new Status(StatusCode.Internal, ex.Message));
			}
		}

		public async Task<Protos.resSiswaMessage> Delete(Protos.SiswaId o, ServerCallContext context)
		{
			try
			{
				var siswa = await _repo.db().Get(new Models.Siswa { Id = o.Id }); // Ensure the record exists
				await _repo.db().Delete(siswa);
				return new Protos.resSiswaMessage { Message = $"Siswa with ID {o.Id} deleted successfully" };
			}
			catch (KeyNotFoundException ex)
			{
				throw new RpcException(new Status(StatusCode.NotFound, ex.Message));
			}
			catch (Exception ex)
			{
				throw new RpcException(new Status(StatusCode.Internal, ex.Message));
			}
		}

		public async Task<Protos.resSiswa> Get(Protos.SiswaId o, ServerCallContext context)
		{
			try
			{
				// Fetch the Siswa record from the database
				var siswa = await _repo.db().Get(new Models.Siswa { Id = o.Id });

				// Map the data model to the Protos response format
				var protoSiswa = _mapper.Map<Protos.SiswaModel>(siswa);

				// Return the response
				return new Protos.resSiswa { Siswa = protoSiswa };
			}
			catch (KeyNotFoundException ex)
			{
				throw new RpcException(new Status(StatusCode.NotFound, ex.Message));
			}
			catch (Exception ex)
			{
				throw new RpcException(new Status(StatusCode.Internal, ex.Message));
			}
		}
	}
}
