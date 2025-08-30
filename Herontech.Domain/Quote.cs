namespace Herontech.Domain;

public class Quote : BaseEntity
{
    public string QuoteNumber { get; set; } = default!;
    
    public Guid ClientId { get; set; } = default!;
    public Client Client { get; set; } = default!;
    
    public Guid  ContactId { get; set; } = default!;
    public Contact Contact { get; set; } = default!;
    
    public IEnumerable<QuoteRevision> QuoteRevisions { get; set; } = default!;
    public short CurrentRevision { get; set; }
    
}

public class QuoteRevision : BaseEntity
{
    
    public short RevisionNumber { get; set; }
    
    
    public Guid QuoteId { get; set; } = default!;
    public Quote  Quote { get; set; } = default!;
    
    public Guid PaymentTermId { get; set; } = default!;
    public PaymentTerm PaymentTerm { get; set; } = default!;
    
    public string Title { get; set; } = default!;
    
    public decimal SalesPersonCommissionPercentage { get; set; } = default!;
    public decimal SalesPersonCommission { get; set; } = default!;
    
    public decimal DiscountPercentage  { get; set; } = default!;
    public decimal TaxPercentage  { get; set; } = default!;
    
    
    public string Description { get; set; } = default!;
    public DateTimeOffset QuoteExpireDateTime { get; set; } = default!;
    
    public DateTimeOffset? DeliveryDateTime { get; set; }
    
    public decimal TotalBeforeTaxes { get; set; } = default!;
    public decimal TotalAfterTaxes { get; set; } = default!;

    
    public IEnumerable<QuoteItem> Items { get; set; } = default!;
    
}


public class QuoteItem : BaseEntity
{
    public Guid QuoteRevisionId { get; set; } = default!;
    public QuoteRevision QuoteRevision { get; set; } = default!;
    
    public string Description { get; set; } = default!;
 
    public decimal TotalBeforeTaxes { get; set; } = default!;
    public decimal TotalAfterTaxes { get; set; } = default!;
   
    public IEnumerable<QuoteProduct> QuoteProducts { get; set; } = default!;
    
    
}


// Um subitem. Está relacionado ao calculo de fato. Pode representar custos
public class QuoteProduct : BaseEntity
{
    public Guid QuoteItemId { get; set; } = default!;
    public QuoteItem QuoteItem { get; set; } = default!;
    
    public Guid ProductId { get; set; } = default!;
    public Product Product { get; set; } = default!;

    public string Description { get; set; } = default!;
    public decimal UnitPrice { get; set; } = default!;
    public decimal Quantity { get; set; } = default!;
    public decimal MarkupPercentage { get; set; } = default!;
    
    public decimal Total0 { get; set; } = default!;
    public decimal Total1AfterMarkup { get; set; } = default!;
    public decimal Total3AfterComission { get; set; } = default!;
    public decimal Total2AfterDiscount { get; set; } = default!;
    public decimal Total4AfterTaxes { get; set; } = default!;
    
    
    public Guid? RelatedClientProductId { get; set; }
    public ClientProduct? RelatedClientProduct { get; set; }

    
}



