namespace TagGame.Shared.Domain.Common;

public class Response<T>
{
    public T? Value { get; init; }

    public Error? Error { get; init; }

    public bool IsSuccess { get; init; }

    public static implicit operator Response<T>(T value) => new()
    {
        Value = value,
        Error = null,
        IsSuccess = true
    };

    public static implicit operator Response<T>(Error value) => new()
    {
        Value = default,
        Error = value,
        IsSuccess = false
    };
}

public class Response : Response<string>
{
    public static Response Empty => new()
    {
        Value = string.Empty,
        Error = null,
    };
    
    public static implicit operator Response(Error value) => new()
    {
        Value = default,
        Error = value,
    };
}
