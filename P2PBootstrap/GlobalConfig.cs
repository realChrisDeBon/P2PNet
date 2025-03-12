using P2PBootstrap.Encryption.Pgp;
using P2PBootstrap.Encryption.Pgp.Keys;

namespace P2PBootstrap
{
    public static class GlobalConfig
    {
        public static IConfiguration AppSettings;
        public static BootstrapTrustPolicyType TrustPolicy = BootstrapTrustPolicyType.Authority;
        public const string ConfigFile = "appsettings.json";
        public static KeyPair ActiveKeys { get; set; } = new KeyPair();

        /// <summary>
        /// Sets the target keys in GlobalConfig to the keys specified in appsettings.json.
        /// </summary>
        public static void SetTargetKeys()
        {
            string keysDirectory = AppSettings["Configuration:KeysDirectory"];
            string publicKeyPath = Path.Combine(AppContext.BaseDirectory, keysDirectory, AppSettings["Configuration:AuthorityKey:PublicKey"]);
            string privateKeyPath = Path.Combine(AppContext.BaseDirectory, keysDirectory, AppSettings["Configuration:AuthorityKey:PrivateKey"]);

            if (File.Exists(publicKeyPath) && File.Exists(privateKeyPath))
            {
                byte[] publicKeyData = File.ReadAllBytes(publicKeyPath);
                byte[] privateKeyData = File.ReadAllBytes(privateKeyPath);

                ActiveKeys = new KeyPair(
                    new PGPKeyInfo(Path.GetFileNameWithoutExtension(publicKeyPath), publicKeyData),
                    new PGPKeyInfo(Path.GetFileNameWithoutExtension(privateKeyPath), privateKeyData)
                );
            }
            else
            {
                DebugMessage("Public or private key file not found.", MessageType.Warning);
            }
        }
    }
}
