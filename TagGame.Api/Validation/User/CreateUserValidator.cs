using System.Drawing;
using FluentValidation;
using TagGame.Shared.DTOs.Users;

namespace TagGame.Api.Validation.User;

public class CreateUserValidator : AbstractValidator<CreateUser.CreateUserRequest>
{
    public CreateUserValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("empty-name");

        RuleFor(x => x.AvatarColor)
            .ChildRules(x =>
            {
                x.RuleFor(c => c.A)
                    .InclusiveBetween((byte)0, (byte)255);
                
                x.RuleFor(c => c.R)
                    .InclusiveBetween((byte)0, (byte)255);
                
                x.RuleFor(c => c.G)
                    .InclusiveBetween((byte)0, (byte)255);
                   
                x.RuleFor(c => c.B)
                    .InclusiveBetween((byte)0, (byte)255);
            })
            .WithMessage("invalid-avatar-color");
    }
}