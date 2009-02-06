using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Robocup.Core {
    public interface ILogger {

        string LogFile {
            get;
            set;
        }

        bool Logging {
            get;
        }

        void StartLogging();
        void StopLogging();

    }
}
