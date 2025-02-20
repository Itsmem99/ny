using Microsoft.EntityFrameworkCore;
using Pharmacy.Models;
using Pharmacy.Models.Pharmacy.Models;

namespace Pharmacy.Models
{
    public class PharmacyDbContext : DbContext
    {
        public PharmacyDbContext(DbContextOptions<PharmacyDbContext> options)
            : base(options)
        {
            //"Server=mahakhalifa.database.windows.net;Database=mahadb;User Id=Mahax99;Password=Blomma01"

        }

        // Lägg till en parameterlös konstruktor för att stödja migrationer
        public PharmacyDbContext()
        {
        }

        public DbSet<Product> Products { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<Customer> Customers { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                // Korrigerad anslutningssträng med rätt syntax
                optionsBuilder.UseSqlServer("Server=tcp:mahakhalifa.database.windows.net,1433;Initial Catalog=mahax99;Persist Security Info=False;User ID=MAHAX99;Password=Blomma01;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;");
                //optionsBuilder.UseSqlServer("Server=MAHO\\SQLEXPRESS;Database=Apotek1;Integrated Security=True;TrustServerCertificate=True;");
            }
        }
    }
}
