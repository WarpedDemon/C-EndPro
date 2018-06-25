using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using System.Threading.Tasks;

namespace ServerService
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "Service1" in code, svc and config file together.
    // NOTE: In order to launch WCF Test Client for testing this service, please select Service1.svc or Service1.svc.cs at the Solution Explorer and start debugging.
    public class Service1 : IService1
    {
        public async Task<List<Customer>> GetCustomersAsync()
        {
            NorthwindEntities dbContext = new NorthwindEntities();
            DbSet<Customer> Customers = dbContext.Customers;
            return await Customers.ToListAsync();
        }

        public async Task<List<Customer>> GetCustomerAsync(string contactName)
        {

            NorthwindEntities dbContext = new NorthwindEntities();
            DbSet<Customer> Customers = dbContext.Customers;
            List<Customer> list = await Customers.Where(p => p.ContactName == contactName).ToListAsync();

            return list;
        }

        public async Task<List<Customer>> GetCustomerOrders(string CustomerID)
        {

            NorthwindEntities dbContext = new NorthwindEntities();
            DbSet<Customer> Customers = dbContext.Customers;
            List<Customer> list = await Customers.Where(p => p.CustomerID == CustomerID).ToListAsync();

            return await Customers.SqlQuery("SELECT t1.ContactName, t1.CompanyName, t1.Phone, t4.ProductName, t3.UnitPrice," +
                " t3.Quantity, t3.Discount FROM Customers as t1 INNER JOIN Orders AS t2 ON t1.CustomerID = t2.CustomerID" +
                " INNER JOIN dbo.[Order Details] as t3 on t2.OrderID = t3.OrderID INNER JOIN Products as t4 ON t3.ProductID" +
                " = t4.ProductID WHERE t1.CustomerID = " + CustomerID).ToListAsync();
        }
    }
}
