using System;
using Azure.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace CanLove_Backend.Core.DataProtection
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

            // 1) 若提供 Client Secret，優先使用（便於在本機或 CI 指定服務主體）
            var clientId = configuration["KeyVault:ClientId"];
            var clientSecret = configuration["KeyVault:ClientSecret"];
            var tenantId = configuration["KeyVault:TenantId"];
            if (!string.IsNullOrEmpty(clientId) && !string.IsNullOrEmpty(clientSecret) && !string.IsNullOrEmpty(tenantId))
            {
                credential = new ClientSecretCredential(tenantId, clientId, clientSecret);
            }
            else
            {
                // 2) 否則走 DefaultAzureCredential（支援 az login / VS 登入 / Managed Identity）
                var defaultOptions = new DefaultAzureCredentialOptions();
                var userAssignedManagedIdentityClientId = configuration["ManagedIdentityClientId"];
                if (!string.IsNullOrEmpty(userAssignedManagedIdentityClientId))
                {
                    defaultOptions.ManagedIdentityClientId = userAssignedManagedIdentityClientId;
                }
                credential = new DefaultAzureCredential(defaultOptions);
            }

            configuration.AddAzureKeyVault(new Uri(keyVaultUri), credential);
        }
    }
}


