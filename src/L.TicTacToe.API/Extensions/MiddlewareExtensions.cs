internal static class MiddlewareExtensions
{
    public static void MapApplicationServicesEndpoints(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();

            app.MapHealthChecks("/health");
        }
    }
}
