using FluentValidation;
using TagGame.Shared.DTOs.Rooms;
using TagGame.Shared.Validation;

namespace TagGame.Api.Core.Validation.Rooms;

public class CreateRoomRequestValidator : AbstractValidator<CreateRoomRequestDto>
{
    public CreateRoomRequestValidator()
    {
        RuleFor(x => x.Name)
            .Custom((value, context) =>
            {
                if (!RoomRules.TryValidateName(value, out var error))
                    context.AddFailure(error!);
            });
    }
}
