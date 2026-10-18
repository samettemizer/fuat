using FluentValidation;

namespace FuatAmca.Api.Features.Duyurular.DuyuruOlustur;

internal sealed class DuyuruOlusturValidator : AbstractValidator<DuyuruOlusturRequest>
{
    public DuyuruOlusturValidator()
    {
        RuleFor(request => request.Title)
            .NotEmpty().WithMessage("Duyuru başlığı zorunludur.")
            .MaximumLength(200);

        RuleFor(request => request.Content)
            .NotEmpty().WithMessage("Duyuru içeriği zorunludur.")
            .MaximumLength(4000);

        RuleFor(request => request.ExpiresOn)
            .Must((request, expiresOn) => expiresOn!.Value >= request.PublishedOn!.Value)
            .When(request => request.ExpiresOn is not null && request.PublishedOn is not null)
            .WithMessage("Bitiş tarihi yayın tarihinden önce olamaz.");
    }
}
