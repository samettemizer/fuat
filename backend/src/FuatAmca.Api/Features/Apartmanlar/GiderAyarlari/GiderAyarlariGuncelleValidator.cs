using FluentValidation;

namespace FuatAmca.Api.Features.Apartmanlar.GiderAyarlari;

internal sealed class GiderAyarlariGuncelleValidator : AbstractValidator<GiderAyarlariGuncelleRequest>
{
    public GiderAyarlariGuncelleValidator()
    {
        RuleFor(request => request.Kalemler)
            .NotEmpty().WithMessage("En az bir gider kalemi gönderilmelidir.");

        RuleForEach(request => request.Kalemler).ChildRules(kalem =>
        {
            kalem.RuleFor(k => k.GiderTipi)
                .IsInEnum().WithMessage("Gider tipi geçersiz.");

            kalem.RuleFor(k => k.VarsayilanTutar)
                .GreaterThanOrEqualTo(0).WithMessage("Tutar negatif olamaz.")
                .LessThanOrEqualTo(1_000_000).WithMessage("Tutar beklenenden büyük.");
        });

        RuleFor(request => request.Kalemler)
            .Must(kalemler => kalemler.Select(k => k.GiderTipi).Distinct().Count() == kalemler.Count)
            .WithMessage("Aynı gider tipi birden fazla gönderilemez.");
    }
}
