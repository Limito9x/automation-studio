using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Wolverine;

namespace Automation.SharedKernel.Abstractions.Modules;

public interface IModule
{
    string Name { get; }
    string SchemaName { get; }
    void ConfigureServices(IServiceCollection services, IConfiguration config);
    void ConfigureWolverine(WolverineOptions options);
    List<Type>? Endpoints => null;
}


