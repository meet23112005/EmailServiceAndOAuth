namespace MVCDHProject.Models
{
    public interface ICustomerDAL
    {
        public List<CustomerModel> Customers_Select();
        public CustomerModel Customer_Select(int Custid);
        public void Customer_Insert(CustomerModel customer);
        public void Customer_Update(CustomerModel customer);
        public void Delete_Customer(int Custid);
    }
}
