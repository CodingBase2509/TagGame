using FluentValidation;
using TagGame.Shared.DTOs.Rooms;
using TagGame.Shared.Validation;

namespace TagGame.Api.Core.Validation.Rooms;

public class JoinRoomRequestValidator : AbstractValidator<JoinRoomRequestDto>
{
    public JoinRoomRequestValidator()
    {
        RuleFor(x => x.AccessCode)
            .Custom((value, context) =>
            {
                if (!RoomRules.TryValidateAccessCode(value, out var error))
                    context.AddFailure(error!);
            });
    }
}
