using Azure.Security.KeyVault.Secrets;

namespace CityInfo.API
{
    public static class KeyVaultConfigLoader
    {
        public static async Task LoadSecretsIntoConfiguration(WebApplicationBuilder builder)
        {
            try
            {
                using var tempProvider = builder.Services.BuildServiceProvider();
                var secretClient = tempProvider.GetRequiredService<SecretClient>();

                // List of required secrets
                var requiredSecrets = new[] { "DatabasePassword", "ApiKey", "JwtSecret" };

                foreach (var secretName in requiredSecrets)
                {
                    try
                    {
                        var secret = await secretClient.GetSecretAsync(secretName);
                        builder.Configuration[$"Secrets:{secretName}"] = secret.Value.Value;
                    }
                    catch (Exception ex)
                    {
                        // Log error or throw if secret is required
                        Console.WriteLine($"Failed to load secret {secretName}: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Key Vault initialization failed: {ex.Message}");
                throw;
            }
        }
    }
}
