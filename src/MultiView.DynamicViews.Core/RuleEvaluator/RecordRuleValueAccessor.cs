using System.Collections.Generic;
using MultiView.DynamicViews.Core.Abstractions;

namespace MultiView.DynamicViews.Core.RuleEvaluator;

public sealed class RecordRuleValueAccessor : IRuleValueAccessor
{
    private readonly IRecordPropertyAccessor _propertyAccessor;
    private readonly object? _record;
    private readonly IReadOnlyDictionary<string, object?> _global;

    public RecordRuleValueAccessor(
        IRecordPropertyAccessor propertyAccessor,
        object? record,
        IReadOnlyDictionary<string, object?>? globalContext = null)
    {
        _propertyAccessor = propertyAccessor;
        _record = record;
        _global = globalContext ?? new Dictionary<string, object?>();
    }

    public bool TryGetValue(string identifier, out object? value)
    {
        if (_global.TryGetValue(identifier, out value))
        {
            return true;
        }

        if (identifier.StartsWith("global.", StringComparison.OrdinalIgnoreCase))
        {
            string globalKey = identifier.Substring("global.".Length);
            return _global.TryGetValue(globalKey, out value);
        }

        value = _propertyAccessor.GetValue(_record, identifier);
        return true;
    }
}
