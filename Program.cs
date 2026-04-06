using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using MVCDHProject.Models;

namespace MVCDHProject
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews(configure =>
            {
                //application-level authantication
               // var policy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();
                //configure.Filters.Add(new AuthorizeFilter(policy));
            }).AddRazorRuntimeCompilation();

            builder.Services.AddDbContext<MVCCoreDbContext>(options =>
            {
                options.UseSqlServer(builder.Configuration.GetConnectionString("ConStr"));
            });

            //builder.Services.AddScoped<ICustomerDAL,CustomerXmlDAL>();
            builder.Services.AddScoped<ICustomerDAL, CustomerSqlDAL>();

            builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
            {
                options.Password.RequiredLength = 8;
                //options.Password.RequireDigit = false;//if we don't want digit in password
            }).AddEntityFrameworkStores<MVCCoreDbContext>().AddDefaultTokenProviders();

            builder.Services.AddAuthentication().AddGoogle(options =>
            {
                options.ClientId = builder.Configuration["Authentication:Google:ClientId"]!;
                options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"]!;
            }).AddFacebook(options =>
            {
                options.AppId = builder.Configuration["Authentication:Facebook:AppId"]!;
                options.AppSecret = builder.Configuration["Authentication:Facebook:AppSecret"]!;
            });

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                //Handle Client-Side Error we have three middlewares
                //app.UseStatusCodePages();
                //app.UseStatusCodePagesWithRedirects("/ClientError/{0}");
                app.UseStatusCodePagesWithReExecute("/ClientError/{0}");

                //app.UseExceptionHandler("/Home/Error");
                app.UseExceptionHandler("/ServerError");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.

                app.UseHsts();
            }

           
            app.UseHttpsRedirection();
            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            //app.MapStaticAssets();
            app.UseStaticFiles();
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}")
                .WithStaticAssets();

            app.Run();
        }
    }
}
