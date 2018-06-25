using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation.Diagnostics;
using Windows.Storage;
using ClientToServer.Data;

namespace ClientToServer.Data
{
    class FileEventLog : IDisposable
    {
        public string GetTimeStamp()
        {
            DateTime now = DateTime.Now;
            return string.Format(System.Globalization.CultureInfo.InvariantCulture,
                "{0:D4}{1:D2}{2:D2}-{3:D2}{4:D2}{5:D2}{6:D3}",
                now.Year,
                now.Month,
                now.Day,
                now.Hour,
                now.Minute,
                now.Second,
                now.Millisecond);
        }

        private const string Prefix = "ClientToServer_";
        public const string DEFAULT_SESSION_NAME = Prefix + "Session";
        public const string DEFAULT_CHANNEL_NAME = Prefix + "Channel";

        private const string APP_LOG_FILE_FOLDER_NAME = Prefix + "LogFiles";

        private FileLoggingSession session;

        private LoggingChannel channel;

        public void Start()
        {
            if (channel != null)
            {
                channel.LogMessage(String.Format("Message={0}: Client To Server Application Started!", 1), LoggingLevel.Information);
            }
        }

        public void Close()
        {
            if (channel != null)
            {
                channel.LogMessage(String.Format("Message={0}: Client To Server Application Stopped!", 2), LoggingLevel.Information);
            }
        }

        public void LoadData(String file)
        {
            if (channel != null)
            {
                channel.LogMessage(String.Format("Message={0}: Client To Server Loading Data From {1}", 20, file), LoggingLevel.Information);
            }
        }

        public void CompletedLoadData(String file)
        {
            if (channel != null)
            {
                channel.LogMessage(String.Format("Message={0}: Client To Server Loading Data From {1}", 21, file), LoggingLevel.Information);
            }
        }

        public void SaveData(String file)
        {
            if (channel != null)
            {
                channel.LogMessage(String.Format("Message={0}: Client To Server Loaded Data to {1}", 30, file), LoggingLevel.Information);
            }
        }

        public void CompletedSaveData(String file)
        {
            if (channel != null)
            {
                channel.LogMessage(String.Format("Message={0}: Client To Server Saving Data to {1}", 31, file), LoggingLevel.Information);
            }
        }

        public async Task<bool> ToggleLoggingEnabledDisabledAsync()
        {
            CheckDisposed();
            IsBusy = true;

            try
            {
                bool enabled;

                if (session != null)
                {
                    string finalLogFilePath = await CloseSessionSaveFinalLogFile();
                    session.Dispose();
                    session = null;
                    if (StatusChanged != null)
                    {
                        StatusChanged.Invoke(this, new FileLogEventArgs(FileLogEventType.LogFileGeneratedAtDisable, finalLogFilePath));
                    }
                    ApplicationData.Current.LocalSettings.Values[LOGGING_ENABLED_SETTING_KEY_NAME] = false;
                    enabled = false;
                }

                else
                {
                    StartLogging();
                    ApplicationData.Current.LocalSettings.Values[LOGGING_ENABLED_SETTING_KEY_NAME] = true;
                    enabled = true;
                }

                if (StatusChanged != null)
                {
                    StatusChanged.Invoke(this, new FileLogEventArgs(enabled));
                }

                return enabled;
            }
            finally
            {
                IsBusy = false;
            }
        }

        private const string LOGGING_ENABLED_SETTING_KEY_NAME = Prefix + "LoggingEnabled";
        private const string LOGFILEGIEN_BEFORE_SUSPEND_SETTING_KEY_NAME = Prefix + "LogFileGeneratedBeforeSuspend";
        public bool IsPreparingForSuspend { get; private set; }

        public bool IsLoggingEnabled
        {
            get
            {
                return session != null;
            }
        }

        public async Task PreparedToSuspend()
        {
            CheckDisposed();

            if (session != null)
            {
                IsPreparingForSuspend = true;

                try
                {
                    string finalFileBeforeSuspend = await CloseSessionSaveFinalLogFile();
                    session.Dispose();
                    session = null;
                    ApplicationData.Current.LocalSettings.Values[LOGGING_ENABLED_SETTING_KEY_NAME] = true;
                    ApplicationData.Current.LocalSettings.Values[LOGFILEGIEN_BEFORE_SUSPEND_SETTING_KEY_NAME] = finalFileBeforeSuspend;
                }
                finally
                {
                    IsPreparingForSuspend = false;
                }
            }
            else
            {
                ApplicationData.Current.LocalSettings.Values[LOGGING_ENABLED_SETTING_KEY_NAME] = false;
                ApplicationData.Current.LocalSettings.Values[LOGFILEGIEN_BEFORE_SUSPEND_SETTING_KEY_NAME] = null;
            }
        }

