using System.Net;
using System.Text.Json.Serialization;

namespace Herontech.Contracts;

public class ApiResultError
{
    public string  Message {get; set;}
    public string? Detail  {get; set;}
}

public record IdDto(Guid Id);
public class ApiResultDto<T>
{
    [JsonIgnore] public HttpStatusCode StatusCode { get; set; }
    public bool Success => (int) StatusCode >= 200 && (int)StatusCode <= 299;
    public T? Data { get; set; }
    public ApiResultError? Error { get; set; }

    public ApiResultDto<U> IntoError<U>()
    {
        if (Success) throw new InvalidOperationException("Cannot cast api result error");
        return new ApiResultDto<U>()
        {
            StatusCode = StatusCode,
            Data = default(U),
            Error = Error
        };
    }
    
    public ApiResultVoid IntoErrorVoid()
    {
        if (Success) throw new InvalidOperationException("Cannot cast api result error");
        return new ApiResultVoid()
        {
            StatusCode = StatusCode,
            Error = Error
        };
    }
    
}

public class ApiResultVoid
{
    [JsonIgnore] public HttpStatusCode StatusCode { get; set; }
    public bool Success => (int) StatusCode >= 200 && (int)StatusCode <= 299;

    public ApiResultError? Error { get; set; }
    
    public ApiResultDto<U> IntoError<U>()
    {
        if (Success) throw new InvalidOperationException("Cannot cast api result error");
        return new ApiResultDto<U>()
        {
            StatusCode = StatusCode,
            Data = default(U),
            Error = Error
        };
    }
}