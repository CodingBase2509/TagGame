using FluentValidation;
using TagGame.Shared.Constants;
using TagGame.Shared.DTOs.Games;

namespace TagGame.Api.Validation.GameRoom;

public class JoinRoomValidator : AbstractValidator<JoinGameRoom.Request>
{
    public JoinRoomValidator()
    {
        RuleFor(x => x.AccessCode)
            .NotEmpty()
            .WithMessage("empty-accesscode");

        RuleFor(x => x.AccessCode)
            .MaximumLength(MaxLengthOptions.AccessCodeLenght)
            .WithMessage("too-long-accesscode");

        RuleFor(x => x.GameName)
            .NotEmpty()
            .WithMessage("empty-name");

        RuleFor(x => x.GameName)
            .MaximumLength(MaxLengthOptions.GameNameLenght)
            .WithMessage("too-long-name");
    }
}
