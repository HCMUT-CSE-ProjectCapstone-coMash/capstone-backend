namespace Capstone.Domain.Entities;

public class ProductQuantities
{
    public Guid Id { get; set; }
    public Guid ProductID { get; set; }
    public string Size { get; set; } = string.Empty;
    public int Quantities { get; set; }
}