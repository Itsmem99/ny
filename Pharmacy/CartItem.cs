using Microsoft.EntityFrameworkCore;
using Pharmacy.Models;
using Pharmacy.Models.Pharmacy.Models;
using System;
using System.Collections.Generic;
using System.Linq;
public class CartItem
{
    private List<Product> _products;
    private List<Product> _cart;
    private PharmacyDbContext _db;

    public CartItem(PharmacyDbContext db)
    {
        _cart = new List<Product>();
        _db = db;
        _products = _db.Products.Include(p => p.Category).ToList();  // Vi laddar alla produkter i bakgrunden
    }

    public void RunVarkorgApp()
    {
        bool shopping = true;

        // Visa välkomsttext för kunden
        Console.WriteLine("\nVälkommen till vår butik! Här hittar du dina favoritprodukter.");

        while (shopping)
        {
            Console.WriteLine("\nVälj ett alternativ:");
            Console.WriteLine("1. Visa produkter efter kategori");
            Console.WriteLine("2. Lägg till produkt i varukorgen");
            Console.WriteLine("3. Ta bort produkt från varukorgen");
            Console.WriteLine("4. Checkout");
            Console.WriteLine("5. Avsluta");

            if (!int.TryParse(Console.ReadLine(), out int choice))
            {
                Console.WriteLine("Ogiltigt val. Försök igen.");
                continue;
            }

            switch (choice)
            {
                case 1:
                    ShowProducts();    // Visa produkter sorterade efter kategori
                    break;
                case 2:
                    AddToCart(); // Lägg till i varukorgen
                    break;
                case 3:
                    RemoveFromCart(); // Ta bort från varukorgen
                    break;
                case 4:
                    Checkout(); // Gå till checkout
                    break;
                case 5:
                    shopping = false;
                    Console.WriteLine("Tack för ditt besök!");
                    break;
                default:
                    Console.WriteLine("Ogiltigt val. Försök igen.");
                    break;
            }
        }
    }

    private void ShowProducts()
    {
        // Visa produkter sorterade efter kategori
        Console.WriteLine("\n--- Produkter sorterade efter kategori ---");

        var groupedProducts = _products
            .GroupBy(p => p.Category != null ? p.Category.Name : "Okänd kategori")
            .ToList();

        if (!groupedProducts.Any())
        {
            Console.WriteLine("Inga produkter tillgängliga.");
            return;
        }

        foreach (var category in groupedProducts)
        {
            Console.WriteLine($"\nKategori: {category.Key}");
            foreach (var product in category)
            {
                Console.WriteLine($"- {product.Name} - {product.Price} SEK");
            }
        }
    }

    private void AddToCart()
    {
        Console.WriteLine("\nVälj en produkt att lägga till i varukorgen:");
        for (int i = 0; i < _products.Count; i++)
        {
            Console.WriteLine($"{i + 1}. {_products[i].Name} - {_products[i].Price} SEK");
        }

        if (int.TryParse(Console.ReadLine(), out int choice) && choice >= 1 && choice <= _products.Count)
        {
            _cart.Add(_products[choice - 1]);
            Console.WriteLine($"{_products[choice - 1].Name} har lagts till i varukorgen.");
        }
        else
        {
            Console.WriteLine("Ogiltigt val. Försök igen.");
        }
    }

    private void RemoveFromCart()
    {
        if (_cart.Count == 0)
        {
            Console.WriteLine("\nVarukorgen är tom.");
            return;
        }

        Console.WriteLine("\nVälj en produkt att ta bort från varukorgen:");
        for (int i = 0; i < _cart.Count; i++)
        {
            Console.WriteLine($"{i + 1}. {_cart[i].Name} - {_cart[i].Price} SEK");
        }

        if (int.TryParse(Console.ReadLine(), out int choice) && choice >= 1 && choice <= _cart.Count)
        {
            Console.WriteLine($"{_cart[choice - 1].Name} har tagits bort från varukorgen.");
            _cart.RemoveAt(choice - 1);
        }
        else
        {
            Console.WriteLine("Ogiltigt val. Försök igen.");
        }
    }

    private void Checkout()
    {
        if (_cart.Count == 0)
        {
            Console.WriteLine("\nVarukorgen är tom. Lägg till produkter innan du checkar ut.");
            return;
        }

        string customerName, customerAddress;

        // Validera att namn och adress inte är tomma
        do
        {
            Console.Write("\nAnge ditt namn: ");
            customerName = Console.ReadLine()?.Trim();
            if (string.IsNullOrWhiteSpace(customerName))
            {
                Console.WriteLine("Namn får inte vara tomt. Försök igen.");
            }
        } while (string.IsNullOrWhiteSpace(customerName));

        do
        {
            Console.Write("Ange din adress: ");
            customerAddress = Console.ReadLine()?.Trim();
            if (string.IsNullOrWhiteSpace(customerAddress))
            {
                Console.WriteLine("Adress får inte vara tomt. Försök igen.");
            }
        } while (string.IsNullOrWhiteSpace(customerAddress));

        try
        {
            // Hämta alla kunder från databasen till minnet och jämför där
            var customer = _db.Customers
                .AsEnumerable()  // Hämta alla kunder till minnet
                .FirstOrDefault(c => c.Name.Equals(customerName, StringComparison.OrdinalIgnoreCase)
                                      && c.Adress.Equals(customerAddress, StringComparison.OrdinalIgnoreCase));

            if (customer == null)
            {
                // Om kunden inte finns, skapa en ny kund
                customer = new Customer { Name = customerName, Adress = customerAddress };
                _db.Customers.Add(customer);
                _db.SaveChanges();
            }

            // Skapa order
            var order = new Order
            {
                CustomerId = customer.Id,
                OrderDate = DateTime.Now,
                OrderItems = _cart.Select(p => new OrderItem
                {
                    ProductId = p.Id,
                    Quantity = 1
                }).ToList()
            };

            _db.Orders.Add(order);
            _db.SaveChanges();

            // Bekräftelse av köp
            Console.WriteLine("\nTack för din beställning! Ordern har lagts till i systemet.");

            // Fraktkostnad (om du vill inkludera frakt)
            decimal shippingCost = 50; // Ställ in fraktkostnaden här

            // Fraktkostnad och totalbelopp
            Console.WriteLine("\nVill du lägga till fraktkostnad? (J/N)");
            string shippingChoice = Console.ReadLine()?.Trim().ToUpper();

            decimal totalAmount = _cart.Sum(p => p.Price);  // Summan av produkterna utan frakt

            if (shippingChoice == "J")
            {
                Console.WriteLine($"Fraktkostnad: {shippingCost} SEK");
                totalAmount += shippingCost; // Lägg till fraktkostnad
            }
            else
            {
                Console.WriteLine("Ingen fraktkostnad tillagd.");
            }

            Console.WriteLine($"Totalt att betala: {totalAmount} SEK");

            // Rensa varukorgen efter beställning
            _cart.Clear();
            Console.WriteLine("\nTack för ditt köp! Ha en fin dag.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ett fel inträffade vid beställningen: {ex.Message}");
            if (ex.InnerException != null)
            {
                Console.WriteLine($"Inner Exception: {ex.InnerException.Message}");
            }
        }
    }
}