using System.IO;
using System.Security.Cryptography;
using Microsoft.IdentityModel.Tokens;

namespace GolfLeagueManager.Helpers
{
    public static class RsaKeyHelper
    {
        public static RsaSecurityKey GetPrivateKey(string privateKeyPath)
        {
            var privateKeyText = File.ReadAllText(privateKeyPath);
            var rsa = RSA.Create();
            rsa.ImportFromPem(privateKeyText.ToCharArray());
            return new RsaSecurityKey(rsa);
        }

        public static RsaSecurityKey GetPublicKey(string publicKeyPath)
        {
            var publicKeyText = File.ReadAllText(publicKeyPath);
            var rsa = RSA.Create();
            rsa.ImportFromPem(publicKeyText.ToCharArray());
            return new RsaSecurityKey(rsa);
        }
    }
}
