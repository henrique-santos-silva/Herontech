namespace Herontech.Domain;

public class Permission
{
    public Guid Id { get; set; }

    public int Users { get; set; }       = (int) PermissionLevel.None;
    public int Permissions { get; set; } = (int) PermissionLevel.None;
}


public enum PermissionLevel
{
    None   = 0,
    Create = 1,
    Read   = 2,
    Update = 4,
    Delete = 8
}

public static class PermissionLevelExtensions
{
    public static bool HasPermission(this int level, PermissionLevel permissionLevel)
    {
        return (level & (int)permissionLevel) == (int)permissionLevel;
    }

    public static int Permissions(params PermissionLevel[] permissionLevels)
    {
        int combined = 0;
        foreach (PermissionLevel p in permissionLevels)
            combined |= (int)p;
        return combined;
    }
}