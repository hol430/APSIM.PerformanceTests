using System;
using System.IO;
using System.Text.Json;

namespace APSIM.POStats.Shared
{
    public class Vault
    {
        /// <summary>Read a value from the application vault.</summary>
        /// <param name="key">The key identifying the value to read</param>
        public static string Read(string key)
        {
            // locate the vault.
            var vaultDirectory = Directory.GetCurrentDirectory();
            var vaultFileName = Path.Combine(vaultDirectory, "..", "Vault.json");
            while (!File.Exists(vaultFileName) && vaultDirectory != Path.GetPathRoot(vaultDirectory))
            {
                vaultDirectory = Directory.GetParent(vaultDirectory).FullName;
                vaultFileName = Path.Combine(vaultDirectory, "..", "Vault.json");
            }
            if (!File.Exists(vaultFileName))
                throw new Exception($"Cannot find application vault {vaultFileName}.");

            // Read from vault.
            var options = new JsonDocumentOptions { AllowTrailingCommas = true };
            using (JsonDocument document = JsonDocument.Parse(File.ReadAllText(vaultFileName), options))
            {
                if (!document.RootElement.TryGetProperty(key, out JsonElement element))
                    throw new Exception($"Cannot find key {key} in vault");
                return element.GetString();
            }
        }
    }
}