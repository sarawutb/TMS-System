using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.AspNetCore.Components.Authorization;
using TmsSystem.BlazorWasm;
using TmsSystem.BlazorWasm.Services;
using TmsSystem.BlazorWasm.ViewModels;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped<LocalStorageService>();
builder.Services.AddScoped<ApiAuthenticationStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(sp => sp.GetRequiredService<ApiAuthenticationStateProvider>());
builder.Services.AddTransient<AuthHeaderHandler>();
builder.Services.AddScoped(sp =>
{
    var handler = sp.GetRequiredService<AuthHeaderHandler>();
    handler.InnerHandler = new HttpClientHandler();
    return new HttpClient(handler)
    {
        BaseAddress = new Uri(builder.Configuration["ApiBaseUrl"] ?? "http://localhost:5188/")
    };
});
builder.Services.AddScoped<AuthSessionService>();
builder.Services.AddScoped<TransportOrderService>();
builder.Services.AddScoped<ShipmentService>();

// ViewModels for MVVM refactoring
builder.Services.AddTransient<LoginViewModel>();
builder.Services.AddTransient<OrderListViewModel>();
builder.Services.AddTransient<OrderDetailViewModel>();
builder.Services.AddTransient<OrderFormViewModel>();
builder.Services.AddTransient<ShipmentListViewModel>();
builder.Services.AddTransient<ShipmentDetailViewModel>();

builder.Services.AddAuthorizationCore();
builder.Services.AddBlazorBootstrap();

await builder.Build().RunAsync();
