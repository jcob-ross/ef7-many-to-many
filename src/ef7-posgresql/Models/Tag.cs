namespace ef7_posgresql.Models
{
  using System.Collections.Generic;

  public class Tag
  {
    public int Id { get; set; }
    public string Name { get; set; }

    public List<ProductTag> ProductTag { get; set; } = new List<ProductTag>();
  }
}