using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Deevenue.Api.Framework;

internal class FluentValidationExceptionFilter : IActionFilter, IOrderedFilter
{
    public int Order => 10;

    public void OnActionExecuted(ActionExecutedContext context)
    {
        if (context.Exception is ValidationException validationException)
        {
            var problemDetails = new ProblemDetails
            {
                Title = "Validation failure",
                Detail = string.Join(". ", validationException.Errors.Select(e => e.ErrorMessage)),
                Status = 400,
            };

            context.Result = new BadRequestObjectResult(problemDetails);
            context.ExceptionHandled = true;
        }
    }

    public void OnActionExecuting(ActionExecutingContext context) { }
}
