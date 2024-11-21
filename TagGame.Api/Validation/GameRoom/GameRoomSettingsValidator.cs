using FluentValidation;
using TagGame.Shared.Domain.Games;

namespace TagGame.Api.Validation.GameRoom;

public class GameRoomSettingsValidator : AbstractValidator<GameSettings>
{
    public GameRoomSettingsValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("empty-id");
        
        RuleFor(x => x.RoomId)
            .NotEmpty()
            .WithMessage("empty-id");

        RuleFor(x => x.SeekerIds)
            .ForEach(x =>
                x.NotEmpty()
                    .WithMessage("empty-id"));

        RuleFor(x => x.HideTimeout)
            .Must(p => p > TimeSpan.Zero);

        RuleFor(x => x.PingInterval)
            .Must(p => p > TimeSpan.Zero)
            .When(x => x.IsPingEnabled);
    }
}