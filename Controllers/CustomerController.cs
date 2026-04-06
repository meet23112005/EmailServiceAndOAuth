using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MVCDHProject.Models;

namespace MVCDHProject.Controllers
{
    [Authorize]
    public class CustomerController : Controller
    {
        //CustomerXmlDAL DAL = new CustomerXmlDAL();//tightly coupled

        ICustomerDAL DAL;

        public CustomerController(ICustomerDAL DAL)
        {
            this.DAL = DAL;
        }

        [AllowAnonymous]
        public ViewResult DisplayCustomers()
        {
            return View(DAL.Customers_Select());
        }

        [AllowAnonymous]
        public ViewResult DisplayCustomer(int Custid)
        {
            var customer = DAL.Customer_Select(Custid);
            if(customer == null)
            {
                throw new Exception("No customer exist's with given Custid.");
            }
            return View(customer);
        }

        [HttpGet]
        public ViewResult AddCustomer()
        {
            return View();
        }

        [HttpPost]
        public RedirectToActionResult AddCustomer(CustomerModel customer)
        {
            DAL.Customer_Insert(customer);
            return RedirectToAction("DisplayCustomers");
        }

        [HttpGet]
        public ViewResult EditCustomer(int Custid)
        {
            var customer = DAL.Customer_Select(Custid);
            if (customer == null)
            {
                throw new Exception("No customer exist's with given Custid.");
            }
            return View(customer);
        }

        [HttpPost]
        public RedirectToActionResult UpdateCustomer(CustomerModel customer)
        {
            DAL.Customer_Update(customer);
            return RedirectToAction("DisplayCustomers");
        }

        public RedirectToActionResult DeleteCustomer(int Custid)
        {
            DAL.Delete_Customer(Custid);
            return RedirectToAction("DisplayCustomers");
        }
    }
}
