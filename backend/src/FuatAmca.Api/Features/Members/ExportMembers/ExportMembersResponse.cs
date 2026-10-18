namespace FuatAmca.Api.Features.Members.ExportMembers;

/// <summary>Uretilen dosyanin icerigi ve indirme bilgileri.</summary>
public sealed record ExportMembersResponse(byte[] Content, string FileName, string ContentType);
