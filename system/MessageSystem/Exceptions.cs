using System;
using System.Collections.Generic;
using System.Text;

namespace Robocup.MessageSystem
{
    public class ConnectionRefusedException : ApplicationException
    {
        public ConnectionRefusedException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
