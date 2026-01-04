using FluentValidation;
using TTX.App.Repositories;

namespace TTX.App.Services.Creators.Validation;

public class OnboardValidator : AbstractValidator<OnboardRequest>
{
    public OnboardValidator(ICreatorRepository _repository) : base()
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
            .MustAsync((ticker, _) => _repository.IsTickerTaken(ticker))
            .WithErrorCode("UNIQUE")
            .WithMessage("must be unique");
    }
}
