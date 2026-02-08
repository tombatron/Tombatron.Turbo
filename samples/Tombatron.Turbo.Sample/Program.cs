using Tombatron.Turbo;

var builder = WebApplication.CreateBuilder(args);

// Add Turbo services
builder.Services.AddTurbo(options =>
{
    options.HubPath = "/turbo-hub";
    options.RequireAuthentication = false;
});

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

// Use Turbo middleware
app.UseTurbo();

app.UseAuthorization();

app.MapRazorPages();

app.Run();
