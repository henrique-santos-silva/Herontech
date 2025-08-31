using System.Net;
using Herontech.Contracts.Interfaces;
using Herontech.Domain;
using TriStateNullable;

namespace Herontech.Contracts.Dtos;

public record PostClientDto(
    ClientType Type,
    string Register,
    string Name,
    string LegalName,
    string? Email,
    string? Phone,
    Guid? CompanyHeadquartersId
) : IInto<Client>
{
    public Client Into()
    {
        return new Client()
        {
            Name = Name,
            Register = Register,
            Type = Type,
            LegalName = LegalName,
            Email = Email,
            Phone = Phone,
            CompanyHeadQuartersId = CompanyHeadquartersId
        };
    }

    public ApiResultVoid? ValidateEntityPost()
    {
        var apiResult = new ApiResultVoid() {StatusCode = HttpStatusCode.OK};
        if (string.IsNullOrWhiteSpace(Name))
        {
            apiResult.StatusCode = HttpStatusCode.BadRequest;
            apiResult.Error = new(){Message = "Nome é obrigatório"};
            return apiResult;
        }
        
        if (this is { Type: ClientType.Company, Register.Length: not 14 })
        {
            apiResult.StatusCode = HttpStatusCode.BadRequest;
            apiResult.Error = new(){Message = "CNPJ deve conter 14 caracteres"};
            return apiResult;
        }
        if (this is { Type: ClientType.Person, Register.Length: not 11 })
        {
            apiResult.StatusCode = HttpStatusCode.BadRequest;
            apiResult.Error = new(){Message = "CPF deve conter 11 caracteres"};
            return apiResult;
        }
        return apiResult;
    }
}


public record PatchClientDto(
    TriStateNullable<ClientType> Type,
    TriStateNullable<string> Register,
    TriStateNullable<string> Name,
    TriStateNullable<string> LegalName,
    TriStateNullable<string> Email,
    TriStateNullable<string> Phone,
    TriStateNullable<Guid> CompanyHeadquartersId
) : IIntoPatch<Client>
{
    public IntoPatchModifiedProperties<Client> IntoPatch(Client entity)
    {
        var modified = new Dictionary<string, object?>();

        if (Register.Tag != TriStateNullableTag.NullNotSerializable)
        {
            var v = Register.UnwrapOrDefault(); // pode ser null (null serializável)
            entity.Register = v;
            modified[nameof(Client.Register)] = v;
        }

        if (Type.Tag != TriStateNullableTag.NullNotSerializable)
        {
            var v = Type.UnwrapOrDefault();
            entity.Type = v;
            modified[nameof(Client.Type)] = v;
        }

        if (Name.Tag != TriStateNullableTag.NullNotSerializable)
        {
            var v = Name.UnwrapOrDefault();
            entity.Name = v;
            modified[nameof(Client.Name)] = v;
        }

        if (LegalName.Tag != TriStateNullableTag.NullNotSerializable)
        {
            var v = LegalName.UnwrapOrDefault();
            entity.LegalName = v;
            modified[nameof(Client.LegalName)] = v;
        }

        if (Email.Tag != TriStateNullableTag.NullNotSerializable)
        {
            var v = Email.UnwrapOrDefault();
            entity.Email = v;
            modified[nameof(Client.Email)] = v;
        }

        if (Phone.Tag != TriStateNullableTag.NullNotSerializable)
        {
            var v = Phone.UnwrapOrDefault();
            entity.Phone = v;
            modified[nameof(Client.Phone)] = v;
        }

        if (CompanyHeadquartersId.Tag != TriStateNullableTag.NullNotSerializable)
        {
            var v = CompanyHeadquartersId.UnwrapOrDefault(); // Guid? (pode ser null)
            entity.CompanyHeadQuartersId = v;
            modified[nameof(Client.CompanyHeadQuartersId)] = v;
        }

        return new IntoPatchModifiedProperties<Client>(entity, modified);
    }

    public ApiResultVoid? ValidateEntityPatch()
    {
        var apiResult = new ApiResultVoid() {StatusCode = HttpStatusCode.OK};
        if ((Type.IsNone && Register.IsSome))
        {
            apiResult.StatusCode = HttpStatusCode.BadRequest;
            apiResult.Error = new(){Message = "Para atualizar o CPF/CNPJ é necessário informar o tipo de cliente"};
            return apiResult;
        }
        if ((Type.IsSome && Register.IsNone))
        {
            apiResult.StatusCode = HttpStatusCode.BadRequest;
            apiResult.Error = new(){Message = "Para atualizar o tipo de cliente é necessário informar o CPF/CNPJ"};
            return apiResult;
        }


        if (Type.IsSome && Register.IsSome)
        {
            ClientType type = Type.Unwrap();
            string register = Register.Unwrap();
            if (type is ClientType.Company && register.Length != 14)
            {
                apiResult.StatusCode = HttpStatusCode.BadRequest;
                apiResult.Error = new(){Message = "CNPJ deve conter 14 caracteres"};
                return apiResult;
            }
            if (type is ClientType.Person && register.Length != 11)
            {
                apiResult.StatusCode = HttpStatusCode.BadRequest;
                apiResult.Error = new(){Message = "CPF deve conter 11 caracteres"};
                return apiResult;
            }
        }
        return apiResult;
    }
}
