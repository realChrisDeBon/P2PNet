namespace P2PBootstrap
{
    public static class GlobalConfig
    {
        public static IConfiguration AppSettings;
        public static BootstrapTrustPolicyType TrustPolicy = BootstrapTrustPolicyType.Trustless;
        public const string ConfigFile = "appsettings.json";
        
    }
}
