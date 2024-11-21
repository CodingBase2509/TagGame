using FluentValidation.Results;

namespace TagGame.Shared.Domain.Common;

public class Error(int code, string message)
{
    public Error(int code, ValidationResult result)
        : this(code, string.Join(", ", result.Errors))
    { }

    public string Message = message;

    public int Code = code;
}
