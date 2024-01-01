using Demo.WebApi.Application.Common.Enums;

namespace Demo.WebApi.Application.Common.Models;
public class HttpResponseMetadata
{
    public string Type { get; set; } = HttpResponseType.Information.ToString();
    public int StatusCode { get; set; }
    public string? Message { get; set; }
    public object? ValidationErrors { get; set; }
    public string? ErrorId { get; set; }
}

public class HttpResponseDto<T>
{
    public HttpResponseMetadata Metadata { get; set; } = default!;
    public T? Body { get; set; }
}
