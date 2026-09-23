using System.Reflection;
using Automation.SharedKernel.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Automation.SharedKernel.Infrastructure.Persistence;

public static class ModelBuilderExtensions
{
    public static void ApplySharedKernelConfigurations(this ModelBuilder modelBuilder)
    {
        modelBuilder.HasPostgresExtension("unaccent");
        modelBuilder.HasCollation("case_insensitive", locale: "und-u-ks-level2", provider: "icu", deterministic: false);
        modelBuilder.UseCollation("case_insensitive");
        modelBuilder.ApplySoftDeleteQueryFilter();
    }

    public static void ApplySoftDeleteQueryFilter(this ModelBuilder modelBuilder)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(ISoftDelete).IsAssignableFrom(entityType.ClrType))
            {
                var method = typeof(ModelBuilderExtensions)
                    .GetMethod(nameof(SetSoftDeleteFilter), BindingFlags.NonPublic | BindingFlags.Static)
                    ?.MakeGenericMethod(entityType.ClrType);

                method?.Invoke(null, new object[] { modelBuilder });
            }
        }
    }

    private static void SetSoftDeleteFilter<TEntity>(ModelBuilder modelBuilder) where TEntity : class, ISoftDelete
    {
        modelBuilder.Entity<TEntity>().HasQueryFilter(x => x.DeletedAt == null);
    }
}



