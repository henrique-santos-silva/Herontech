using System.Reflection.Metadata;

namespace Herontech.Contracts.Interfaces;

public interface IInto<T>
{
    T Into();
    ApiResultVoid? ValidateEntityPost() => null;
    
}

public record IntoPatchModifiedProperties<T>(
    T Entity,
    IReadOnlyDictionary<string, object?> ModifiedProperties
);

public interface IIntoPatch<T>
{
    IntoPatchModifiedProperties<T> IntoPatch(T entity);
    ApiResultVoid? ValidateEntityPatch() => null;
}
