using System;
using Azure.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace CanLove_Backend.Extensions
{
    public static class KeyVaultConfigurationExtensions
    {
        public static void AddAzureKeyVaultWithIdentity(this ConfigurationManager configuration, IHostEnvironment environment)
        {
            var keyVaultUri = configuration["KeyVault:VaultUri"];
            if (string.IsNullOrEmpty(keyVaultUri))
            {
                return;
            }

            Azure.Core.TokenCredential credential;

            if (environment.IsProduction())
            {
                // 正式環境：使用 Managed Identity
                var defaultOptions = new DefaultAzureCredentialOptions();
                var userAssignedManagedIdentityClientId = configuration["ManagedIdentityClientId"];
                if (!string.IsNullOrEmpty(userAssignedManagedIdentityClientId))
                {
                    defaultOptions.ManagedIdentityClientId = userAssignedManagedIdentityClientId;
                }
                credential = new DefaultAzureCredential(defaultOptions);
            }
            else
            {
                // 開發環境：使用 Client Secret
                var clientId = configuration["KeyVault:ClientId"];
                var clientSecret = configuration["KeyVault:ClientSecret"];
                var tenantId = configuration["KeyVault:TenantId"];

                if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(clientSecret) || string.IsNullOrEmpty(tenantId))
                {
                    throw new InvalidOperationException("開發環境需要設定 KeyVault:ClientId, KeyVault:ClientSecret, KeyVault:TenantId");
                }

                credential = new ClientSecretCredential(tenantId, clientId, clientSecret);
            }

            configuration.AddAzureKeyVault(new Uri(keyVaultUri), credential);
        }
    }
}


