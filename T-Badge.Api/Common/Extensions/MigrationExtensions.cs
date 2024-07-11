using Microsoft.EntityFrameworkCore;
using T_Badge.Persistence;

namespace T_Badge.Common.Extensions;

public static class MigrationExtensions
{
    public static IApplicationBuilder ApplyMigrations(this IApplicationBuilder app)
    {
        using var scope = app.ApplicationServices.CreateScope();
        using var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationContext>();

        dbContext.Database.Migrate();

        return app;
    }
}