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
		Task<Protos.SiswaModel> GetByCode(string code, ServerCallContext context);
		Task<Protos.resSiswaAll> GetAll(Protos.SiswaEmpty o, ServerCallContext context);
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


		public Task<SiswaModel> GetByCode(string code, ServerCallContext context)
		{
			throw new NotImplementedException();
		}
	}
}
