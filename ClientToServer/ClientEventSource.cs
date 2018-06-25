using ClientToServer.Data;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Diagnostics.Tracing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace ClientToServer
{
    class ClientEventSource : EventSource
    {
        public static ClientEventSource Log = new ClientEventSource();
        public void Start()
        {
            Log.WriteEvent(1);
        }

        public void Close()
        {
            Log.WriteEvent(2);
        }

        public void LoadData(string file)
        {
            Log.WriteEvent(20, file);
        }
        public void CompletedLoadData(string file)
        {
            Log.WriteEvent(21, file);
        }

        public void SaveData(string file)
        {
            Log.WriteEvent(30, file);
        }

        public void CompletedSaveData(string file)
        {
            Log.WriteEvent(31, file);
        }
    }
}
