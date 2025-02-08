using System.Text.Json.Serialization;

namespace P2PBootstrap.Encryption.Pgp.Keys
    {
    [Serializable]
    public class KeyPair : IKeyPair
        {
        [JsonPropertyName("Public")]
        public PGPKeyInfo Public { get; set; }
        [JsonPropertyName("Private")]
        public PGPKeyInfo Private { get; set; }

        public KeyPair() { }
        public KeyPair(PGPKeyInfo newPublic, PGPKeyInfo newPrivate)
            {
            Public = newPublic;
            Private = newPrivate;
            }
        }

    public interface IKeyPair
        {
        public PGPKeyInfo Public { get; set; }
        public PGPKeyInfo Private { get; set; }
        }

    }
