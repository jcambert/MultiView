using MultiView.DynamicViews.Core.Abstractions;
using System.Reflection;
using System.Collections.Generic;

namespace MultiView.DynamicViews.Core.Services;

public sealed class ReflectionRecordPropertyAccessor : IRecordPropertyAccessor
{
    public object? GetValue(object? instance, string path)
    {
        if (instance is null)
        {
            return null;
        }

        object? current = instance;
        string[] members = path.Split('.', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        foreach (string member in members)
        {
            if (current is IDictionary<string, object?> dictionary)
            {
                if (!dictionary.TryGetValue(member, out current))
                {
                    return null;
                }

                continue;
            }

            Type currentType = current.GetType();
            PropertyInfo? propertyInfo = currentType.GetProperty(member,
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase);

            if (propertyInfo is null)
            {
                return null;
            }

            current = propertyInfo.GetValue(current);
            if (current is null)
            {
                return null;
            }
        }

        return current;
    }

    public void SetValue(object? instance, string path, object? value)
    {
        if (instance is null)
        {
            return;
        }

        string[] members = path.Split('.', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        if (members.Length == 0)
        {
            return;
        }

        object? current = instance;
        for (int i = 0; i < members.Length - 1; i++)
        {
            current = GetValue(current, members[i]);
            if (current is null)
            {
                return;
            }
        }

        if (current is null)
        {
            return;
        }

        if (current is IDictionary<string, object?> writableDictionary)
        {
            writableDictionary[members[^1]] = value;
            return;
        }

        PropertyInfo? propertyInfo = current.GetType().GetProperty(members[^1],
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase);
        if (propertyInfo is null || !propertyInfo.CanWrite)
        {
            return;
        }

        if (value is null)
        {
            propertyInfo.SetValue(current, null);
            return;
        }

        Type targetType = propertyInfo.PropertyType;
        object? convertedValue = Convert.ChangeType(value, Nullable.GetUnderlyingType(targetType) ?? targetType);
        propertyInfo.SetValue(current, convertedValue);
    }
}
