using Microsoft.AspNetCore.Components;
using MultiView.DynamicViews.Core.Abstractions;
using MultiView.DynamicViews.Domain.Model;

namespace MultiView.DynamicViews.Blazor.Components.Fields;

public abstract class FieldWidgetBase : ComponentBase
{
    [Inject]
    protected IRecordPropertyAccessor RecordPropertyAccessor { get; set; } = null!;

    [Parameter]
    public object Record { get; set; } = null!;

    [Parameter]
    public ViewFieldDefinition Field { get; set; } = null!;

    [Parameter]
    public bool IsEnabled { get; set; } = true;

    [Parameter]
    public bool IsReadOnly { get; set; }

    [Parameter]
    public bool IsRequired { get; set; }

    protected string Label => string.IsNullOrWhiteSpace(Field.Label) ? Field.Name : Field.Label;

    protected object? GetValue()
    {
        return RecordPropertyAccessor.GetValue(Record, Field.Name);
    }

    protected void SetValue(object? value)
    {
        RecordPropertyAccessor.SetValue(Record, Field.Name, value);
    }
}
