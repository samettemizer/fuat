using FuatAmca.Api.Common.Enums;

namespace FuatAmca.Api.Features.ParaHareketleri.TahakkukOlustur;

/// <summary>Uretilen tahakkukun gider kalemi bazinda ozeti.</summary>
public sealed record TahakkukKalemSummaryResponse(
    GiderTipi GiderTipi,
    int RecordCount,
    decimal ToplamTutar);

/// <summary>
/// Aylik tahakkuk uretim sonucu.
/// AtlananKayitSayisi: donem icin kaydi zaten bulunan ya da is kurallari geregi
/// (kiraci muafiyeti gibi) tahakkuk uretilmeyen uye x kalem kombinasyonlaridir.
/// UyeSayisi * islenen kalem sayisi = OlusturulanKayitSayisi + AtlananKayitSayisi.
/// </summary>
public sealed record TahakkukOlusturResponse(
    int Yil,
    int Ay,
    int MemberCount,
    int CreatedRecordCount,
    int SkippedRecordCount,
    decimal ToplamTutar,
    IReadOnlyList<TahakkukKalemSummaryResponse> Kalemler);
