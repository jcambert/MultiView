using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using MultiView.DynamicViews.Core.Abstractions;

namespace MultiView.DynamicViews.Blazor.Services;

public sealed class BlazorAuthenticationRuleUserContextAccessor : IRuleUserContextAccessor
{
    private readonly AuthenticationStateProvider _authenticationStateProvider;

    public BlazorAuthenticationRuleUserContextAccessor(AuthenticationStateProvider authenticationStateProvider)
    {
        _authenticationStateProvider = authenticationStateProvider;
    }

    public RuleUserContext GetCurrentUser()
    {
        ClaimsPrincipal? user = _authenticationStateProvider
            .GetAuthenticationStateAsync()
            .GetAwaiter()
            .GetResult()
            .User;

        if (user?.Identity is null)
        {
            return RuleUserContext.Empty;
        }

        IReadOnlySet<string> roles = user.Claims
            .Where(claim => claim.Type == ClaimTypes.Role)
            .Select(claim => claim.Value)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        Dictionary<string, string> claims = user.Claims
            .GroupBy(claim => claim.Type, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(
                group => group.Key,
                group => group.First().Value,
                StringComparer.OrdinalIgnoreCase);

        return new RuleUserContext
        {
            IsAuthenticated = user.Identity.IsAuthenticated,
            UserId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? user.FindFirst("sub")?.Value,
            UserName = user.Identity.Name,
            Roles = roles,
            Claims = claims
        };
    }
}
