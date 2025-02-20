using System.Collections;
using Org.BouncyCastle.Pqc.Crypto.Lms;
using System.IO;
using System.Diagnostics;
using System.Security.Cryptography;
using PgpCore;
using Org.BouncyCastle.Bcpg.OpenPgp;
using P2PBootstrap.Encryption.Pgp.Keys;
using System.Text;


namespace P2PBootstrap.Encryption.Pgp
    {
    public static class PgpService
        {
        private static PGP localPGP = new PGP();
        private static string KeysDirectory { get; set; } = Path.Combine(AppContext.BaseDirectory, AppSettings["Configuration:KeysDirectory"]);
        private static KeyPair ActiveKeyPair { get; set; } = new KeyPair();
        private static List<PGPKeyInfo> _pgpKeys = new List<PGPKeyInfo>();
        private static bool PublicKeySet, PrivateKeySet = false;

        // TODO implement public accessors, advise user to implement a unique salt in documentation
        private static byte[] hashSalt = new byte[]
                               {
                                       0x12, 0x34, 0x56, 0x78, 0x9A, 0xBC, 0xDE, 0xF0, 0x11, 0x22, 0x33, 0x44, 0x55, 0x66,
                                       0x77, 0x88
                               };

        // TODO fix me
        private static string username = "bob";
        // TODO fix me
        private static string passphrase = "password";

        public static void Initialize()
        {
            // Ensure the keys directory exists
            if (!Directory.Exists(KeysDirectory))
            {
                Directory.CreateDirectory(KeysDirectory);
            }

        }

        public static bool GeneratePGPKeyPair(string seedphrase, out string message)
        {
                if ((seedphrase.Length > 128) || (seedphrase.Length < 8))
                {
                    message = "PGP seedphrase must be between 8 and 128 character.";
                    DebugMessage(message, MessageType.Warning);
                    return false;
                }
                else
                {
                
                // Determine unique file names
                string publicKeyFile = Path.Combine(KeysDirectory, "public.asc");
                string privateKeyFile = Path.Combine(KeysDirectory, "private.asc");
                int index = 1;
                while (File.Exists(publicKeyFile) && File.Exists(privateKeyFile))
                {
                    publicKeyFile = Path.Combine(KeysDirectory, $"public({index}).asc");
                    privateKeyFile = Path.Combine(KeysDirectory, $"private({index}).asc");
                    index++;
                }

                // Create new FileInfo objects to coorespond with the public/private key file paths
                FileInfo pubKeyPath = new FileInfo(publicKeyFile);
                FileInfo privKeyPath = new FileInfo(privateKeyFile);

                    // Generate new PGP key pair
                    // Derive Passphrase from Seed Phrase (Important for Security)
                    using (Rfc2898DeriveBytes pbkdf2 = new Rfc2898DeriveBytes(seedphrase,
                               salt: hashSalt, iterations: 10000))
                    {
                        string passphrase = Convert.ToBase64String(pbkdf2.GetBytes(32)); // 32 bytes for a strong key

                    try {
                        // Generate Keys 
                        localPGP.GenerateKey(pubKeyPath, privKeyPath, username, passphrase, emitVersion: false);
                    }
                    catch (Exception ex) {
                        message = "PGP key generation failed: " + ex.Message;
                        DebugMessage(message, MessageType.Critical);
                        return false;
                    }

                        // load all keys into memory
                        LoadPGPKeysFromDirectory();

                        // Open passphrase in an instance of Notepad for user to view/save/write down
                        string tempFile = Path.GetTempFileName();
                        File.WriteAllText(tempFile, passphrase);
                        DebugMessage("The message about to open in Notepad is your private key's passphrase. You will need to hold on to it in order to sign and encrypt messages. It is recommended you write it down, then make sure the temporary file was deleted! (deletion should be automatic, but always double check!)");
                        // Start Notepad process
                        Process notepad = new Process();
                        notepad.StartInfo.FileName = "notepad.exe";
                        notepad.StartInfo.RedirectStandardInput = true;
                        notepad.StartInfo.Arguments = tempFile;
                        notepad.StartInfo.UseShellExecute = false;
                        notepad.Start();
                        Thread.Sleep(2000);
                        File.Delete(tempFile); // Delete temporary file
                        message = $"PGP key pair generated successfully. Save this passphrase:{passphrase}";
                        DebugMessage("PGP key pair generated successfully.", MessageType.General);
                        return true;
                    }

                }
            

        }

        public static void LoadPGPKeysFromDirectory()
        {
            // We will locate every *.asc key in the keysDirectory folder, and for each key found we will
            // create an instance of PGPKeyInfo, then add it to _pgpKeys
            _pgpKeys.Clear(); // Clear existing keys

            // Get all *.asc files
            var keyFiles = Directory.GetFiles(KeysDirectory, "*.asc");

            // Read key data and add to the list
            foreach (var file in keyFiles)
            {
                var keyData = File.ReadAllBytes(file);
                var keyName = Path.GetFileNameWithoutExtension(file);
                _pgpKeys.Add(new PGPKeyInfo { Name = keyName, KeyData = keyData });
            }

        }

        public static Task<bool> ClearSignString(ref string message)
        {
            if (!PrivateKeySet)
            {
                DebugMessage( "No private key set.", MessageType.Warning);
                return Task.FromResult(false);
            }

            bool flawless_ = true;

            try
            {
                    string prvkey = Encoding.UTF8.GetString(ActiveKeyPair.Private.KeyData);

                    EncryptionKeys encyptionKeys = new EncryptionKeys(prvkey, passphrase);

                    PGP temppgp = new PGP(encyptionKeys);

                    string msgout = temppgp.ClearSign(message);

                    message = msgout;
                }
                catch
                {
                    flawless_ = false;
                }
                finally
                {

                    if (flawless_ == true)
                    {
                        // message signed
                    }
                }
                return Task.FromResult(false);

        }

    }

}
  