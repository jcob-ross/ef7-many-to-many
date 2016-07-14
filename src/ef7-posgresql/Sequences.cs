namespace ef7_posgresql
{
  using System;
  using System.Linq;
  using Models;

  public static class Sequences
  {
    public static void Run()
    {
      SetupDb();
      DumpDb();
    }

    public static void SetupDb()
    {
      using (var context = new Context())
      {
        Console.WriteLine("> Database setup / tear-down ...");
        context.Database.EnsureDeleted();
        context.Database.EnsureCreated();
        
        context.Products.Add(new Product { Name = "Socks (over) 9000 GT" });
        context.Products.Add(new Product { Name = "Ring of Void" });
        context.Products.Add(new Product { Name = "3Dfx Voodoo 3 3000" });
        context.Products.Add(new Product { Name = "Riva TNT 2" });

        context.SaveChanges();
      }
    }

    public static void DumpDb()
    {
      using (var context = new Context())
      {
        Console.WriteLine("> Product dump:");
        var products = context.Products.ToList();

        foreach (var product in products)
        {
          var text = string.Empty;
          text += $"Product: {product.Name}\n";
          text += $"Number: {product.ProductNumber}\n";
          Console.WriteLine(text);
        }
      }
    }
  }
}