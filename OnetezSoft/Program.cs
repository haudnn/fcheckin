using BlazorDateRangePicker;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OnetezSoft.Hubs;
using OnetezSoft.Services;
using OnetezSoft.Shared.Component.DragDrop;
using System;
using System.Linq;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddCustomDragDrop();
builder.Services.AddWMBSC(); // Don't using Jquery

// Add service storage userList
builder.Services.AddScoped<HubService>((ctx) =>
{
  NavigationManager navigate = ctx.GetService<NavigationManager>();

  return new HubService(navigate);
});
builder.Services.AddSingleton<GlobalService>();
builder.Services.AddHostedService<IntervalPing>((ctx) =>
{
  IHubContext<UserHub> hubcontext = ctx.GetService<IHubContext<UserHub>>();
  GlobalService globalService = ctx.GetService<GlobalService>();

  return new IntervalPing(hubcontext, globalService);
});

builder.Services.AddCookiePolicy(options =>
{
  options.CheckConsentNeeded = context => true;
  options.MinimumSameSitePolicy = SameSiteMode.None;

});
builder.Services.AddHttpContextAccessor();

// Datepicker (Sẽ remove sau)
builder.Services.AddDateRangePicker(config =>
{
  config.Culture = System.Globalization.CultureInfo.GetCultureInfo("vi-VN");
  config.DateFormat = "dd/MM/yyyy";
  config.TimePicker24Hour = true;
  config.TimePickerIncrement = 5;
  config.Opens = SideType.Right;
  config.ApplyLabel = "Xác nhận";
  config.CancelLabel = "Hủy";
  config.ButtonClasses = "button is-small";
  config.ApplyButtonClasses = "is-link";
});

// Config ping server
builder.Services.AddServerSideBlazor(options =>
{
  options.DisconnectedCircuitMaxRetained = 150;
  options.DisconnectedCircuitRetentionPeriod = TimeSpan.FromMinutes(60);
  options.JSInteropDefaultCallTimeout = TimeSpan.FromSeconds(60);
  options.DetailedErrors = true;
}).AddHubOptions(options =>
{
  options.KeepAliveInterval = TimeSpan.FromSeconds(30);
  options.ClientTimeoutInterval = TimeSpan.FromMinutes(1);
  options.HandshakeTimeout = TimeSpan.FromSeconds(30);
  options.EnableDetailedErrors = true;
  options.MaximumReceiveMessageSize = 1 * 1024 * 1024; //1MB
});

// SignalR
builder.Services.AddResponseCompression(opts =>
{
  opts.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(
     new[] { "application/octet-stream" });
});

// https://learn.microsoft.com/vi-vn/aspnet/core/security/cors
const string MyAllowSpecificOrigins = "_myAllowSpecificOrigins";
builder.Services.AddCors(options =>
{
  options.AddPolicy(name: MyAllowSpecificOrigins,
                    builder =>
                    {
                      builder.WithOrigins("*");
                    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
  app.UseExceptionHandler("/Error");
  // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
  app.UseHsts();
}

app.UseResponseCompression();
app.MapHub<UserHub>("/userHub");

app.UseCookiePolicy();

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();
