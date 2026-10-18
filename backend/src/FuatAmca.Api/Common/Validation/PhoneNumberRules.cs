using System.Text.RegularExpressions;

namespace FuatAmca.Api.Common.Validation;

/// <summary>
/// Cep telefonu kurallari: basinda sifir olmadan 10 hane (ornek: 5321234567).
/// </summary>
public static partial class PhoneNumberRules
{
    /// <summary>Basi 0 olmayan, 10 haneli numara deseni.</summary>
    [GeneratedRegex("^[1-9][0-9]{9}$", RegexOptions.CultureInvariant)]
    public static partial Regex Pattern();

    public static bool IsValid(string? phoneNumber) =>
        !string.IsNullOrWhiteSpace(phoneNumber) && Pattern().IsMatch(phoneNumber);

    /// <summary>
    /// Serbest formda gelen numarayi 10 haneye indirger: bosluk/isaret temizler,
    /// bastaki 90 veya 0 onekini atar. Cevrilemezse null doner.
    /// </summary>
    public static string? Normalize(string? phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
        {
            return null;
        }

        var digits = string.Concat(phoneNumber.Where(char.IsAsciiDigit));

        if (digits.Length > 10 && digits.StartsWith("90", StringComparison.Ordinal))
        {
            digits = digits[2..];
        }

        digits = digits.TrimStart('0');

        return IsValid(digits) ? digits : null;
    }
}
