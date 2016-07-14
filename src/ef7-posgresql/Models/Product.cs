namespace ef7_posgresql.Models
{
  using System;
  using System.Collections.Generic;

  public class Product
  {
    public int Id { get; set; }
    public string Name { get; set; }
    public string ProductNumber { get; set; }
    public DateTime DateCreated { get; set; }
    public DateTime LastUpdated { get; set; }
    public List<ProductTag> ProductTag { get; set; } = new List<ProductTag>();
  }
}