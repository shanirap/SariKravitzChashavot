namespace AccountingProject.Infrastructure
{
    /// <summary>
    /// Validates configuration when the host environment requires stricter settings (production and non-development CORS).
    /// </summary>
    public static class HostEnvironmentConfigurationValidator
    {
        private static readonly string[] NonDevelopmentOriginPlaceholderFragments =
        [
            "replace-with",
            "placeholder",
            "example.com",
            "localhost",
            "127.0.0.1"
        ];

        /// <summary>
        /// Known JWT signing key material from development templates; must never be used in Production.
        /// </summary>
        public const string DevelopmentJwtKeyPlaceholder = "DEV_ONLY_REPLACE_IN_PRODUCTION";

        public static void Validate(IHostEnvironment environment, IConfiguration configuration)
        {
            var origins = GetAllowedOrigins(configuration);

            if (!environment.IsDevelopment() && origins.Length == 0)
            {
                throw new InvalidOperationException(
                    "Configuration error: AllowedOrigins must list at least one http(s) URL for the SPA when not running in the Development environment.");
            }

            if (!environment.IsProduction())
                return;

            var conn = configuration.GetConnectionString("DefaultConnection");
            if (string.IsNullOrWhiteSpace(conn))
            {
                throw new InvalidOperationException(
                    "Production configuration error: ConnectionStrings:DefaultConnection is missing or empty. Configure it via environment variables, appsettings.Production.json, or your deployment secrets store.");
            }

            var jwtKey = configuration["Jwt:Key"];
            if (string.IsNullOrWhiteSpace(jwtKey))
            {
                throw new InvalidOperationException(
                    "Production configuration error: Jwt:Key is missing or empty. Set a strong symmetric key via environment variables or a secrets store (do not commit production keys).");
            }

            if (string.Equals(jwtKey.Trim(), DevelopmentJwtKeyPlaceholder, StringComparison.Ordinal))
            {
                throw new InvalidOperationException(
                    $"Production configuration error: Jwt:Key must not equal the development placeholder '{DevelopmentJwtKeyPlaceholder}'.");
            }

            foreach (var origin in origins)
            {
                if (!Uri.TryCreate(origin, UriKind.Absolute, out var uri))
                {
                    throw new InvalidOperationException(
                        $"Production configuration error: AllowedOrigins value '{origin}' must be a valid absolute http or https URL.");
                }

                if (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps)
                {
                    throw new InvalidOperationException(
                        $"Production configuration error: AllowedOrigins value '{origin}' must use http or https.");
                }

                var normalizedOrigin = origin.ToLowerInvariant();
                if (NonDevelopmentOriginPlaceholderFragments.Any(fragment => normalizedOrigin.Contains(fragment, StringComparison.Ordinal)))
                {
                    throw new InvalidOperationException(
                        $"Production configuration error: AllowedOrigins value '{origin}' looks like a placeholder or local-only origin. Configure the real internal SPA URL.");
                }
            }
        }

        public static string[] GetAllowedOrigins(IConfiguration configuration)
        {
            var section = configuration.GetSection("AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();
            return section
                .Where(o => !string.IsNullOrWhiteSpace(o))
                .Select(o => o.Trim())
                .Distinct(StringComparer.Ordinal)
                .ToArray();
        }
    }
}
