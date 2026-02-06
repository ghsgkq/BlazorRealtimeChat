using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazorRealtimeChat.Shared.DTOs;

public class ServiceResponse<T>
{
    public T? Data { get; set; }
    public bool Success { get; set; } = true;
    public string Message { get; set; } = string.Empty;
    public ResponseStatus Status { get; set; } = ResponseStatus.Ok;
}

public enum ResponseStatus
{
    Ok,
    Created,
    BadRequest,
    NotFound,
    Unauthorized,
    Forbidden,
    InternalServerError
}

