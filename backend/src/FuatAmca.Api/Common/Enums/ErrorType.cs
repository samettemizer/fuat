namespace FuatAmca.Api.Common.Enums;

/// <summary>
/// Beklenen hata turleri. HTTP durum kodu eslemesi bu tur uzerinden yapilir.
/// </summary>
public enum ErrorType
{
    Validation = 1,
    NotFound = 2,
    Conflict = 3,
    Unauthorized = 4,
    Forbidden = 5,
    BusinessRule = 6
}
