using MultiView.DynamicViews.Core.Validation;

namespace MultiView.DynamicViews.Core.Tests.Validation;

public class JsonViewDefinitionValidatorTests
{
    [Fact]
    public void Validate_WithValidListDefinition_ReturnsNoErrors()
    {
        JsonViewDefinitionValidator validator = new(new ViewDefinitionValidationOptions());

        string json = """
        {
          "id": "saleorder_list",
          "model": "SaleOrder",
          "name": "saleorder.list",
          "kind": "List",
          "fields": [
            { "name": "Number", "kind": "Text" },
            { "name": "Amount", "kind": "Currency" }
          ],
          "columns": [
            { "field": "Number", "header": "N°" },
            { "field": "Amount", "header": "Montant" }
          ],
          "defaultPageSize": 20
        }
        """;

        ViewDefinitionValidationResult result = validator.Validate(json);

        Assert.False(result.HasErrors);
        Assert.Empty(result.Warnings);
    }

    [Fact]
    public void Validate_WithUnknownRootField_DefaultModeReturnsError()
    {
        JsonViewDefinitionValidator validator = new(new ViewDefinitionValidationOptions());

        string json = """
        {
          "id": "saleorder_form",
          "model": "SaleOrder",
          "name": "saleorder.form",
          "kind": "Form",
          "fields": [],
          "unexpected": true
        }
        """;

        ViewDefinitionValidationResult result = validator.Validate(json);

        Assert.Contains(result.Errors, issue => issue.Path == "$.unexpected");
    }

    [Fact]
    public void Validate_WithUnknownRootField_WarningModeReturnsWarningOnly()
    {
        JsonViewDefinitionValidator validator = new(new ViewDefinitionValidationOptions
        {
            UnknownFieldHandling = UnknownJsonFieldHandling.Warning
        });

        string json = """
        {
          "id": "saleorder_form",
          "model": "SaleOrder",
          "name": "saleorder.form",
          "kind": "Form",
          "fields": [],
          "unexpected": true
        }
        """;

        ViewDefinitionValidationResult result = validator.Validate(json);

        Assert.False(result.HasErrors);
        Assert.Contains(result.Warnings, issue => issue.Path == "$.unexpected");
    }

    [Fact]
    public void Validate_WithUnknownGraphReference_ReturnsError()
    {
        JsonViewDefinitionValidator validator = new(new ViewDefinitionValidationOptions());

        string json = """
        {
          "id": "saleorder_graph",
          "model": "SaleOrder",
          "name": "saleorder.graph",
          "kind": "Graph",
          "fields": [
            { "name": "Status", "kind": "Select" }
          ],
          "categoryField": "Status",
          "valueField": "Amount"
        }
        """;

        ViewDefinitionValidationResult result = validator.Validate(json);

        Assert.Contains(result.Errors, issue => issue.Path == "$.valueField");
    }

    [Fact]
    public void Validate_WithDuplicateFieldNames_ReturnsError()
    {
        JsonViewDefinitionValidator validator = new(new ViewDefinitionValidationOptions());

        string json = """
        {
          "id": "saleorder_search",
          "model": "SaleOrder",
          "name": "saleorder.search",
          "kind": "Search",
          "fields": [
            { "name": "Status", "kind": "Select" },
            { "name": "Status", "kind": "Text" }
          ]
        }
        """;

        ViewDefinitionValidationResult result = validator.Validate(json);

        Assert.Contains(result.Errors, issue => issue.Path == "$.fields[1].name");
    }
}
