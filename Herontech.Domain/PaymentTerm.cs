namespace Herontech.Domain;

public class PaymentTerm : BaseEntity
{
    public string Name { get; set; } = default!;
    public decimal Description { get; set; }
}