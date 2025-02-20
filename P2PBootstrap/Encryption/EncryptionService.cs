using PgpCore;
using System.Collections;
using Org.BouncyCastle.Pqc.Crypto.Lms;
using System.IO;
using P2PBootstrap.Encryption.Pgp;
using P2PBootstrap.CLI.Command;
using System.Runtime.CompilerServices;

namespace P2PBootstrap.Encryption
    {
    public static class EncryptionService
        {
            public static void Initialize()
            {
                PgpService.Initialize();
            }

            public static void GenNewPGPKey(string input, ref CommandResponse commandResponse)
            {
                bool valid = PgpService.GeneratePGPKeyPair(input, out string message);
                commandResponse.Response = message;
                commandResponse.Success = valid;
            }

        }
    }