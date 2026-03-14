namespace MultiView.DynamicViews.Core.Validation;

public enum UnknownJsonFieldHandling
{
    Ignore,
    Warning,
    Error
}

public sealed class ViewDefinitionValidationOptions
{
    public UnknownJsonFieldHandling UnknownFieldHandling { get; set; } = UnknownJsonFieldHandling.Error;
}

public sealed class ViewDefinitionValidationIssue
{
    public required string Path { get; init; }

    public required string Message { get; init; }

    public override string ToString()
    {
        return $"{Path}: {Message}";
    }
}

public sealed class ViewDefinitionValidationResult
{
    private readonly List<ViewDefinitionValidationIssue> _errors = [];
    private readonly List<ViewDefinitionValidationIssue> _warnings = [];

    public IReadOnlyList<ViewDefinitionValidationIssue> Errors => _errors;

    public IReadOnlyList<ViewDefinitionValidationIssue> Warnings => _warnings;

    public bool HasErrors => _errors.Count > 0;

    public void AddError(string path, string message)
    {
        _errors.Add(new ViewDefinitionValidationIssue
        {
            Path = path,
            Message = message
        });
    }

    public void AddWarning(string path, string message)
    {
        _warnings.Add(new ViewDefinitionValidationIssue
        {
            Path = path,
            Message = message
        });
    }
}

public sealed class ViewDefinitionValidationException : InvalidOperationException
{
    public ViewDefinitionValidationException(ViewDefinitionValidationResult result)
        : base(BuildMessage(result))
    {
        Result = result;
    }

    public ViewDefinitionValidationResult Result { get; }

    private static string BuildMessage(ViewDefinitionValidationResult result)
    {
        List<string> messages = result.Errors
            .Select(issue => issue.ToString())
            .ToList();

        if (messages.Count == 0)
        {
            messages.Add("La définition de vue est invalide.");
        }

        return $"Validation JSON échouée:{Environment.NewLine}- {string.Join(Environment.NewLine + "- ", messages)}";
    }
}
