using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClientToServer.Data;
using Windows.Storage;

namespace ClientToServer.Data
{
    class Utilities
    {
        private int CustomerIndex;
        private string file = "ClientToServerData.bin";
        ClientEventSource avs = new ClientEventSource();
        internal FileEventLog Log { get { return FileEventLog.Instance; } }

        public async void Save(ObservableCollection<ServiceReference1.Customer> items)
        {
            ClientEventSource.Log.SaveData(file);
            StorageFolder localFolder = ApplicationData.Current.LocalFolder;
            Stream s = await localFolder.OpenStreamForWriteAsync(file, CreationCollisionOption.ReplaceExisting);
            BinaryWriter bw = new BinaryWriter(s);
            foreach (ServiceReference1.Customer a in items)
            {
                bw.Write((a.CustomerID.ToString() == null) ? "NA" : a.CustomerID.ToString());
                bw.Write((a.CompanyName == null) ? "NA" : a.CompanyName);
                bw.Write((a.ContactName == null) ? "NA" : a.ContactName);
                bw.Write((a.ContactTitle == null) ? "NA" : a.ContactTitle);
                bw.Write((a.Address == null) ? "NA" : a.Address);
                bw.Write((a.City == null) ? "NA" : a.City);
                bw.Write((a.Region == null) ? "NA" : a.Region);
                bw.Write((a.PostalCode == null) ? "NA" : a.PostalCode);
                bw.Write((a.Country == null) ? "NA" : a.Country);
                bw.Write((a.Phone == null) ? "NA" : a.Phone);
                bw.Write((a.Fax == null) ? "NA" : a.Fax);
            }
            bw.Flush();
            ClientEventSource.Log.CompletedSaveData(file);
        }

        public async void Load(ObservableCollection<ServiceReference1.Customer> items)
        {
            ClientEventSource.Log.LoadData(file);
            try
            {
                StorageFolder localFolder = ApplicationData.Current.LocalFolder;
                StorageFile f = await localFolder.CreateFileAsync(file, CreationCollisionOption.OpenIfExists);
                BinaryReader br = new BinaryReader(await f.OpenStreamForReadAsync());
                Guid id;
                while (br.BaseStream.Length > br.BaseStream.Position)
                {
                    items.Add(new ServiceReference1.Customer()
                    {
                        CustomerID = br.ReadString(),
                        CompanyName = br.ReadString(),
                        ContactName = br.ReadString(),
                        ContactTitle = br.ReadString(),
                        Address = br.ReadString(),
                        City = br.ReadString(),
                        Region = br.ReadString(),
                        PostalCode = br.ReadString(),
                        Country = br.ReadString(),
                        Phone = br.ReadString(),
                        Fax = br.ReadString()
                    });
                }
            }
            catch (Exception e)
            {
                Debug.Write(e.Message);
            }
            ClientEventSource.Log.CompletedLoadData(file);
        }

        public void Sort(ObservableCollection<SortedCustomer> items)
        {
            if (items.Count > 1)
            {
                QuickSort(items, 0, items.Count - 1);
            }
            else
            {
                Debug.WriteLine("Not enough to  Sort");
            }
        }

        private void QuickSort(ObservableCollection<SortedCustomer> elements, int left, int right)
        {
            int i = left, j = right;
            IComparable pivot = elements[(left + right) / 2];
            while (i <= j)
            {
                while (elements[i].CompareTo(pivot) < 0)
                {
                    i++;
                }

                while (elements[j].CompareTo(pivot) > 0)
                {
                    j--;
                }


                if (i <= j)
                {

                    SortedCustomer tmp = elements[i];
                    elements[i] = elements[j];
                    elements[j] = tmp;

                    i++;
                    j--;
                }
            }
            if (left < j)
            {
                QuickSort(elements, left, j);
            }
            if (i > right)
            {
                QuickSort(elements, i, right);
            }
        }
    }
}
