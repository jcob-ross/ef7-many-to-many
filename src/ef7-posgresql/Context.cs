namespace ef7_posgresql
{
  using Microsoft.EntityFrameworkCore;
  using Models;

  public class Context : DbContext
  {
    public DbSet<Product> Products { get; set; }
    public DbSet<Tag> Tags { get; set; }
    public DbSet<ProductTag> ProductTags { get; set; }
    
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
      const string host = "localhost";
      const string port = "5432";
      const string dbName = "testing";
      const string userId = "testing";
      const string password = "testing";
      
      optionsBuilder.UseNpgsql($"User ID={userId};Password={password};Host={host};Port={port};Database={dbName};Pooling=true;");
    }

    protected override void OnModelCreating(ModelBuilder b)
    {
      b.HasSequence<long>("product_numbers")
       .HasMin(1000000)
       .StartsAt(9999998)
       .HasMax(9999999)
       .IsCyclic()
       .IncrementsBy(1);
      
      b.Entity<Product>()
       .Property(p => p.ProductNumber)
       .HasDefaultValueSql("nextval('product_numbers')");



      b.Entity<Product>()
       .HasKey(p => p.Id);

      b.Entity<Product>()
       .Property(p => p.Name)
       .IsRequired();

      b.Entity<Product>()
       .Property(p => p.DateCreated)
       .HasDefaultValueSql("now()")
       .ValueGeneratedOnAdd();

      b.Entity<Product>()
       .Property(p => p.LastUpdated)
       .HasDefaultValueSql("now()")
       .ValueGeneratedOnAddOrUpdate();
       

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

      base.OnModelCreating(b);
    }
  }
}