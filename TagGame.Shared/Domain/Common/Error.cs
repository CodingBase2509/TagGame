using FluentValidation.Results;

namespace TagGame.Shared.Domain.Common;

public class Error(int code, string message)
{
    public Error()
        : this(0, string.Empty)
    {
        
    }
    
    public Error(int code, ValidationResult result)
        : this(code, string.Join(", ", result.Errors))
    { }

    public string Message { get; set; } = message;

    public int Code { get; set; } = code;
}
