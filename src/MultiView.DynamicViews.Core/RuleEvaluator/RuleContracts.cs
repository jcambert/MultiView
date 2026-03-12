namespace MultiView.DynamicViews.Core.Abstractions;

public interface IRecordPropertyAccessor
{
    object? GetValue(object? instance, string path);

    void SetValue(object? instance, string path, object? value);
}

public interface IRuleValueAccessor
{
    bool TryGetValue(string identifier, out object? value);
}

public interface IRuleEvaluator
{
    bool Evaluate(string? expression, IRuleValueAccessor context);

    Func<IRuleValueAccessor, bool> Compile(string expression);
}
