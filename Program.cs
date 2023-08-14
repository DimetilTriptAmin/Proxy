using System.Text.RegularExpressions;
using Proxy;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddHttpClient();

var app = builder.Build();

app.UseMiddleware<ProxyMiddleware>();

app.Run();