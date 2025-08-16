namespace Herontech.Contracts;

public enum PersonType { Company = 1, Individual = 2 }

public sealed record AccountDto(Guid Id, string Name);
public sealed record CompanyDto(Guid Id, string Name, string? TradeName, string? DocumentNumber, PersonType PersonType);
public sealed record ClientDto(Guid Id, string Name, string? DocumentNumber, PersonType PersonType, bool IsActive);
public sealed record ClientBranchDto(Guid Id, Guid ClientId, string Name);
public sealed record ContactDto(Guid Id, string FullName, string? Email, string? Phone, Guid? ClientId, Guid? ClientBranchId);
public sealed record ProductDto(Guid Id, string Name, string? Sku, Guid? ParentId);
public sealed record DocumentDto(Guid Id, string Title, string? Kind, Guid? ClientId, string? PayloadJson);

public enum DynamicFieldType { String,BigString, Integer, DateTime, Boolean, Enum }
public sealed record DynamicFieldDefinitionDto(Guid Id, string Entity, string Key, DynamicFieldType Type, bool Required, string? EnumOptionsCsv);
public sealed record DynamicFieldValueDto(Guid Id, Guid DefinitionId, Guid EntityId, string? TextValue, decimal? NumberValue, DateTime? DateValue, bool? BoolValue);

public sealed record LoginRequest(string Email, string Password)
{
}

public sealed record TokenResponse(string AccessToken);