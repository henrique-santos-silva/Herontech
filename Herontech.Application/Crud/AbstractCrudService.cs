using System.Net;
using Herontech.Contracts;
using Herontech.Contracts.Interfaces;
using Herontech.Domain;
using Herontech.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using MySqlConnector;

namespace Herontech.Application.Crud;

public abstract class AbstractCrudService<T> : ICrudService<T>
    where T : BaseEntity, new()
{
    
    public virtual Task<ApiResultVoid?> ModifyModelBeforeDb(T model, CancellationToken ct)
        => Task.FromResult<ApiResultVoid?>(null);

    private readonly AppDbContext _db;

    public AbstractCrudService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<ApiResultDto<IdDto>> Create<TDto>(TDto dto, CancellationToken ct)
    where TDto:IInto<T>
    {
        var validationResult = dto.ValidateEntityPost();
        if (validationResult?.Success is false)
            return validationResult.IntoError<IdDto>();

        var entity = dto.Into();
        var modificationResult = await ModifyModelBeforeDb(entity, ct);
        if (modificationResult?.Success is false)
            return modificationResult.IntoError<IdDto>();

        try
        {
            ct.ThrowIfCancellationRequested();

            await _db.Set<T>().AddAsync(entity, ct);
            await _db.SaveChangesAsync(ct);

            return new ApiResultDto<IdDto>
            {
                StatusCode = HttpStatusCode.Created,
                Data = new IdDto(entity.Id)
            };
        }
        catch (Exception ex)
        {
            return Catcher(ex, ct).IntoError<IdDto>();
        }
    }
    

    public async Task<ApiResultVoid> Delete(Guid entityId, CancellationToken ct)
    {
        try
        {
            ct.ThrowIfCancellationRequested();

            var entity = new T { Id = entityId };
            _db.Attach(entity);
            _db.Remove(entity);

            var affected = await _db.SaveChangesAsync(ct);
            if (affected == 0)
            {
                return new ApiResultVoid
                {
                    StatusCode = HttpStatusCode.NotFound,
                    Error = new ApiResultError
                    {
                        Message = "Entidade não encontrada."
                    }
                };
            }

            return new ApiResultVoid
            {
                StatusCode = HttpStatusCode.OK
            };
        }
        catch (Exception ex)
        {
            return Catcher(ex, ct);
        }
    }

    public async Task<ApiResultVoid> Update<TDto>(Guid entityId, TDto dto, CancellationToken ct)
        where TDto : IIntoPatch<T>
    {
        var validationResult = dto.ValidateEntityPatch();
        if (validationResult?.Success is false) return validationResult;

        var entity = new T { Id = entityId };
        _db.Attach(entity);

        // 1) Aplica DTO e marca as props do payload
        var patch = dto.IntoPatch(entity);
        var entry = _db.Entry(entity);
        foreach (var kv in patch.ModifiedProperties)
        {
            entry.Property(kv.Key).CurrentValue = kv.Value;
            entry.Property(kv.Key).IsModified = true;
        }

        // 2) Snapshot após aplicar DTO (baseline local, sem SELECT)
        var baseline = entry.CurrentValues.Clone();

        // 3) Modificações adicionais (auditoria, denormalizações, etc.)
        var modificationResult = await ModifyModelBeforeDb(entity, ct);
        if (modificationResult?.Success is false) return modificationResult;

        // 4) Detecta o que mudou depois do baseline e marca IsModified
        foreach (var prop in entry.Properties)
        {
            if (prop.IsModified) continue; // já marcado via DTO

            var before = baseline[prop.Metadata.Name];
            var after  = prop.CurrentValue;

            // Comparação segura (inclui nulls)
            if (!Equals(before, after))
            {
                prop.IsModified = true;
            }
        }

        try
        {
            ct.ThrowIfCancellationRequested();
            await _db.SaveChangesAsync(ct);
            return new ApiResultVoid { StatusCode = HttpStatusCode.OK };
        }
        catch (Exception ex)
        {
            return Catcher(ex, ct);
        }
    }


    private ApiResultVoid Catcher(Exception ex, CancellationToken ct)
    {
        if (ct.IsCancellationRequested || ex is OperationCanceledException || ex is TaskCanceledException)
        {
            return new ApiResultVoid
            {
                StatusCode = (HttpStatusCode)499, // Client Closed Request (padrão Nginx)
                Error = new ApiResultError
                {
                    Message = "Requisição cancelada pelo cliente.",
                    Detail = ex.Message
                }
            };
        }

        return ex switch
        {
            // cancelamento
            _ when ct.IsCancellationRequested || ex is OperationCanceledException || ex is TaskCanceledException
                => new ApiResultVoid
                {
                    StatusCode = (HttpStatusCode)499,
                    Error = new ApiResultError
                    {
                        Message = "Requisição cancelada pelo cliente.",
                        Detail = ex.Message
                    }
                },

            // concorrência
            DbUpdateConcurrencyException dbcx => new ApiResultVoid
            {
                StatusCode = HttpStatusCode.Conflict,
                Error = new ApiResultError
                {
                    Message = "O registro foi modificado ou removido por outro processo.",
                    Detail = dbcx.Message
                }
            },

            // UNIQUE (MySQL 1062)
            DbUpdateException { InnerException: MySqlException { Number: 1062 } mySqlEx } => new ApiResultVoid
            {
                StatusCode = HttpStatusCode.Conflict,
                Error = new ApiResultError
                {
                    Message = "Violação de chave única (MySQL).",
                    Detail = mySqlEx.Message
                }
            },
            
            DbUpdateException { InnerException: MySqlException { Number: 1048 } mySqlEx } => new ApiResultVoid
            {
                StatusCode = HttpStatusCode.BadRequest,
                Error = new ApiResultError
                {
                    Message = "Tentativa de gravar valor nulo em coluna obrigatória.",
                    Detail = mySqlEx.Message
                }
            },

            // FK: aponta para pai inexistente (insert/update) → 1452
            DbUpdateException { InnerException: MySqlException { Number: 1452 } mySqlEx } => new ApiResultVoid
            {
                StatusCode = HttpStatusCode.Conflict,
                Error = new ApiResultError
                {
                    Message = "Violação de integridade referencial: chave estrangeira inexistente.",
                    Detail = mySqlEx.Message
                }
            },

            // FK: tentativa de deletar/atualizar pai com filhos existentes → 1451
            DbUpdateException { InnerException: MySqlException { Number: 1451 } mySqlEx } => new ApiResultVoid
            {
                StatusCode = HttpStatusCode.Conflict,
                Error = new ApiResultError
                {
                    Message = "Violação de integridade referencial: registro ainda é referenciado.",
                    Detail = mySqlEx.Message
                }
            },

            // (Opcional) compat: 1216/1217
            DbUpdateException { InnerException: MySqlException { Number: var n } mySqlEx } 
                when n == 1216 || n == 1217 => new ApiResultVoid
            {
                StatusCode = HttpStatusCode.Conflict,
                Error = new ApiResultError
                {
                    Message = "Violação de integridade referencial (compat).",
                    Detail = mySqlEx.Message
                }
            },

            // fallback DbUpdate
            DbUpdateException dbue => new ApiResultVoid
            {
                StatusCode = HttpStatusCode.BadRequest,
                Error = new ApiResultError
                {
                    Message = "Erro ao persistir mudanças no banco.",
                    Detail = dbue.Message
                }
            },

            // fallback geral
            _ => new ApiResultVoid
            {
                StatusCode = HttpStatusCode.InternalServerError,
                Error = new ApiResultError
                {
                    Message = "Erro inesperado.",
                    Detail = ex.Message
                }
            }
        };
    }
}
