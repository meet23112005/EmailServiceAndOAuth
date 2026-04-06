
namespace MVCDHProject.Models
{
    public class CustomerSqlDAL : ICustomerDAL
    {
        private readonly MVCCoreDbContext dc;

        public CustomerSqlDAL(MVCCoreDbContext dc)
        {
            this.dc = dc;
        }

        public List<CustomerModel> Customers_Select()
        {
            return dc.Customers.Where(C => C.Status == true).ToList();

        }

        public CustomerModel Customer_Select(int Custid)
        {
            return dc.Customers.Find(Custid);
        }
       

        public void Customer_Insert(CustomerModel customer)
        {
            dc.Customers.Add(customer);
            dc.SaveChanges();
        }

        public void Customer_Update(CustomerModel customer)
        {

            customer.Status = true;
            dc.Update(customer);
            dc.SaveChanges();
        }

        public void Delete_Customer(int Custid)
        {
            CustomerModel customer = dc.Customers.Find(Custid);
            customer.Status = false;
            dc.SaveChanges();
        }
    }
}
