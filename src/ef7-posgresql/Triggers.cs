namespace ef7_posgresql
{
  using System;
  using System.Linq;
  using Microsoft.EntityFrameworkCore;
  using Models;

  public static class Triggers
  {
    public static void Run()
    {
      SetupDb();
      DumpDb();

      Console.WriteLine("> Press Enter to continue");
      Console.ReadLine();

      AlterOldAndInsertNew();
      DumpDb();
    }

    public static void SetupDb()
    {
      using (var context = new Context())
      {
        Console.WriteLine("> Database setup / tear-down ...");
        context.Database.EnsureDeleted();
        context.Database.EnsureCreated();

        const string plpgsql = @"
          -- backing function for trigger which adds timestamp on insert
          create or replace function created_timestamp()
          returns trigger as $$
          begin
            NEW.""DateCreated"" = now();
            return NEW;
          end;
          $$ language 'plpgsql';

          -- backing function for trigger which adds timestamp on update
          create or replace function updated_timestamp()
          returns trigger as $$
          begin
            NEW.""LastUpdated"" = now();
            return NEW;
          end;
          $$ language 'plpgsql';

          -- trigger - timestamp on insert (DateCreated field)
          create trigger add_created_timestamp before insert on public.""Products""
          for each row execute procedure created_timestamp();

          -- trigger - timestamp on update (LastUpdated field)
          create trigger add_updated_timestamp before update on public.""Products""
          for each row execute procedure updated_timestamp();
        ";
        context.Database.ExecuteSqlCommand(plpgsql);

        context.Products.Add(new Product { Name = "Helm of B.." });
        context.Products.Add(new Product { Name = "Moonblade" });
        context.Products.Add(new Product { Name = "Minor Globe of Invulnerability" });

        context.SaveChanges();
      }
    }

    public static void AlterOldAndInsertNew()
    {
      using (var context = new Context())
      {
        context.Products.Add(new Product { Name = "Nymph Cloak" });

        var item = context.Products.First(p => p.Name.Contains("Helm"));
        item.Name = "Helm of Balduran";
        context.Products.Update(item);
        var rowsAffected = context.SaveChanges();
        Console.WriteLine("> Inserted 1 new item, updated 1 old item");
        Console.WriteLine($"> {rowsAffected} row(s) were affected\n");
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
          text += $"Date created: {product.DateCreated}\n";
          text += $"Last updated: {product.LastUpdated}\n";
          Console.WriteLine(text);
        }
      }
    }

  }
}