namespace Herontech.Domain;

public sealed class Product : BaseEntity
{
    public string Name        { get; set; } = default!;
    public string Description { get; set; } = default!;
    
    public Guid MeasurementUnitId { get; set; } = default!;
    public MeasurementUnit MeasurementUnit { get; set; } = default!;
    public string UnitPrice   { get; set; } = default!;
    public Guid ParentProductCategoryId { get; set; } = default!;
    public ProductCategory ParentProductCategory { get; set; } = default!;
}


public sealed class ClientProduct : BaseEntity
{
    public string Identifier { get; set; } = default!;
    public string? SerialNumber { get; set; } = default!;
    
    public Guid ProductId { get; set; } = default!;
    public Product Product { get; set; } = default!;   
    
    public Guid ClientId { get; set; } = default!;
    public Client  Client { get; set; } = default!;
    
    
}


public sealed class MeasurementUnit : BaseEntity
{
    // Identificação
    public string Name { get; set; } = default!;
    public string Symbol { get; set; } = default!;

    // Vetor dimensional (SI base): m, kg, s, A, K, mol, cd
    // Use int/short para potências exatas e estáveis
    public short L { get; set; } // length (m)
    public short M { get; set; } // mass (kg)
    public short T { get; set; } // time (s)
    public short I { get; set; } // electric current (A)
    public short Th { get; set; } // thermodynamic temperature (K)
    public short N { get; set; } // amount of substance (mol)
    public short J { get; set; } // luminous intensity (cd)

    // Conversão para a unidade SI correspondente ao vetor dimensional
    // value_SI = (value_unit * FactorToSI) + OffsetToSI
    // OffsetToSI ≠ 0 apenas para unidades afins (ex.: °C → K com +273.15)
    public double FactorToSI { get; set; } = 1d;
    public double OffsetToSI { get; set; } = 0d;
}

