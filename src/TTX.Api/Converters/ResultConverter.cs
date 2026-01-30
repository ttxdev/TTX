using Microsoft.AspNetCore.Mvc;
using TTX.Domain.Exceptions;
using TTX.Domain.ValueObjects;

namespace TTX.Api.Converters;

public static class ResultConverter
{
    public static ActionResult<T> ToActionResult<T>(this Result<T> result)
    {
        if (!result.IsSuccessful)
        {
            return result.ToBadActionResult();
        }

        return new OkObjectResult(result.Value);
    }

    public static ActionResult ToBadActionResult<T>(this Result<T> result)
    {
        if (result.Error is NotFoundException<object>)
        {
            return new NotFoundObjectResult(result.Error);
        }

        return new BadRequestObjectResult(result.Error);
    }
}
