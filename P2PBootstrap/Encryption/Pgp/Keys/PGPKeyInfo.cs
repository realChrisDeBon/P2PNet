using System.Text.Json.Serialization;

namespace P2PBootstrap.Encryption.Pgp.Keys
    {
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
