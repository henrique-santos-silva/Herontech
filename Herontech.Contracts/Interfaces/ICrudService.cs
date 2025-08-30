using Herontech.Domain;

namespace Herontech.Contracts.Interfaces;

public interface ICrudService<T>
where T: BaseEntity
{
    public Task<ApiResultDto<IdDto>> Create(T entity);
    public Task<ApiResultVoid>       Update(T entity);
    public Task<ApiResultDto<IdDto>> Delete(Guid entityId);
    
    public Task<ApiResultDto<IdDto>> Create<TDto>(TDto dto)
        where TDto : IInto<T> =>
        Create(dto.Into());

    public Task<ApiResultVoid> Update<TDto>(TDto dto) 
        where TDto : IInto<T> =>
        Update(dto.Into());

    public Task<ApiResultDto<IdDto>> Delete(T entity) => Delete(entity.Id);
}