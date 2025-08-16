namespace Herontech.Domain;

public sealed class ProductCategory : BaseEntity
{
    public string Name        { get; set; } = default!;
    public string? Description { get; set; } = default!;
    
    public Guid? ParentProductCategoryId { get; set; }
    public ProductCategory? ParentProductCategory { get; set; }
    public IEnumerable<ProductCategory>? ChildProductCategories { get; set; }
    
    public IEnumerable<Product>? Products { get; set; }
}