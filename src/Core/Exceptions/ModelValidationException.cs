using System.ComponentModel.DataAnnotations;

namespace TTX.Core.Exceptions;

public class ModelValidationException(IEnumerable<ValidationResult> results) : DomainException
{
    public IEnumerable<ValidationResult> Results { get; } = results;
}