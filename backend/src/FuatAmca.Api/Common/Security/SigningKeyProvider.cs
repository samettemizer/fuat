using System.Security.Cryptography;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace FuatAmca.Api.Common.Security;

/// <summary>
/// RS256 imza anahtarini yukler. Sira: konfigurasyondaki PEM icerigi, PEM dosyasi,
/// (sadece Development'ta) uretilip diske yazilan gelistirme anahtari.
/// Uretimde anahtar bulunamazsa uygulama acilista hata verir.
/// </summary>
public sealed class SigningKeyProvider : IDisposable
{
    private readonly RSA _rsa;

    public SigningKeyProvider(
        IOptions<JwtSettings> settings,
        IHostEnvironment environment,
        ILogger<SigningKeyProvider> logger)
    {
        var jwt = settings.Value;
        _rsa = RSA.Create();

        if (!string.IsNullOrWhiteSpace(jwt.PrivateKeyPem))
        {
            _rsa.ImportFromPem(jwt.PrivateKeyPem);
            CanSign = true;
            logger.LogInformation("JWT signing key loaded from configuration.");
        }
        else if (!string.IsNullOrWhiteSpace(jwt.KeyFilePath) && File.Exists(jwt.KeyFilePath))
        {
            _rsa.ImportFromPem(File.ReadAllText(jwt.KeyFilePath));
            CanSign = true;
            logger.LogInformation("JWT signing key loaded from {Path}.", jwt.KeyFilePath);
        }
        else if (!string.IsNullOrWhiteSpace(jwt.PublicKeyPem))
        {
            _rsa.ImportFromPem(jwt.PublicKeyPem);
            CanSign = false;
            logger.LogInformation("Public key loaded for validation only.");
        }
        else if (environment.IsDevelopment() && !string.IsNullOrWhiteSpace(jwt.KeyFilePath))
        {
            _rsa = RSA.Create(2048);
            var directory = Path.GetDirectoryName(jwt.KeyFilePath);
            if (!string.IsNullOrWhiteSpace(directory))
            {
                Directory.CreateDirectory(directory);
            }

            File.WriteAllText(jwt.KeyFilePath, _rsa.ExportPkcs8PrivateKeyPem());
            CanSign = true;
            logger.LogWarning(
                "A development RSA signing key was generated at {Path}. Configure Jwt:PrivateKeyPem in production.",
                jwt.KeyFilePath);
        }
        else
        {
            throw new InvalidOperationException(
                "JWT signing key was not found. Configure Jwt:PrivateKeyPem or Jwt:KeyFilePath.");
        }

        Key = new RsaSecurityKey(_rsa) { KeyId = CreateKeyId(_rsa) };
    }

    /// <summary>Token dogrulamada ve imzalamada kullanilan anahtar.</summary>
    public RsaSecurityKey Key { get; }

    /// <summary>Ozel anahtar yuklendi mi (token uretebilir mi).</summary>
    public bool CanSign { get; }

    /// <summary>RS256 imza bilgisi.</summary>
    public SigningCredentials SigningCredentials => CanSign
        ? new SigningCredentials(Key, SecurityAlgorithms.RsaSha256)
        : throw new InvalidOperationException("Tokens cannot be signed because no private key is loaded.");

    public void Dispose() => _rsa.Dispose();

    /// <summary>Acik anahtardan deterministik bir kid uretir.</summary>
    private static string CreateKeyId(RSA rsa)
    {
        var hash = SHA256.HashData(rsa.ExportSubjectPublicKeyInfo());
        return Convert.ToHexString(hash)[..16];
    }
}
