using MultiView.DynamicViews.Blazor;
using MultiView.DynamicViews.Core;
using MultiView.DynamicViews.Core.Abstractions;
using MultiView.DynamicViews.Sample.Actions;
using MultiView.DynamicViews.Sample.Data;
using MultiView.DynamicViews.Sample.Models;
using System.Text.Json;
using MudBlazor.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddMudServices();
builder.Services.AddDynamicViewsMudBlazor();
builder.Services.AddDynamicViewActionHandler<ToggleConfirmActionHandler>();
builder.Services.AddSingleton<IDataProvider<SaleOrder>, SaleOrderDataProvider>();

string definitionsFolder = Path.Combine(builder.Environment.ContentRootPath, "Definitions");
Dictionary<string, string> definitions = Directory.EnumerateFiles(definitionsFolder, "saleorder-*.json")
    .Select(File.ReadAllText)
    .Select(json => new { Json = json, Id = ExtractDefinitionId(json) })
    .Where(item => !string.IsNullOrWhiteSpace(item.Id))
    .ToDictionary(item => item.Id!, item => item.Json, StringComparer.OrdinalIgnoreCase);

builder.Services.AddDynamicViewDefinitions(definitions);

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();
app.UseAntiforgery();
app.MapStaticAssets();
app.MapRazorComponents<MultiView.DynamicViews.Sample.Components.App>()
    .AddInteractiveServerRenderMode();

app.Run();

static string? ExtractDefinitionId(string json)
{
    using JsonDocument document = JsonDocument.Parse(json);
    return document.RootElement.TryGetProperty("id", out JsonElement id) ? id.GetString() : null;
}
