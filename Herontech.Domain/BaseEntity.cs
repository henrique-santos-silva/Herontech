namespace Herontech.Domain;

public class BaseEntity
{
    public Guid Id { get; set; } = Guid.Empty;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset LastUpdatedAt { get; set; } = DateTimeOffset.UtcNow;
    public Guid CreatorId     { get; set; }
    
    public User Creator { get; set; }
    
    public Guid LastUpdaterId { get; set; }
    public User LastUpdater { get; set; }
    


    public BaseEntity BackupBaseEntity() => new()
    {
        Id = Id,
        CreatedAt = CreatedAt,
        CreatorId = CreatorId,
        Creator = Creator,
        LastUpdatedAt = LastUpdatedAt,
        LastUpdaterId = LastUpdaterId,
        LastUpdater = LastUpdater,
    };

    public void RestoreBaseEntityBackup(BaseEntity backup)
    {
        Id = backup.Id;
        CreatedAt = backup.CreatedAt;
        CreatorId = backup.CreatorId;
        Creator = backup.Creator;
        LastUpdatedAt = backup.LastUpdatedAt;
        LastUpdaterId = backup.LastUpdaterId;
        LastUpdater = backup.LastUpdater;
    }

    public void BaseEntityDefaults()
    {
        Id = Guid.NewGuid();
        CreatedAt = default;
        LastUpdatedAt = default;
        CreatorId = default;
        LastUpdaterId = default;
    }
    
}