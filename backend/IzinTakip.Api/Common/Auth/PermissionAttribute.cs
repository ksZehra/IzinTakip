using Microsoft.AspNetCore.Authorization;

namespace IzinTakip.Api.Common.Auth;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public sealed class PermissionAttribute(params string[] kodlar)
    : AuthorizeAttribute, IAuthorizationRequirement, IAuthorizationRequirementData
{
    public IReadOnlyList<string> Kodlar { get; } = kodlar;

    public IEnumerable<IAuthorizationRequirement> GetRequirements()
    {
        yield return this;
    }
}
