using Tombatron.Turbo;
using Tombatron.Turbo.Dashboard;

var builder = WebApplication.CreateBuilder(new WebApplicationOptions
{
    Args = args,
    EnvironmentName = Environments.Development
});

// Add Turbo services
builder.Services.AddTurbo(options =>
{
    options.HubPath = "/turbo-hub";
});

// Register our metrics service and background updater
builder.Services.AddSingleton<MetricsService>();
builder.Services.AddHostedService<MetricsUpdater>();

builder.Services.AddRazorPages();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseTurbo();
app.UseAuthorization();

app.MapRazorPages();
app.MapTurboHub();

app.Run();
