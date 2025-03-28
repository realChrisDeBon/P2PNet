using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PgpCore;
using Org.BouncyCastle.Bcpg.OpenPgp;
using System.Text.Json.Serialization;
using System.Transactions;
using P2PNet.DicoveryChannels.WAN;
using System.Security.Cryptography;

namespace P2PNet.Distribution
{
    public static class EncryptionAndSecurityHandler
    {
        private static MD5 hashing = MD5.Create();

        /// <summary>
        /// Verifies a clear signed message using the provided public key.
        /// </summary>
        /// <param name="signature">A string represenation of the clear signed message.</param>
        /// <param name="pubKey">A string representation of the public key being checked against.</param>
        /// <returns>True if this is a valid signature, otherwise false.</returns>
        public static async Task<bool> VerifySignature(string signature, string pubKey)
        {
            EncryptionKeys key = new EncryptionKeys(pubKey);
            PGP pgp = new PGP(key);
            return await pgp.VerifyAsync(signature);
        }


    }

    [Serializable]
    public class PGPKeyInfo
    {
        [JsonPropertyName("Name")]
        public string Name { get; set; }
        [JsonPropertyName("KeyData")]
        public byte[] KeyData { get; set; }

        [JsonConstructor]
        public PGPKeyInfo() { }
        public PGPKeyInfo(string name, byte[] keyData)
        {
            Name = name;
            KeyData = keyData;
        }
    }
}