        public void ResumeLoggingIfApplicable()
        {
            CheckDisposed();
            object loggingEnabled;

            if (ApplicationData.Current.LocalSettings.Values.TryGetValue(LOGFILEGIEN_BEFORE_SUSPEND_SETTING_KEY_NAME, out loggingEnabled) == false)
            {
                ApplicationData.Current.LocalSettings.Values[LOGGING_ENABLED_SETTING_KEY_NAME] = true;
                loggingEnabled = ApplicationData.Current.LocalSettings.Values[LOGGING_ENABLED_SETTING_KEY_NAME];
            }

            if (loggingEnabled is bool && (bool)loggingEnabled == true)
            {
                StartLogging();
            }

            object LogFileGeneratedBeforeSuspendObject;

            if (ApplicationData.Current.LocalSettings.Values.TryGetValue(LOGFILEGIEN_BEFORE_SUSPEND_SETTING_KEY_NAME, out LogFileGeneratedBeforeSuspendObject) && LogFileGeneratedBeforeSuspendObject != null && LogFileGeneratedBeforeSuspendObject is string)
            {
                if (StatusChanged != null)
                {
                    StatusChanged.Invoke(this, new FileLogEventArgs(FileLogEventType.LogFileGeneratedAtSuspend, (string)LogFileGeneratedBeforeSuspendObject));
                }
                ApplicationData.Current.LocalSettings.Values[LOGFILEGIEN_BEFORE_SUSPEND_SETTING_KEY_NAME] = null;
            }
        }

        public void StartLogging()
        {
            CheckDisposed();

            if (session == null)
            {
                session = new FileLoggingSession(DEFAULT_SESSION_NAME);
                session.LogFileGenerated += LogFileGenerateHandler;
            }
            session.AddLoggingChannel(channel, LoggingLevel.Verbose);
        }

        private async void LogFileGenerateHandler(IFileLoggingSession sender, LogFileGeneratedEventArgs args)
        {
            StorageFolder sampleAppDefinedLogFolder = await ApplicationData.Current.LocalFolder.CreateFolderAsync(APP_LOG_FILE_FOLDER_NAME, CreationCollisionOption.OpenIfExists);
            string newLogFileName = "Log-" + GetTimeStamp() + ".etl";
            await args.File.MoveAsync(sampleAppDefinedLogFolder, newLogFileName);
            if (IsPreparingForSuspend == false)
            {
                if (StatusChanged != null)
                {
                    string newLogFileFullPathName = System.IO.Path.Combine(sampleAppDefinedLogFolder.Path, newLogFileName);
                    StatusChanged.Invoke(this, new FileLogEventArgs(FileLogEventType.LogFileGenerated, newLogFileFullPathName));
                }
            }
        }

        private async Task<string> CloseSessionSaveFinalLogFile()
        {
            StorageFile finalFileBeforeSuspend = await session.CloseAndSaveToFileAsync();
            if (finalFileBeforeSuspend != null)
            {
                StorageFolder AppDefinedLogFolder = await ApplicationData.Current.LocalFolder.CreateFolderAsync(APP_LOG_FILE_FOLDER_NAME, CreationCollisionOption.OpenIfExists);
                string newLogFileName = "Log-" + GetTimeStamp() + ".etl";
                await finalFileBeforeSuspend.MoveAsync(AppDefinedLogFolder, newLogFileName);
                return System.IO.Path.Combine(AppDefinedLogFolder.Path, newLogFileName);
            }
            else
            {
                return null;
            }
        }

        public event EventHandler<FileLogEventArgs> StatusChanged;
        private bool IsBusy;

        public bool isBusy
        {
            get
            {
                return isBusy;
            }

            private set
            {
                isBusy = value;
                if (StatusChanged != null)
                {
                    StatusChanged.Invoke(this, new FileLogEventArgs(FileLogEventType.BusyStatusChanged));
                }
            }
        }

        async void OnAppSuspending(object sender, Windows.ApplicationModel.SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            await PreparedToSuspend();
            deferral.Complete();
        }

        void OnAppResuming(object sender, object e)
        {
            ResumeLoggingIfApplicable();
        }

        static private FileEventLog log;

        static public FileEventLog Instance
        {
            get
            {
                if (log == null)
                {
                    log = new FileEventLog();
                }
                return log;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (isDisposed == false)
            {
                isDisposed = true;
                if (channel != null)
                {
                    channel.Dispose();
                    channel = null;
                }
                if (session != null)
                {
                    session.Dispose();
                    session = null;
                }
            }
        }

        private bool isDisposed = false;

        private void CheckDisposed()
        {
            if (isDisposed)
            {
                throw new ObjectDisposedException("ClientToServer");
            }
        }

        private bool isChannelEnabled = false;

        private LoggingLevel channelLoggingLevel = LoggingLevel.Verbose;

        void OnChannelLoggingEnabled(ILoggingChannel sender, object args)
        {
            isChannelEnabled = sender.Enabled;
            channelLoggingLevel = sender.Level;
        }

        private FileEventLog()
        {
#pragma warning disable CS0618 // Type or member is obsolete
            channel = new LoggingChannel(DEFAULT_CHANNEL_NAME);
#pragma warning restore CS0618 // Type or member is obsolete

            channel.LoggingEnabled += OnChannelLoggingEnabled;

            App.Current.Suspending += OnAppSuspending;
            App.Current.Resuming += OnAppResuming;

            ResumeLoggingIfApplicable();
        }
    }
}
