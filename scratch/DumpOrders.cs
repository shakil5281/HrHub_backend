
using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using ERPBackend.Infrastructure.Data;
using ERPBackend.Core.Models;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace DBChecker
{
    class Program
    {
        static void Main(string[] args)
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonOptions(options => { }) // dummy
                .Build();
            
            // Hardcoding connection string for quick check
            var connectionString = "Server=unity3\\SQLEXPRESS;Database=merchandising;User Id=sa;Password=123580;Encrypt=true;TrustServerCertificate=True;MultipleActiveResultSets=true";
            
            var optionsBuilder = new DbContextOptionsBuilder<MerchandisingDbContext>();
            optionsBuilder.UseSqlServer(connectionString);

            using (var context = new MerchandisingDbContext(optionsBuilder.Options))
            {
                var orders = context.StyleOrders.ToList();
                Console.WriteLine($"Total StyleOrders: {orders.Count}");
                foreach(var o in orders) {
                    Console.WriteLine($"ID: {o.Id}, PO: {o.PONumber}, Company: {o.CompanyId}");
                }
            }
        }
    }
}
