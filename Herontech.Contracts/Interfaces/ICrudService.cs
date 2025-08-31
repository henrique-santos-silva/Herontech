using Herontech.Domain;

namespace Herontech.Contracts.Interfaces;

public interface ICrudService<T>
where T: BaseEntity
{
    public Task<ApiResultVoid> Delete(Guid entityId, CancellationToken ct);
    public Task<ApiResultVoid> Update<TDto>(Guid entityId,TDto dto, CancellationToken ct)
        where TDto : IIntoPatch<T>;

    public Task<ApiResultDto<IdDto>> Create<TDto>(TDto dto, CancellationToken ct) where TDto : IInto<T>;
    
    public Task<ApiResultVoid> Delete(T entity, CancellationToken ct) => Delete(entity.Id,ct);
}