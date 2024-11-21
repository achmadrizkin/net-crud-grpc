using net_test_generator_svc.Protos;
using FluentValidation;
using net_test_generator_svc.Models;

namespace net_test_generator_svc.Validators
{
    public class SiswaValidator : AbstractValidator<Siswa>
    {
        public SiswaValidator()
        {
		//DI SESUAIKAN VALIDASI NYA
		//RuleFor(c => c.Nama).NotEmpty().MaximumLength(255);
		RuleFor(c => c.Nama).NotNull().NotEmpty();
		RuleFor(c => c.Alamat).MaximumLength(255);
		RuleFor(c => c.Telepon).MaximumLength(255);

        }
    }
}
