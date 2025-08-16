using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Herontech.Domain;

public enum RoleEnum
{
    None = 0,
    SysAdmin,
    Employee,
    ExternalContact,
}



public sealed class User
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public RoleEnum Role { get; set; }  = RoleEnum.None;
    
    public string Email { get; set; } = default!;
    public bool IsEmailConfirmed { get; set; } = false;
    
    [JsonIgnore,IgnoreDataMember]
    public byte[] PassWordSalt { get; set; } = default!;
    
    [JsonIgnore,IgnoreDataMember]
    public byte[] PasswordHash { get; set; } = default!;
    public bool IsActive { get; set; } = true;
    
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset LastUpdatedAt { get; set; } = DateTimeOffset.UtcNow;
}