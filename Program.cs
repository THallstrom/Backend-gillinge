using GillingeClassicCars.Handlers;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Extensions;
using Umbraco.StorageProviders.AzureBlob;


WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        builder => builder
        .AllowAnyOrigin()
        .AllowAnyMethod()
        .AllowAnyHeader());
});

builder.CreateUmbracoBuilder()
    .AddBackOffice()
    .AddWebsite()
    .AddDeliveryApi()
    .AddComposers()
    .AddAzureBlobMediaFileSystem()
    .AddNotificationHandler<MediaSavedNotification, MediaUploadedHandler>()
    .Build();

builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBodySize = 104857600; // 100 MB
    options.Limits.MaxResponseBufferSize = 104857600; // 100 MB
});


WebApplication app = builder.Build();

await app.BootUmbracoAsync();

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseUmbraco()
    .WithMiddleware(u =>
    {
        u.UseBackOffice();
        u.UseWebsite();
    })
    .WithEndpoints(u =>
    {
        u.UseBackOfficeEndpoints();
        u.UseWebsiteEndpoints();
    });

await app.RunAsync();
