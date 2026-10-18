using FluentValidation;
using FuatAmca.Api.Common.BusinessRules;
using FuatAmca.Api.Common.Enums;

namespace FuatAmca.Api.Features.ParaHareketleri.HareketOlustur;

internal sealed class HareketOlusturValidator : AbstractValidator<HareketOlusturRequest>
{
    public HareketOlusturValidator()
    {
        RuleFor(request => request.MemberId)
            .NotEmpty().WithMessage("Üye seçimi zorunludur.");

        RuleFor(request => request.GiderTipi)
            .IsInEnum().WithMessage("Gider tipi geçersiz.");

        RuleFor(request => request.Yon)
            .IsInEnum().WithMessage("Hareket yönü geçersiz.");

        RuleFor(request => request.Tutar)
            .GreaterThan(0).WithMessage("Tutar sıfırdan büyük olmalıdır.")
            .LessThanOrEqualTo(1_000_000).WithMessage("Tutar 1.000.000 TL'yi aşamaz.");

        RuleFor(request => request.Description)
            .MaximumLength(500).WithMessage("Açıklama en fazla 500 karakter olabilir.");

        // Tahakkuk her zaman bir doneme yazilir; odemede donem bilgisi opsiyoneldir.
        When(request => request.Yon == HareketYonu.Tahakkuk, () =>
        {
            RuleFor(request => request.DonemYil)
                .NotNull().WithMessage("Tahakkuk için dönem yılı zorunludur.");

            RuleFor(request => request.DonemAy)
                .NotNull().WithMessage("Tahakkuk için dönem ayı zorunludur.");
        });

        RuleFor(request => request)
            .Must(request => TahakkukKurallari.DonemGecerliMi(request.DonemYil!.Value, request.DonemAy!.Value))
            .When(request => request.DonemYil is not null && request.DonemAy is not null)
            .WithName(nameof(HareketOlusturRequest.DonemYil))
            .WithMessage("Dönem geçersiz. Yıl 2000-2100, ay 1-12 aralığında olmalıdır.");

        // Yil / ay tek basina girildiyse digeri de istenir; yarim donem bilgisi kayda yazilmaz.
        RuleFor(request => request.DonemAy)
            .NotNull()
            .When(request => request.DonemYil is not null)
            .WithMessage("Dönem yılı girildiyse dönem ayı da zorunludur.");

        RuleFor(request => request.DonemYil)
            .NotNull()
            .When(request => request.DonemAy is not null)
            .WithMessage("Dönem ayı girildiyse dönem yılı da zorunludur.");
    }
}
