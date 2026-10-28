using System.Text.Json;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace IzinTakip.Api.Common.Validation;

public sealed class ValidationFilter : IAsyncActionFilter
{
    public static IActionResult HataYaniti(ActionContext context)
    {
        var problemDetailsFactory = context.HttpContext.RequestServices.GetRequiredService<ProblemDetailsFactory>();
        var problem = problemDetailsFactory.CreateValidationProblemDetails(
            context.HttpContext,
            context.ModelState,
            StatusCodes.Status400BadRequest,
            detail: "Gönderilen bilgilerde hata var. İşaretli alanları düzeltin.");

        return new BadRequestObjectResult(problem) { ContentTypes = { "application/problem+json" } };
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        foreach (var argument in context.ActionArguments.Values)
        {
            if (argument is null)
            {
                continue;
            }

            var validatorType = typeof(IValidator<>).MakeGenericType(argument.GetType());
            if (context.HttpContext.RequestServices.GetService(validatorType) is not IValidator validator)
            {
                continue;
            }

            var sonuc = await validator.ValidateAsync(new ValidationContext<object>(argument), context.HttpContext.RequestAborted);
            foreach (var hata in sonuc.Errors)
            {
                context.ModelState.AddModelError(JsonNamingPolicy.CamelCase.ConvertName(hata.PropertyName), hata.ErrorMessage);
            }
        }

        if (!context.ModelState.IsValid)
        {
            context.Result = HataYaniti(context);
            return;
        }

        await next();
    }
}
