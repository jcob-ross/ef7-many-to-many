namespace ef7_many_to_many_test
{
  using System.Collections.Generic;
  using System.Linq;
  using Microsoft.EntityFrameworkCore;
  using static System.Console;

  public class Program
  {
    public static void Main(string[] args)
    {
      SetupDb();
      DumpDb();

      WriteLine("> Press Enter to continue");
      ReadLine();

      DeleteTagByName("pure awesomeness");
      DumpDb();

      WriteLine("> Press Enter to continue");
      ReadLine();

      RemoveTagFromProduct("Ring of Void", "out of stock");
      DumpDb();
    }

    private static void RemoveTagFromProduct(string productName, string tagName)
    {
      using (var context = new Context())
      {
        WriteLine();
        WriteLine($"> Removing tag '{tagName}' from product '{productName}'");

        var product = context.Products.Include(p => p.ProductTag).ThenInclude(pt => pt.Tag)
                         .FirstOrDefault(p => p.Name == productName);

        var targetProductTag = product.ProductTag.SingleOrDefault(pt => pt.Tag.Name == tagName);
        product.ProductTag.Remove(targetProductTag);
        var affectedRowsCount = context.SaveChanges();
        WriteLine($"> {affectedRowsCount} row(s) were affected");
      }
    }

    private static void DeleteTagByName(string tagName)
    { 
      WriteLine($"> Deleting tag with name '{tagName}'");

      using (var context = new Context())
      {
        var tag = context.Tags.Include(t => t.ProductTag).ThenInclude(pt => pt.Product)
                     .FirstOrDefault(t => t.Name == tagName);

        context.Tags.Remove(tag);
        var affectedRowsCount = context.SaveChanges();
        WriteLine($"> {affectedRowsCount} row(s) were affected");
      }
    }

    private static void DumpDb()
    { 
      using (var context = new Context())
      {
        WriteLine("\n===========================================");

        var products = context.Products.Include(p => p.ProductTag).ThenInclude(pt => pt.Tag).ToList();
        
        WriteLine("> Product dump:");
        foreach (var product in products)
        {
          WriteLine($"Product '{product.Name}' has tags:");
          var productTags = product.ProductTag.Select(p => p.Tag);
          WriteLine(productTags.Aggregate("", (acc, item) => acc += $"{item.Name}; "));
          WriteLine();
        }

        WriteLine();

        var tags = context.Tags.Include(t => t.ProductTag).ThenInclude(pt => pt.Product).ToList();

        WriteLine("> Tag dump:");
        foreach (var tag in tags)
        {
          WriteLine($"Tag '{tag.Name}' is on products:");
          var tagProducts = tag.ProductTag.Select(t => t.Product);
          WriteLine(tagProducts.Aggregate("", (acc, item) => acc += $"{item.Name}; "));
        }
        WriteLine("\n===========================================");
      }
    }

    private static void SetupDb()
    {
      using (var context = new Context())
      {
        WriteLine("> Database setup / tear-down ...");
        context.Database.EnsureDeleted();
        context.Database.EnsureCreated();

        var tagAwesome = new Tag { Name = "pure awesomeness" };
        var tagNoLazy = new Tag { Name = "makes you less lazy" };
        var tagUnicornPoop = new Tag { Name = "smells like unicorn poop" };
        var tagOutOfStock = new Tag {Name = "out of stock"};

        var productSocks = new Product {Name = "Socks (over) 9000 GT"};
        context.Products.Add(productSocks);

        AddTagToProduct(context, productSocks, tagAwesome);
        AddTagToProduct(context, productSocks, tagNoLazy);
        AddTagToProduct(context, productSocks, tagUnicornPoop);

        var productRing = new Product {Name = "Ring of Void"};
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

      var productTag = new ProductTag {Product = product, Tag = tag};
      context.ProductTags.Add(productTag);
    }
  }

  public class Context : DbContext
  {
    public DbSet<Product> Products { get; set; }
    public DbSet<Tag> Tags { get; set; }
    public DbSet<ProductTag> ProductTags { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
      const string host = "localhost";
      const string port = "5432";
      const string dbName = "manytest";
      const string userId = "testing";
      const string password = "testing";

      optionsBuilder.UseNpgsql($"User ID={userId};Password={password};Host={host};Port={port};Database={dbName};Pooling=true;");
    }

    protected override void OnModelCreating(ModelBuilder b)
    {
      base.OnModelCreating(b);

      b.Entity<Product>()
       .HasKey(p => p.Id);

      b.Entity<Product>()
       .Property(p => p.Name)
       .IsRequired();

      b.Entity<Tag>()
       .HasKey(t => t.Id);

      b.Entity<Tag>()
       .Property(t => t.Name)
       .IsRequired();



      // actual many to many setup follows
      // https://github.com/aspnet/EntityFramework/issues/1368

      b.Entity<ProductTag>()
       .HasKey(pt => new {pt.ProductId, pt.TagId});

      b.Entity<ProductTag>()
       .HasOne(pt => pt.Product)
       .WithMany(p => p.ProductTag)
       .HasForeignKey(pt => pt.ProductId);

      b.Entity<ProductTag>()
       .HasOne(pt => pt.Tag)
       .WithMany(t => t.ProductTag)
       .HasForeignKey(pt => pt.TagId);
    }
  }

  public class Product
  {
    public int Id { get; set; }
    public string Name { get; set; }

    public List<ProductTag> ProductTag { get; set; } = new List<ProductTag>();
  }

  public class Tag
  {
    public int Id { get; set; }
    public string Name { get; set; }

    public List<ProductTag> ProductTag { get; set; } = new List<ProductTag>();
  }

  public class ProductTag
  {
    public int ProductId { get; set; }
    public Product Product { get; set; }

    public int TagId { get; set; }
    public Tag Tag { get; set; }
  }
}