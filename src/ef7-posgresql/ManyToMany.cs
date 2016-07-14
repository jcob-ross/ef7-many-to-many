namespace ef7_posgresql
{
  using System;
  using System.Linq;
  using Microsoft.EntityFrameworkCore;
  using Models;

  public static class ManyToMany
  {
    public static void Run()
    {
      SetupDb();
      DumpDb();

      Console.WriteLine("> Press Enter to continue");
      Console.ReadLine();

      DeleteTagByName("pure awesomeness");
      DumpDb();

      Console.WriteLine("> Press Enter to continue");
      Console.ReadLine();

      RemoveTagFromProduct("Ring of Void", "out of stock");
      DumpDb();
    }

    private static void RemoveTagFromProduct(string productName, string tagName)
    {
      using (var context = new Context())
      {
        Console.WriteLine();
        Console.WriteLine($"> Removing tag '{tagName}' from product '{productName}'");

        var product = context.Products.Include(p => p.ProductTag).ThenInclude(pt => pt.Tag)
                         .FirstOrDefault(p => p.Name == productName);

        var targetProductTag = product.ProductTag.SingleOrDefault(pt => pt.Tag.Name == tagName);
        product.ProductTag.Remove(targetProductTag);
        var affectedRowsCount = context.SaveChanges();
        Console.WriteLine($"> {affectedRowsCount} row(s) were affected");
      }
    }

    private static void DeleteTagByName(string tagName)
    {
      Console.WriteLine($"> Deleting tag with name '{tagName}'");

      using (var context = new Context())
      {
        var tag = context.Tags.Include(t => t.ProductTag).ThenInclude(pt => pt.Product)
                     .FirstOrDefault(t => t.Name == tagName);

        context.Tags.Remove(tag);
        var affectedRowsCount = context.SaveChanges();
        Console.WriteLine($"> {affectedRowsCount} row(s) were affected");
      }
    }

    private static void DumpDb()
    {
      using (var context = new Context())
      {
        Console.WriteLine("\n===========================================");

        var products = context.Products.Include(p => p.ProductTag).ThenInclude(pt => pt.Tag).ToList();

        Console.WriteLine("> Product dump:");
        foreach (var product in products)
        {
          Console.WriteLine($"Product '{product.Name}' has tags:");
          var productTags = product.ProductTag.Select(p => p.Tag);
          Console.WriteLine(productTags.Aggregate("", (acc, item) => acc += $"{item.Name}; "));
          Console.WriteLine();
        }

        Console.WriteLine();

        var tags = context.Tags.Include(t => t.ProductTag).ThenInclude(pt => pt.Product).ToList();

        Console.WriteLine("> Tag dump:");
        foreach (var tag in tags)
        {
          Console.WriteLine($"Tag '{tag.Name}' is on products:");
          var tagProducts = tag.ProductTag.Select(t => t.Product);
          Console.WriteLine(tagProducts.Aggregate("", (acc, item) => acc += $"{item.Name}; "));
        }
        Console.WriteLine("\n===========================================");
      }
    }

    private static void SetupDb()
    {
      using (var context = new Context())
      {
        Console.WriteLine("> Database setup / tear-down ...");
        context.Database.EnsureDeleted();
        context.Database.EnsureCreated();

        var tagAwesome = new Tag { Name = "pure awesomeness" };
        var tagNoLazy = new Tag { Name = "makes you less lazy" };
        var tagUnicornPoop = new Tag { Name = "smells like unicorn poop" };
        var tagOutOfStock = new Tag { Name = "out of stock" };

        var productSocks = new Product { Name = "Socks (over) 9000 GT" };
        context.Products.Add(productSocks);

        AddTagToProduct(context, productSocks, tagAwesome);
        AddTagToProduct(context, productSocks, tagNoLazy);
        AddTagToProduct(context, productSocks, tagUnicornPoop);

        var productRing = new Product { Name = "Ring of Void" };
        context.Products.Add(productRing);

        AddTagToProduct(context, productRing, tagAwesome);
        AddTagToProduct(context, productRing, tagOutOfStock);

        context.SaveChanges();
      }
    }

    private static void AddTagToProduct(Context context, Product product, Tag tag)
    {
      if (!context.Tags.Contains(tag))
        context.Tags.Add(tag);

      var productTag = new ProductTag { Product = product, Tag = tag };
      context.ProductTags.Add(productTag);
    }
  }
}