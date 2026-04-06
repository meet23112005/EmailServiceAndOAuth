using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace MVCDHProject.Models
{
    public class MVCCoreDbContext : IdentityDbContext
    {
        public MVCCoreDbContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<CustomerModel> Customers { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<CustomerModel>().HasData(
                new CustomerModel { Custid = 101, Name = "Sai", Balance = 50000.00m, City = "Delhi", Status = true },
                new CustomerModel { Custid = 102, Name = "Sonia", Balance = 40000.00m, City = "Mumbai", Status = true },
                new CustomerModel { Custid = 103, Name = "Pankaj", Balance = 30000.00m, City = "Chennai", Status = true },
                new CustomerModel { Custid = 104, Name = "Samuels", Balance = 25000.00m, City = "Bengaluru", Status = true }
                );
        }
    }
}
