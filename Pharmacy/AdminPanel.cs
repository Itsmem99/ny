using Microsoft.EntityFrameworkCore;
using Pharmacy;
using Pharmacy.Models;
using System;
using System.Linq;

public class AdminPanel
{
    private PharmacyDbContext _db;

    public AdminPanel(PharmacyDbContext db)
    {
        _db = db;
    }

    public void RunAdminPanel()
    {
        bool running = true;
        while (running)
        {
            Console.WriteLine("\n--- ADMIN PANEL ---");
            Console.WriteLine("1. Visa alla produkter");
            Console.WriteLine("2. Lägg till ny kategori");
            Console.WriteLine("3. Lägg till ny produkt");
            Console.WriteLine("4. Ta bort en produkt");
            Console.WriteLine("5. Uppdatera en produkt");
            Console.WriteLine("6. Visa kundbeställningar");
            Console.WriteLine("7. Återgå");

            if (!int.TryParse(Console.ReadLine(), out int choice))
            {
                Console.WriteLine("Ogiltigt val. Försök igen.");
                continue;
            }

            switch (choice)
            {
                case 1:
                    ShowProducts();
                    break;
                case 2:
                    AddCategory();
                    break;
                case 3:
                    AddProduct();
                    break;
                case 4:
                    RemoveProduct();
                    break;
                case 5:
                    UpdateProduct();
                    break;
                case 6:
                    ShowOrders();
                    break;
                case 7:
                    running = false;
                    Console.WriteLine("Stänger adminpanelen...");
                    break;
                default:
                    Console.WriteLine("Ogiltigt val. Försök igen.");
                    break;
            }
        }
    }

    private void ShowProducts()
    {
        Console.WriteLine("\n--- Produkter ---");
        // Hämtar alla produkter inklusive kategori
        var products = _db.Products.Include(p => p.Category).ToList();

        if (!products.Any())
        {
            Console.WriteLine("Inga produkter tillgängliga.");
            return;
        }

        // Skriv ut varje produkt med tillhörande kategori
        foreach (var product in products)
        {
            string categoryName = product.Category?.Name ?? "Ingen kategori"; // Om ingen kategori finns, visa "Ingen kategori"
            Console.WriteLine($"ID: {product.Id}, Namn: {product.Name}, Pris: {product.Price} SEK, Kategori: {categoryName}");
        }
    }

    private void AddCategory()
    {
        Console.Write("Ange namn på den nya kategorin: ");
        string categoryName = Console.ReadLine()?.Trim();

        if (string.IsNullOrWhiteSpace(categoryName))
        {
            Console.WriteLine("Kategorinamn får inte vara tomt.");
            return;
        }

        if (_db.Categories.Any(c => c.Name ==(categoryName)))
        {
            Console.WriteLine("Denna kategori finns redan.");
            return;
        }

        var newCategory = new Category { Name = categoryName };
        _db.Categories.Add(newCategory);
        _db.SaveChanges();
        Console.WriteLine($"Kategorin '{categoryName}' har lagts till.");
    }

    private void AddProduct()
    {
        // Kontrollera att det finns kategorier först
        var categories = _db.Categories.ToList();
        if (!categories.Any())
        {
            Console.WriteLine("Det finns inga tillgängliga kategorier. Lägg till en kategori först.");
            return;
        }

        Console.Write("Ange produktens namn: ");
        string name = Console.ReadLine()?.Trim();

        Console.Write("Ange pris: ");
        if (!decimal.TryParse(Console.ReadLine(), out decimal price) || price < 0)
        {
            Console.WriteLine("Ogiltigt prisformat. Ange ett positivt decimaltal.");
            return;
        }

        Console.WriteLine("Tillgängliga kategorier:");
        foreach (var category1 in categories)
        {
            Console.WriteLine($"ID: {category1.Id}, Namn: {category1.Name}");
        }

        Console.Write("Ange kategori-ID: ");
        if (!int.TryParse(Console.ReadLine(), out int categoryId))
        {
            Console.WriteLine("Ogiltigt kategori-ID. Försök igen.");
            return;
        }

        var category = _db.Categories.Find(categoryId);
        if (category == null)
        {
            Console.WriteLine("Kategorin hittades inte.");
            return;
        }

        Product newProduct = new Product { Name = name, Price = price, Category = category };
        _db.Products.Add(newProduct);
        _db.SaveChanges();

        Console.WriteLine($"Produkten '{name}' har lagts till i kategorin '{category.Name}'.");
    }

    private void RemoveProduct()
    {
        Console.Write("Ange ID på produkten du vill ta bort: ");
        if (!int.TryParse(Console.ReadLine(), out int productId))
        {
            Console.WriteLine("Ogiltigt ID. Försök igen.");
            return;
        }

        var product = _db.Products.Find(productId);
        if (product == null)
        {
            Console.WriteLine("Produkten hittades inte.");
            return;
        }

        _db.Products.Remove(product);
        _db.SaveChanges();
        Console.WriteLine($"Produkten {product.Name} har tagits bort.");
    }

    private void UpdateProduct()
    {
        Console.Write("Ange ID på produkten du vill uppdatera: ");
        if (!int.TryParse(Console.ReadLine(), out int productId))
        {
            Console.WriteLine("Ogiltigt ID. Försök igen.");
            return;
        }

        var product = _db.Products.Find(productId);
        if (product == null)
        {
            Console.WriteLine("Produkten hittades inte.");
            return;
        }

        Console.Write("Ange nytt namn (eller tryck Enter för att behålla nuvarande): ");
        string newName = Console.ReadLine();
        if (!string.IsNullOrWhiteSpace(newName))
        {
            product.Name = newName;
        }

        Console.Write("Ange nytt pris (eller tryck Enter för att behålla nuvarande): ");
        string newPriceInput = Console.ReadLine();
        if (decimal.TryParse(newPriceInput, out decimal newPrice))
        {
            product.Price = newPrice;
        }

        _db.SaveChanges();
        Console.WriteLine("Produkten har uppdaterats.");
    }

    private void ShowOrders()
    {
        Console.WriteLine("\n--- Kundbeställningar ---");
        var orders = _db.Orders
            .Include(o => o.Customer)
            .Include(o => o.OrderItems)
            .ToList();

        if (!orders.Any())
        {
            Console.WriteLine("Inga beställningar har gjorts ännu.");
            return;
        }

        foreach (var order in orders)
        {
            Console.WriteLine($"Order-ID: {order.Id}, Kund: {order.Customer?.Name ?? "Okänd"}, Adress: {order.Customer?.Adress ?? "Ingen adress"}, Beställd: {order.OrderDate}");
            Console.WriteLine("Produkter:");
            foreach (var item in order.OrderItems)
            {
                Console.WriteLine($" - {item.Product.Name}: {item.Quantity} st x {item.Product.Price} SEK");
            }
            Console.WriteLine("-----------------------------------");
        }
    }
}
