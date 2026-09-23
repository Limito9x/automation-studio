using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;

namespace Automation.SharedKernel.Extensions.Observability;

public static class OpenTelemetryExtensions
{
    public static IHostApplicationBuilder AddOpenTelemetryServices(this IHostApplicationBuilder builder)
    {
        // Thu th?p logging
        builder.Logging.AddOpenTelemetry(logging =>
        {
            logging.IncludeFormattedMessage = true;
            logging.IncludeScopes = true;
        });

        // Metric - ch? s? s?c kh?e c?a server
        builder.Services.AddOpenTelemetry()
            .WithMetrics(metrics =>
            {
                // Metrics c?a aspnetcore
                metrics.AddAspNetCoreInstrumentation()
                       // Metrics c?a http client
                       .AddHttpClientInstrumentation()
                       // Metrics c?a runtime
                       .AddRuntimeInstrumentation();
            })
            // Trace - truy v?t lu?ng x? lý
            .WithTracing(tracing =>
            {
                // Trace c?a aspnetcore
                tracing.AddAspNetCoreInstrumentation()
                       // Trace c?a http client
                       .AddHttpClientInstrumentation()
                       // Trace c?a ef core
                       .AddEntityFrameworkCoreInstrumentation();
            });

        // Export Metrics và Trace d?n Collector
        var useOtlpExporter = !string.IsNullOrWhiteSpace(builder.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"]);

        if (useOtlpExporter)
        {
            builder.Services.AddOpenTelemetry().UseOtlpExporter();
        }

        return builder;
    }
}



