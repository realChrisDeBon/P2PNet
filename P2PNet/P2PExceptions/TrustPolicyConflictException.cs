using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace P2PNet.Exceptions
{
    /// <summary>
    /// Represents an exception thrown when a trust policy conflict occurs. Exception is thrown by the following classes:
    /// <list type="bullet">
    /// <item>
    /// <term>IncomingPeerTrustPolicy</term>
    /// <description>Handles trust and permissions in regards to incoming peer connections.</description>
    /// </item>
    /// <item>
    /// <term>BootstrapTrustPolicy</term>
    /// <description>Handles trust and permissions in regards to bootstrap connections.</description>
    /// </item>
    /// </summary>
    public class TrustPolicyConflictException : Exception
    {
        public TrustPolicyConflictException()
        {
        }
        public TrustPolicyConflictException(string message)
            : base(message)
        {
        }
        public TrustPolicyConflictException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
