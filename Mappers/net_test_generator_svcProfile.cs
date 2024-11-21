using AutoMapper;
namespace net_test_generator_svc.Mappers
{
    public class net_test_generator_svcProfile : Profile
    {
        public net_test_generator_svcProfile()
        {
            // Source -> Target
			CreateMap<Models.Siswa, Protos.SiswaModel>().ReverseMap();
        }
    }
}
