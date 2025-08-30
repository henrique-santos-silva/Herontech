using System.Net;
using Herontech.Contracts.Interfaces;
using Herontech.Domain;

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
        // var apiResult = new ApiResultDto<Client>() { Success = false };
        // if (string.IsNullOrWhiteSpace(Name))
        // {
        //     apiResult.StatusCode = HttpStatusCode.BadRequest;
        //     apiResult.Error = new(){Message = "Nome é obrigatório"};
        //     return apiResult;
        // }
        //
        // if (type == ClientType.Company && Register.Length != 14)
        // {
        //     apiResult.StatusCode = HttpStatusCode.BadRequest;
        //     apiResult.Error = new(){Message = "CNPJ deve conter 14 caracteres"};
        //     return apiResult;
        // }
        // if (type == ClientType.Person && Register.Length != 11)
        // {
        //     apiResult.StatusCode = HttpStatusCode.BadRequest;
        //     apiResult.Error = new(){Message = "CPF deve conter 11 caracteres"};
        //     return apiResult;
        // }
        //     
        // if ( CompanyHeadquartersId is not null && companyBranches?.Any() is true)
        // {
        //     apiResult.StatusCode = HttpStatusCode.BadRequest;
        //     apiResult.Error = new(){Message = "Empresa não pode ser filial e matriz ao mesmo tempo"};
        //     return apiResult;
        // }
        //     
        // apiResult.StatusCode = HttpStatusCode.OK;
        // apiResult.Error = null;
    }
}