using System.Data;
using MVCDHProject.Models;

namespace MVCDHProject.Models
{
    public class CustomerXmlDAL : ICustomerDAL
    {
        DataSet ds;

        public CustomerXmlDAL()
        {
            ds = new DataSet();
            ds.ReadXml("Customers.xml");

            ds.Tables[0].PrimaryKey = new DataColumn[] { ds.Tables[0].Columns["Custid"] };
        }

        public List<CustomerModel> Customers_Select()
        {
            List<CustomerModel> customers = new List<CustomerModel>();

            foreach (DataRow dr in ds.Tables[0].Rows)
            {
                CustomerModel cust = new CustomerModel
                {
                    Custid = Convert.ToInt32(dr["Custid"]),
                    Name = Convert.ToString(dr["Name"]),
                    Balance = Convert.ToDecimal(dr["Balance"]),
                    City = Convert.ToString(dr["City"]),
                    Status = Convert.ToBoolean(dr["Status"])
                };
                customers.Add(cust);
            }
            return customers;
        }

        public CustomerModel Customer_Select(int Custid)
        {
            DataRow? dr = ds.Tables[0].Rows.Find(Custid);

            CustomerModel cust = new CustomerModel
            {
                Custid = Convert.ToInt32(dr?["Custid"]),
                Name = Convert.ToString(dr?["Name"]),
                Balance = Convert.ToDecimal(dr?["Balance"]),
                City = Convert.ToString(dr?["City"]),
                Status = Convert.ToBoolean(dr?["Status"])
            };

            return cust;
        }



        public void Customer_Insert(CustomerModel customer)
        {
            DataRow dr = ds.Tables[0].NewRow();

            dr["Custid"] = customer.Custid;
            dr["Name"] = customer.Name;
            dr["Balance"] = customer.Balance;
            dr["City"] = customer.City;
            dr["Status"] = customer.Status;

            ds.Tables[0].Rows.Add(dr);
            //Saving data back to XML file 
            ds.WriteXml("Customers.xml");
        }


        public void Customer_Update(CustomerModel customer)
        {
            DataRow? dr = ds.Tables[0].Rows.Find(customer.Custid);

            int Index = ds.Tables[0].Rows.IndexOf(dr);

            ds.Tables[0].Rows[Index]["Name"] = customer.Name;
            ds.Tables[0].Rows[Index]["Balance"] = customer.Balance;
            ds.Tables[0].Rows[Index]["City"] = customer.City;

            ds.WriteXml("Customers.xml");
        }


        public void Delete_Customer(int Custid) 
        {
            DataRow? dr = ds.Tables[0].Rows.Find(Custid);

            int Index = ds.Tables[0].Rows.IndexOf(dr);

            ds.Tables[0].Rows[Index].Delete();

            ds.WriteXml("Customers.xml");
        }
    }
}
