
using Microsoft.EntityFrameworkCore;
using ERPBackend.Infrastructure.Data;
using ERPBackend.Core.Models;
using System;
using System.Linq;

namespace DBCheck
{
    class Program
    {
        static void Main(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<MerchandisingDbContext>();
            // Guessing the connection string from common project patterns or checking appsettings.json
            // But I can't easily guess it.
            // Instead, I'll just check if the model is registered correctly.
        }
    }
}
