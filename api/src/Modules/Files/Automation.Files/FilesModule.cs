using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Automation.SharedKernel.Abstractions.Modules;
using Automation.Files.Infrastructure;
using Automation.Files.Infrastructure.Persistence;
using Automation.Files.Infrastructure.Storage;
using Automation.SharedKernel.Extensions.Modules;
using Wolverine;
using Amazon.S3;

namespace Automation.Files;

public class FilesModule : IModule
{
    public string Name => "Files";
    public string SchemaName => "files";

    public List<Type> Endpoints => [..DiscoveredTypes.All];
    

    public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddModuleDbContext<FilesDbContext>(configuration, SchemaName);
        
        services.AddScoped<IAssetApi, AssetApiService>();
        
        // R2 Storage
        services.Configure<R2Options>(configuration.GetSection("CloudflareR2"));
        services.AddScoped<IObjectStorageService, R2StorageService>();
        services.AddSingleton<IAmazonS3>(sp =>
        {
            var options = configuration.GetSection("CloudflareR2").Get<R2Options>();
            if (options == null) throw new Exception("ObjectStorage config is missing");

            var config = new AmazonS3Config
            {
                ServiceURL = options.ServiceUrl,
            };

            return new AmazonS3Client(options.AccessKey, options.SecretKey, config);
        });

        services.AddTransient<Jobs.FileJobs>();
        services.AddTransient<Jobs.TickerQJobs>();

        services.AddSingleton<AssetRegistry>();
    }

    public void ConfigureWolverine(WolverineOptions options)
    {
    }
}



