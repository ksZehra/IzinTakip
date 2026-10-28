using System.Text.Json;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace IzinTakip.Api.Common.Validation;

public sealed class ValidationFilter : IAsyncActionFilter
{
    public static IActionResult HataYaniti(ActionContext context) =>
        Yanit(context.HttpContext, context.ModelState, "Gönderilen bilgilerde hata var. İşaretli alanları düzeltin.");

    public static IActionResult BaglamaHataYaniti(ActionContext context)
    {
        var parametreler = context.ActionDescriptor.Parameters.Select(p => p.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var alanlar = new ModelStateDictionary();
        foreach (var (anahtar, kayit) in context.ModelState)
        {
            var alan = anahtar.StartsWith("$.", StringComparison.Ordinal) ? anahtar[2..] : anahtar;
            if (kayit.Errors.Count > 0 && alan is not ("" or "$") && !parametreler.Contains(alan))
            {
                alanlar.AddModelError(alan, "Bu alanın değeri geçersiz.");
            }
        }

        return Yanit(context.HttpContext, alanlar, "Gönderilen bilgiler okunamadı. Alanların biçimini kontrol edin.");
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

    private static BadRequestObjectResult Yanit(HttpContext httpContext, ModelStateDictionary modelState, string detail)
    {
        var problemDetailsFactory = httpContext.RequestServices.GetRequiredService<ProblemDetailsFactory>();
        var problem = problemDetailsFactory.CreateValidationProblemDetails(httpContext, modelState, StatusCodes.Status400BadRequest, detail: detail);
        problem.Errors = problem.Errors.ToDictionary(e => JsonNamingPolicy.CamelCase.ConvertName(e.Key), e => e.Value);

        return new BadRequestObjectResult(problem) { ContentTypes = { "application/problem+json" } };
    }
}
