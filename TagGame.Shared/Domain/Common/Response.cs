namespace TagGame.Shared.Domain.Common;

public class Response<T>
{
    public T? Value { get; init; }

    public Error? Error { get; init; }

    public bool IsSuccess { get; init; }

    public static Response<T> Empty => new()
    {
        Value = default,
        Error = null,
        IsSuccess = true
    };
    
    public static explicit operator Response<T>(T value) => new()
    {
        Value = value,
        Error = null,
        IsSuccess = true
    };

    public static explicit operator Response<T>(Error value) => new()
    {
        Value = default,
        Error = value,
        IsSuccess = false
    };
}
