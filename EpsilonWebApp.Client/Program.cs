using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.AspNetCore.Components.Authorization;
using Blazored.SessionStorage;
using EpsilonWebApp.Client.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

// Add session storage
builder.Services.AddBlazoredSessionStorage();

// Add services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<AuthorizationMessageHandler>();

// Add HttpClient for general use (e.g., Login page)
builder.Services.AddScoped(sp => new HttpClient 
{ 
    BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) 
});

// Add typed HttpClient with authorization handler for CustomerService
builder.Services.AddHttpClient<ICustomerService, CustomerService>(client =>
{
    client.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress);
})
.AddHttpMessageHandler<AuthorizationMessageHandler>();

// Add authentication
builder.Services.AddAuthorizationCore();
builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthenticationStateProvider>();

await builder.Build().RunAsync();
