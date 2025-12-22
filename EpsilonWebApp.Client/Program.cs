using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using EpsilonWebApp.Services.AuthorizationService;
using Microsoft.AspNetCore.Components.Authorization;
using EpsilonWebApp.Client.Handlers;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

// Configure HttpClient with base address
builder.Services.AddScoped(sp => new HttpClient
{
    BaseAddress = new Uri(builder.HostEnvironment.BaseAddress)
});

// Add Authorization services
builder.Services.AddAuthorizationCore();

// Register authentication services
// Note: sessionStorage no longer needed - using HttpOnly cookies
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthenticationStateProvider>();

// Register the Authorization Message Handler
builder.Services.AddTransient<AuthorizationMessageHandler>();

// Configure HttpClient with the handler
builder.Services.AddHttpClient("API", client =>
{
    client.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress);
})
.AddHttpMessageHandler<AuthorizationMessageHandler>();

builder.Services.AddScoped<EpsilonWebApp.Services.CustomerService.ICustomerService, 
    EpsilonWebApp.Services.CustomerService.CustomerService>();

await builder.Build().RunAsync();
