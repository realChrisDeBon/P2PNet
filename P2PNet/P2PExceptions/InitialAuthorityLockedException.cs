using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace P2PNet.Exceptions
{
    /// <summary>
    /// Represents an exception thrown when the initial bootstrap authority is locked in and another action conflicts with the initial locked in authority.
    /// </summary>
    public class InitialAuthorityLockedException : Exception
    {
        public InitialAuthorityLockedException()
        {
        }
        public InitialAuthorityLockedException(string message)
            : base(message)
        {
        }
        public InitialAuthorityLockedException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
