using FluentValidation;

namespace FuatAmca.Api.Features.Duyurular.DuyuruGuncelle;

internal sealed class DuyuruGuncelleValidator : AbstractValidator<DuyuruGuncelleRequest>
{
    public DuyuruGuncelleValidator()
    {
        RuleFor(request => request.Title)
            .NotEmpty().WithMessage("Duyuru başlığı zorunludur.")
            .MaximumLength(200);

        RuleFor(request => request.Content)
            .NotEmpty().WithMessage("Duyuru içeriği zorunludur.")
            .MaximumLength(4000);

        RuleFor(request => request.ExpiresOn)
            .Must((request, expiresOn) => expiresOn!.Value >= request.PublishedOn)
            .When(request => request.ExpiresOn is not null)
            .WithMessage("Bitiş tarihi yayın tarihinden önce olamaz.");
    }
}
