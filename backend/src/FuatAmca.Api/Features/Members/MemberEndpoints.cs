using FuatAmca.Api.Features.Members.GetMember;
using FuatAmca.Api.Features.Members.UpdateMember;
using FuatAmca.Api.Features.Members.ListMembers;
using FuatAmca.Api.Features.Members.ExportMembers;
using FuatAmca.Api.Features.Members.CreateMember;
using FuatAmca.Api.Features.Members.DeactivateMember;

namespace FuatAmca.Api.Features.Members;

/// <summary>
/// Member endpointleri: /members
/// Tum rotalar apartman kapsamlidir; handler'lar erisim kontrolunu kendileri yapar.
/// </summary>
public static class MemberEndpoints
{
    public static IEndpointRouteBuilder MapMemberEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/members")
            .WithTags("Members")
            .RequireAuthorization();

        group.MapCreateMember();
        group.MapListMembers();
        group.MapExportMembers();
        group.MapGetMember();
        group.MapUpdateMember();
        group.MapDeactivateMember();

        return endpoints;
    }
}
