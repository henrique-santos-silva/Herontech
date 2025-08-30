namespace Herontech.Domain;


public enum ClientType
{
    Company,
    Person
}

public sealed class Client : BaseEntity
{
    public ClientType Type { get; set; } = ClientType.Company;
    
    public string Register {get; set;} = default!;
    
    public string Name { get; set; } = default!;
    public string? LegalName { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    
    public Guid? CompanyHeadQuartersId { get; set; }
    public Client? CompanyHeadQuarters { get; set; }
    
    public IEnumerable<Client> CompanyBranches { get; set; } = default!;
    
    public IEnumerable<ClientContactRelationship> ClientContactRelationships { get; set; } = default!;
    
    public IEnumerable<Quote> Quotes { get; set; } = default!;
    
}

public sealed class Contact : BaseEntity
{
    public string? PersonalPhone { get; set; } 
    public string? PersonalEmail { get; set; } 
    public string FirstName { get; set; } = default!;
    public string? LastName { get; set; }
    public string? Register {get; set;}
    
    public IEnumerable<ClientContactRelationship> ClientContactRelationships { get; set; }
}


public sealed class ClientContactRelationship : BaseEntity
{
    public string? CompanyRelatedPhone { get; set; }
    public string? CompanyRelatedEmail { get; set; }
    
    public Guid ClientId { get; set; }  =  default!;
    public Client Client { get; set; } = default!;
    
    public Guid ContactId { get; set; } =  default!;
    public Contact Contact { get; set; } = default!;
    
    public DateTimeOffset Start {get; set;} = default!;
    public DateTimeOffset? End {get; set;}
}