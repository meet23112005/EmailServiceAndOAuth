using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using Microsoft.Extensions.Configuration;

namespace MVCDHProject.Models
{
    public class CustomerExcelDAL : ICustomerDAL
    {
        private readonly string _conStr;

        public CustomerExcelDAL(IConfiguration config)
        {
            // Make sure the Excel file is outside wwwroot and has full read/write access
            string root = System.IO.Directory.GetCurrentDirectory();
            string filePath = System.IO.Path.Combine(root, config["ExcelFilePath"]); // e.g., "Data/Customers.xlsx"

            _conStr = $"Provider=Microsoft.ACE.OLEDB.12.0;Data Source={filePath};Extended Properties='Excel 12.0 Xml;HDR=YES;'";
        }

        public List<CustomerModel> Customers_Select()
        {
            List<CustomerModel> list = new List<CustomerModel>();
            DataTable dt = new DataTable();

            using (OleDbConnection con = new OleDbConnection(_conStr))
            {
                con.Open();
                string q = "SELECT * FROM [Customers$] WHERE IsDeleted=0 OR IsDeleted IS NULL";

                using (OleDbCommand cmd = new OleDbCommand(q, con))
                using (OleDbDataReader dr = cmd.ExecuteReader())
                {
                    dt.Load(dr);
                }
            }

            foreach (DataRow r in dt.Rows)
            {
                string s = r["Status"].ToString().Trim();
                bool status = s == "1" || s == "-1" || s.Equals("true", StringComparison.OrdinalIgnoreCase);

                list.Add(new CustomerModel
                {
                    Custid = Convert.ToInt32(r["Custid"]),
                    Name = Convert.ToString(r["Name"]),
                    Balance = r["Balance"] != DBNull.Value ? Convert.ToDecimal(r["Balance"]) : 0m,
                    City = Convert.ToString(r["City"]),
                    Status = status
                });
            }

            return list;
        }

        public CustomerModel Customer_Select(int Custid)
        {
            DataTable dt = new DataTable();
            using (OleDbConnection con = new OleDbConnection(_conStr))
            {
                con.Open();
                string q = "SELECT * FROM [Customers$] WHERE Custid=@id AND (IsDeleted=0 OR IsDeleted IS NULL)";

                using (OleDbCommand cmd = new OleDbCommand(q, con))
                {
                    cmd.Parameters.AddWithValue("@id", Custid);
                    using (OleDbDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }
                }
            }

            if (dt.Rows.Count == 0) return null;

            DataRow r = dt.Rows[0];
            string s = r["Status"].ToString().Trim();
            bool status = s == "1" || s == "-1" || s.Equals("true", StringComparison.OrdinalIgnoreCase);

            return new CustomerModel
            {
                Custid = Convert.ToInt32(r["Custid"]),
                Name = Convert.ToString(r["Name"]),
                Balance = r["Balance"] != DBNull.Value ? Convert.ToDecimal(r["Balance"]) : 0m,
                City = Convert.ToString(r["City"]),
                Status = status
            };
        }

        public void Customer_Insert(CustomerModel customer)
        {
            using (OleDbConnection con = new OleDbConnection(_conStr))
            {
                con.Open();
                string q = @"INSERT INTO [Customers$] 
                             (Custid, Name, Balance, City, Status, IsDeleted)
                             VALUES (@id, @name, @bal, @city, @status, 0)";

                using (OleDbCommand cmd = new OleDbCommand(q, con))
                {
                    cmd.Parameters.AddWithValue("@id", customer.Custid);
                    cmd.Parameters.AddWithValue("@name", customer.Name ?? string.Empty);
                    cmd.Parameters.AddWithValue("@bal", customer.Balance ?? 0m);
                    cmd.Parameters.AddWithValue("@city", customer.City ?? string.Empty);

                    int excelStatus = customer.Status ? -1 : 0;
                    cmd.Parameters.AddWithValue("@status", excelStatus);

                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void Customer_Update(CustomerModel customer)
        {
            using (OleDbConnection con = new OleDbConnection(_conStr))
            {
                con.Open();
                string q = @"UPDATE [Customers$] 
                             SET Name=@name, Balance=@bal, City=@city, Status=@status
                             WHERE Custid=@id";

                using (OleDbCommand cmd = new OleDbCommand(q, con))
                {
                    cmd.Parameters.AddWithValue("@name", customer.Name ?? string.Empty);
                    cmd.Parameters.AddWithValue("@bal", customer.Balance ?? 0m);
                    cmd.Parameters.AddWithValue("@city", customer.City ?? string.Empty);

                    int excelStatus = customer.Status ? -1 : 0;
                    cmd.Parameters.AddWithValue("@status", excelStatus);

                    cmd.Parameters.AddWithValue("@id", customer.Custid);

                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void Delete_Customer(int Custid)
        {
            // Use IsDeleted flag instead of DELETE
            using (OleDbConnection con = new OleDbConnection(_conStr))
            {
                con.Open();
                string q = "UPDATE [Customers$] SET IsDeleted=-1 WHERE Custid=@id";

                using (OleDbCommand cmd = new OleDbCommand(q, con))
                {
                    cmd.Parameters.AddWithValue("@id", Custid);
                    cmd.ExecuteNonQuery();
                }
            }
        }
    }
}
