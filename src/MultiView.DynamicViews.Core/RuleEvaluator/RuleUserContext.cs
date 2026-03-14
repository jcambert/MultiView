namespace MultiView.DynamicViews.Core.Abstractions;

public sealed class RuleUserContext
{
    public static RuleUserContext Empty { get; } = new();

    public bool IsAuthenticated { get; init; }

    public string? UserId { get; init; }

    public string? UserName { get; init; }

    public IReadOnlySet<string> Roles { get; init; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

    public IReadOnlyDictionary<string, string> Claims { get; init; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
}

public sealed class EmptyRuleUserContextAccessor : IRuleUserContextAccessor
{
    public RuleUserContext GetCurrentUser()
    {
        return RuleUserContext.Empty;
    }
}
