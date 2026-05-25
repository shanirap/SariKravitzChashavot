namespace AccountingProject.Infrastructure
{
    public static class WebApplicationBuilderCorsExtensions
    {
        /// <summary>
        /// Registers the "ReactClient" CORS policy from AllowedOrigins.
        /// Development: empty AllowedOrigins allows any localhost / 127.0.0.1 origin (any port) for local SPAs.
        /// Non-development: uses only the configured AllowedOrigins list (validated at startup).
        /// </summary>
        public static void AddConfiguredCors(this WebApplicationBuilder builder)
        {
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("ReactClient", policy =>
                {
                    policy.AllowAnyHeader().AllowAnyMethod();

                    var origins = HostEnvironmentConfigurationValidator.GetAllowedOrigins(builder.Configuration);

                    if (builder.Environment.IsDevelopment())
                    {
                        if (origins.Length > 0)
                        {
                            policy.WithOrigins(origins);
                        }
                        else
                        {
                            policy.SetIsOriginAllowed(static origin =>
                            {
                                if (string.IsNullOrWhiteSpace(origin)) return false;
                                try
                                {
                                    var uri = new Uri(origin);
                                    var hostOk = uri.Host.Equals("localhost", StringComparison.OrdinalIgnoreCase)
                                        || uri.Host == "127.0.0.1";
                                    return hostOk && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
                                }
                                catch (UriFormatException)
                                {
                                    return false;
                                }
                            });
                        }
                    }
                    else
                    {
                        policy.WithOrigins(origins);
                    }
                });
            });
        }
    }
}
