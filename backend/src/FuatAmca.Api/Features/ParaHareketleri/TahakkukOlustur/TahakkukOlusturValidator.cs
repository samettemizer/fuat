using FluentValidation;
using FuatAmca.Api.Common.BusinessRules;

namespace FuatAmca.Api.Features.ParaHareketleri.TahakkukOlustur;

internal sealed class TahakkukOlusturValidator : AbstractValidator<TahakkukOlusturRequest>
{
    public TahakkukOlusturValidator()
    {
        RuleFor(request => request.Yil)
            .Must((request, yil) => TahakkukKurallari.DonemGecerliMi(yil, request.Ay))
            .WithMessage("Tahakkuk dönemi geçersiz. Yıl 2000-2100, ay 1-12 aralığında olmalıdır.");

        When(request => request.GiderTipleri is not null, () =>
        {
            RuleFor(request => request.GiderTipleri!)
                .NotEmpty().WithMessage("Gider tipi listesi boş olamaz.");

            RuleForEach(request => request.GiderTipleri!)
                .IsInEnum().WithMessage("Gider tipi geçersiz.");
        });

        RuleFor(request => request.Description)
            .MaximumLength(500).WithMessage("Açıklama en fazla 500 karakter olabilir.");
    }
}
