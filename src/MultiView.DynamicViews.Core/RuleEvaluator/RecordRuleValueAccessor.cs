using System.Collections.Generic;
using MultiView.DynamicViews.Core.Abstractions;

namespace MultiView.DynamicViews.Core.RuleEvaluator;

public sealed class RecordRuleValueAccessor : IRuleValueAccessor
{
    private const string GlobalPrefix = "global.";
    private const string UserPrefix = "user.";

    private readonly IRecordPropertyAccessor _propertyAccessor;
    private readonly object? _record;
    private readonly IReadOnlyDictionary<string, object?> _global;
    private readonly RuleUserContext _user;

    public RecordRuleValueAccessor(
        IRecordPropertyAccessor propertyAccessor,
        object? record,
        IReadOnlyDictionary<string, object?>? globalContext = null,
        RuleUserContext? userContext = null)
    {
        _propertyAccessor = propertyAccessor;
        _record = record;
        _global = globalContext ?? new Dictionary<string, object?>();
        _user = userContext ?? RuleUserContext.Empty;
    }

    public bool TryGetValue(string identifier, out object? value)
    {
        if (_global.TryGetValue(identifier, out value))
        {
            return true;
        }

        if (identifier.StartsWith(GlobalPrefix, StringComparison.OrdinalIgnoreCase))
        {
            string globalKey = identifier.Substring(GlobalPrefix.Length);
            return _global.TryGetValue(globalKey, out value);
        }

        if (identifier.StartsWith(UserPrefix, StringComparison.OrdinalIgnoreCase))
        {
            return TryGetUserValue(identifier.Substring(UserPrefix.Length), out value);
        }

        value = _propertyAccessor.GetValue(_record, identifier);
        return true;
    }

    private bool TryGetUserValue(string userPath, out object? value)
    {
        if (string.IsNullOrWhiteSpace(userPath))
        {
            value = _user;
            return true;
        }

        string[] parts = userPath.Split('.', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 0)
        {
            value = _user;
            return true;
        }

        if (parts[0].Equals("isAuthenticated", StringComparison.OrdinalIgnoreCase))
        {
            value = _user.IsAuthenticated;
            return true;
        }

        if (parts[0].Equals("id", StringComparison.OrdinalIgnoreCase))
        {
            value = _user.UserId;
            return true;
        }

        if (parts[0].Equals("name", StringComparison.OrdinalIgnoreCase))
        {
            value = _user.UserName;
            return true;
        }

        if (parts[0].Equals("roles", StringComparison.OrdinalIgnoreCase))
        {
            if (parts.Length == 1)
            {
                value = _user.Roles;
                return true;
            }

            value = _user.Roles.Contains(parts[1]);
            return true;
        }

        if (parts[0].Equals("claims", StringComparison.OrdinalIgnoreCase))
        {
            if (parts.Length == 1)
            {
                value = _user.Claims;
                return true;
            }

            value = _user.Claims.TryGetValue(parts[1], out string? claimValue) ? claimValue : null;
            return true;
        }

        value = null;
        return true;
    }
}
