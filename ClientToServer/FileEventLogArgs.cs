using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientToServer.Data
{
    enum FileLogEventType
    {
        BusyStatusChanged,
        LogFileGenerated,
        LogFileGeneratedAtDisable,
        LogFileGeneratedAtSuspend,
        LoggingEnabledDisabled
    }

    class FileLogEventArgs : EventArgs
    {
        public FileLogEventArgs(FileLogEventType type)
        {
            Type = type;
        }

        public FileLogEventArgs(FileLogEventType type, string logFilePath)
        {
            Type = type;
            LogFilePath = logFilePath;
        }

        public FileLogEventArgs(bool enabled)
        {
            Type = FileLogEventType.LoggingEnabledDisabled;
            Enabled = enabled;
        }

        public FileLogEventType Type { get; private set; }

        public String LogFilePath { get; private set; }

        public bool Enabled { get; private set; }
    }
}
