using FluentValidation.Results;
using TTX.Domain.Exceptions;

namespace TTX.App.Exceptions;

public class InvalidRequestException(List<ValidationFailure> failures) : TtxException("Invalid Request")
{
    public List<ValidationFailure> Failure = failures;
}
