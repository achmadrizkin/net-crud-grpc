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
					await _repo.db().Add(oNew);
				}
				else
				{
					throw new Exception("Data validation error");
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
				// Attempt to retrieve data from cache
				var siswaList = await _repo.cache().GetCaches();
				if (siswaList == null || !siswaList.Any())
				{
					// Fallback to database if cache is empty
					siswaList = await _repo.db().GetAll();
					await _repo.cache().SetCaches(siswaList); // Update the cache
				}

				// Map the data to the Protos response format
				var protoSiswaList = _mapper.Map<List<Protos.SiswaModel>>(siswaList);

				// Construct the response
				var response = new Protos.resSiswaAll();
				response.ListSiswa.AddRange(protoSiswaList); // Use the correct property

				return response;
			}
			catch (Exception ex)
			{
				throw new RpcException(new Status(StatusCode.Internal, "Error fetching all records"), ex.Message);
			}
		}

		public async Task<Protos.resSiswa> Get(Protos.SiswaId o, ServerCallContext context)
		{
			try
			{
				// Attempt to retrieve the Siswa record from cache
				var siswa = await _repo.cache().GetCache(o.Id.ToString());
				if (siswa == null)
				{
					// Fallback to database if cache is empty
					siswa = await _repo.db().Get(new Models.Siswa { Id = o.Id });
					await _repo.cache().SetCache(siswa); // Update the cache
				}

				// Map the data model to the Protos response format
				var protoSiswa = _mapper.Map<Protos.SiswaModel>(siswa);

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

				// Update the database and cache
				var siswa = await _repo.db().Update(updatedSiswa);
				await _repo.cache().Clear(); // Clear all caches or implement specific removal
				await _repo.cache().SetCache(siswa); // Update the cache

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

				// Delete from database
				await _repo.db().Delete(siswa);

				// Remove from cache
				await _repo.cache().Clear(); // Clear all caches or implement specific removal
				await _repo.cache().ClearSiswa(o.Id); // Clear all caches or implement specific removal

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
	}

}
