using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using ClientToServer.Data;
// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace ClientToServer
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public ObservableCollection<ServiceReference1.Customer> Items { get; set; }
        /// <summary>
        /// ClientToServer.Items
        /// Customers inside service make icomparable with name as the comparable
        /// </summary>
        string Name;
        string Surname;
       // private string fileName = "ClientToServer.bin";
        Utilities util = new Utilities();
        public MainPage()
        {
            util.Log.StatusChanged += Log__StatusChanged;
            this.InitializeComponent();
        }

        private void UpdateControls()
        {
            if (util.Log.IsLoggingEnabled)
            {
                logging.Content = "Stop Logging";
            }
            else
            {
                logging.Content = "Start Logging";
            }
        }

        async void Log__StatusChanged(Object Sender, FileLogEventArgs e)
        {
            if (e.Type == FileLogEventType.BusyStatusChanged)
            {
                UpdateControls();
            }
        }

        private async void StartLogging()
        {
            await util.Log.ToggleLoggingEnabledDisabledAsync();
        }

        private void logging_Click(object sender, RoutedEventArgs e)
        {
            StartLogging();
            UpdateControls();
        }

        public async Task StartupGetData()
        {            
            ServiceReference1.Service1Client client = new ServiceReference1.Service1Client();
            if (client.State != System.ServiceModel.CommunicationState.Opened)
                await client.OpenAsync();
            Items = await client.GetCustomersAsync();
            lstView.ItemsSource = Items;
            await client.CloseAsync();   
        }

        public async Task ViewOrders()
        {
            ServiceReference1.Service1Client client = new ServiceReference1.Service1Client();
            if (client.State != System.ServiceModel.CommunicationState.Opened)
            {
                await client.OpenAsync();
            }

            Name = custNametxtBox.Text;
            Surname = CustSurnametxtBox.Text;

            if (Name == "" && Surname == "")
            {
                await StartupGetData();
            }
            else if (Surname != "" && Name != "")
            {
                string contactName = Name + " " + Surname;

                Items = await client.GetCustomerAsync(contactName);
                lstView.ItemsSource = Items;
                await client.CloseAsync();
            }
        }

        private void sort_Click(object sender, RoutedEventArgs e)
        {
            ObservableCollection<SortedCustomer> sortedList = new ObservableCollection<SortedCustomer>();

            foreach (ServiceReference1.Customer a in Items)
            {
                sortedList.Add(new SortedCustomer()
                {
                    CustomerID = a.CustomerID,
                    CompanyName = a.CompanyName,
                    ContactName = a.ContactName,
                    ContactTitle = a.ContactTitle,
                    Address = a.Address,
                    City = a.City,
                    Region = a.Region,
                    PostalCode = a.PostalCode,
                    Country = a.Country,
                    Phone = a.Phone,
                    Fax = a.Fax
                });
            }

            util.Sort(sortedList);
            Items.Clear();

            foreach (SortedCustomer a in sortedList)
            {
                Items.Add(new ServiceReference1.Customer()
                {
                    CustomerID = a.CustomerID,
                    CompanyName = a.CompanyName,
                    ContactName = a.ContactName,
                    ContactTitle = a.ContactTitle,
                    Address = a.Address,
                    City = a.City,
                    Region = a.Region,
                    PostalCode = a.PostalCode,
                    Country = a.Country,
                    Phone = a.Phone,
                    Fax = a.Fax
                });
            }

            lstView.ItemsSource = null;
            lstView.ItemsSource = Items;
        }

        private void save_Click(object sender, RoutedEventArgs e)
        {
            util.Save(Items);
        }

        private void load_Click(object sender, RoutedEventArgs e)
        {
            lstView.ItemsSource = null;
            util.Load(Items);
        }

        private async void ListView_Loaded(object sender, RoutedEventArgs e)
        {
            await StartupGetData();
        }

        private async void ViewOrdersButton_Click(object sender, RoutedEventArgs e)
        {
            await ViewOrders();
        }
    }
}
