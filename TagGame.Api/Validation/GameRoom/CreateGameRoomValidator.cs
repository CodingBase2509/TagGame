using FluentValidation;
using TagGame.Shared.Constants;
using TagGame.Shared.DTOs.Games;

namespace TagGame.Api.Validation.GameRoom;

public class CreateGameRoomValidator : AbstractValidator<CreateGameRoom.CreateGameRoomRequest>
{
    public CreateGameRoomValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("empty-id");

        RuleFor(x => x.GameRoomName)
            .NotEmpty()
            .WithMessage("empty-name");

        RuleFor(x => x.GameRoomName)
            .MaximumLength(MaxLengthOptions.GameNameLenght)
            .WithMessage("too-long-name");
    }
}
