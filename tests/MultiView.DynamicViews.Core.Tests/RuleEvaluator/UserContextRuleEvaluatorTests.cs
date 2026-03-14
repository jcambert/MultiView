using MultiView.DynamicViews.Core.Abstractions;
using MultiView.DynamicViews.Core.RuleEvaluator;
using MultiView.DynamicViews.Core.Services;

namespace MultiView.DynamicViews.Core.Tests.RuleEvaluator;

public class UserContextRuleEvaluatorTests
{
    private readonly DefaultRuleEvaluator _evaluator = new();
    private readonly ReflectionRecordPropertyAccessor _propertyAccessor = new();

    [Fact]
    public void Evaluate_WithAuthenticatedUserRole_ReturnsTrue()
    {
        RecordRuleValueAccessor context = CreateContext(
            new { Amount = 150m },
            new RuleUserContext
            {
                IsAuthenticated = true,
                UserId = "u-123",
                UserName = "alice",
                Roles = new HashSet<string>(["Admin"], StringComparer.OrdinalIgnoreCase),
                Claims = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                {
                    ["department"] = "sales"
                }
            });

        bool result = _evaluator.Evaluate("user.isAuthenticated == true && user.roles.Admin == true", context);

        Assert.True(result);
    }

    [Fact]
    public void Evaluate_WithClaimComparisonAndRecordValue_ReturnsTrue()
    {
        RecordRuleValueAccessor context = CreateContext(
            new { Amount = 1000m },
            new RuleUserContext
            {
                IsAuthenticated = true,
                UserId = "u-123",
                UserName = "alice",
                Roles = new HashSet<string>(["Manager"], StringComparer.OrdinalIgnoreCase),
                Claims = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                {
                    ["department"] = "sales"
                }
            });

        bool result = _evaluator.Evaluate("user.claims.department == 'sales' && Amount >= 1000", context);

        Assert.True(result);
    }

    [Fact]
    public void Evaluate_WithMissingUserClaim_ReturnsFalse()
    {
        RecordRuleValueAccessor context = CreateContext(
            new { Amount = 50m },
            new RuleUserContext
            {
                IsAuthenticated = true,
                Roles = new HashSet<string>(StringComparer.OrdinalIgnoreCase),
                Claims = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            });

        bool result = _evaluator.Evaluate("user.claims.department == 'sales'", context);

        Assert.False(result);
    }

    private RecordRuleValueAccessor CreateContext(object record, RuleUserContext user)
    {
        return new RecordRuleValueAccessor(_propertyAccessor, record, null, user);
    }
}
