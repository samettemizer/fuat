using FuatAmca.Api.Features.Apartmanlar;
using FuatAmca.Api.Features.Duyurular;
using FuatAmca.Api.Features.Authentication;
using FuatAmca.Api.Features.PanelUsers;
using FuatAmca.Api.Features.ParaHareketleri;
using FuatAmca.Api.Features.Members;

namespace FuatAmca.Api.Common.DependencyInjection;

/// <summary>
/// Feature gruplarinin endpoint kayitlari. Her feature kendi MapGroup'unu kurar.
/// </summary>
public static class EndpointRegistration
{
    public static WebApplication MapFuatAmcaEndpoints(this WebApplication app)
    {
        app.MapAuthenticationEndpoints();
        app.MapPanelUserEndpoints();
        app.MapApartmanEndpoints();
        app.MapMemberEndpoints();
        app.MapParaHareketiEndpoints();
        app.MapDuyuruEndpoints();

        return app;
    }
}
