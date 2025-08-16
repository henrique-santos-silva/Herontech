namespace Herontech.Domain;

public class Quote : BaseEntity
{
    public string QuoteNumber { get; set; } = default!;
    
    public IEnumerable<QuoteRevision> QuoteRevisions { get; set; } = default!;
    public short CurrentRevision { get; set; }
    
}

public class QuoteRevision : BaseEntity
{
    public short RevisionNumber { get; set; }
    
    public Guid QuoteId { get; set; } = default!;
    public Quote  Quote { get; set; } = default!;
    
    public Guid ClientId { get; set; } = default!;
    public Client Client { get; set; } = default!;
    
    public Guid  ContactId { get; set; } = default!;
    public Contact Contact { get; set; } = default!;
    
    public string Description { get; set; } = default!;
    public DateTimeOffset QuoteExpireDateTime { get; set; } = default!;
    
    public IEnumerable<QuoteProduct> Products { get; set; } = default!;
    
}


public class QuoteProduct : BaseEntity
{
    
    public Guid ProductId { get; set; } = default!;
    public Product Product { get; set; } = default!;
    
    public Guid QuoteRevisionId { get; set; } = default!;
    public QuoteRevision QuoteRevision { get; set; } = default!;
    
    public decimal DefaultUnitPrice { get; set; } = default!;
    public decimal DiscountUnitPrice { get; set; } = default!;
    public decimal Quantity { get; set; } = default!;
    
    
    
}