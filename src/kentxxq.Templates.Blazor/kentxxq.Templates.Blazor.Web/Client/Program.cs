using kentxxq.Templates.Blazor.UI.Interfaces;
using kentxxq.Templates.Blazor.Web.Client;
using kentxxq.Templates.Blazor.Web.Client.Services;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

// �Լ��ķ�������
builder.Services.AddSingleton<ITextService, TextService>();

await builder.Build().RunAsync();