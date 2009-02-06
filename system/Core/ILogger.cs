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

        void StartLogging();
        void StopLogging();

    }
}