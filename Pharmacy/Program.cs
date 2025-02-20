using Microsoft.EntityFrameworkCore;
using Pharmacy;
using Pharmacy.Models;
using System;
using System.Linq;
using System.Runtime.InteropServices;

class Program
{
    static void Main (string[] args)
    {
        try
       {
            using (var db = new PharmacyDbContext())
            {
                bool running = true;

                // Ensure the database exists
                db.Database.EnsureCreated();

                while (running)
                {
                    Console.WriteLine("\nVälkommen till ApotekApp!");
                    Console.WriteLine("1. Admin");
                    Console.WriteLine("2. Kund");
                    Console.WriteLine("3. Registrera kund");
                    Console.WriteLine("4. Avsluta");

                    string input = Console.ReadLine();

                    if (int.TryParse(input, out int choice))
                    {
                        switch (choice)
                        {
                            case 1:
                                HandleAdminLogin(db);
                                break;

                            case 2:
                                HandleCustomerView(db);
                                break;

                            case 3:
                                RegisterCustomer(db);
                                break;

                            case 4:
                                running = false;
                                Console.WriteLine("Avslutar applikationen...");
                                break;

                            default:
                                Console.WriteLine("Ogiltigt val. Försök igen.");
                                break;
                        }
                    }
                    else
                    {
                        Console.WriteLine("Ogiltigt val. Försök igen.");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ett fel inträffade: {ex.Message}");
        }
    }

    // Admin login handler
    static void HandleAdminLogin(PharmacyDbContext db)
    {
        if (AdminLogin())
        {
            AdminPanel adminPanel = new AdminPanel(db);
            adminPanel.RunAdminPanel();
        }
        else
        {
            Console.WriteLine("Fel användarnamn eller lösenord. Försök igen.");
        }
    }
    // Customer view handler
    static void HandleCustomerView(PharmacyDbContext db)
    {
        // Hämta alla produkter där priset är större än 0 och är ordnade efter kategori
        var products = db.Products.Where(p => p.Price > 0)
                                  .Include(p => p.Category)  // Ladda kategori för varje produkt
                                  .ToList();

        // Kolla om inga produkter finns tillgängliga
        if (products.Count == 0)
        {
            Console.WriteLine("Inga produkter tillgängliga i butiken.");
            return;
        }

        // Visa produkter kategoriserade
        var categories = db.Categories.ToList();
        foreach (var category in categories)
        {
            Console.WriteLine($"\nKategori: {category.Name}");
            var categoryProducts = products.Where(p => p.CategoryId == category.Id).ToList();

            foreach (var product in categoryProducts)
            {
                Console.WriteLine($"  Produkt: {product.Name}, Pris: {product.Price} SEK");
            }
        }

        // Starta kundvyn (CartItem) - här kommer produkterna och menyalternativen att visas
        CartItem customerView = new CartItem(db);
        customerView.RunVarkorgApp();
    }



    // Admin login verification
    static bool AdminLogin()
    {
        string adminUsername = "admin";
        string adminPassword = "pass123";

        Console.Write("Ange användarnamn: ");
        string username = Console.ReadLine().Trim();
        Console.Write("Ange lösenord: ");
        string password = Console.ReadLine().Trim();

        return username.Equals(adminUsername, StringComparison.OrdinalIgnoreCase) && password == adminPassword;
    }

    // Customer registration

    static void RegisterCustomer(PharmacyDbContext db)
    {
        Console.WriteLine("Ange kundens namn:");
        string name = Console.ReadLine();

        Console.WriteLine("Ange kundens adress:");
        string address = Console.ReadLine(); ;

        // Skapa en ny kund
        Customer newCustomer = new Customer
        {
            Name = name,
            Adress = address
        };

        // Lägg till den nya kunden i databasen
        db.Add(newCustomer);
        db.SaveChanges();


        Console.WriteLine("Kund registrerad!");
    }
}