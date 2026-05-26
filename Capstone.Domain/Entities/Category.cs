namespace Capstone.Domain.Entities;

public class Category
{
    public Guid Id { get; set; }
    public string CategoryId { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
};