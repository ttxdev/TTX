using FluentValidation;
using TTX.App.Data;
using Microsoft.EntityFrameworkCore;

namespace TTX.App.Services.Creators.Validation;

public class OnboardValidator : AbstractValidator<OnboardRequest>
{
    public OnboardValidator(ApplicationDbContext dbContext) : base()
    {
        RuleFor(r => r.Username)
            .NotEmpty()
            .NotNull()
            .Unless(r => r.PlatformId is not null)
            .WithErrorCode("REQUIRED")
            .WithMessage("is required");

        RuleFor(r => r.PlatformId)
            .NotEmpty()
            .NotNull()
            .Unless(r => r.Username is not null)
            .WithErrorCode("REQUIRED")
            .WithMessage("is required");

        RuleFor(r => r.Ticker)
            .MustAsync((ticker, ct) => dbContext.Creators.Where(c => c.Ticker == ticker).AnyAsync(ct))
            .WithErrorCode("UNIQUE")
            .WithMessage("must be unique");
    }
}
