using Azure.Extensions.AspNetCore.Configuration.Secrets;
using Azure.Security.KeyVault.Secrets;

namespace HoneyHub.Users.Api.Infrastructure;

public sealed class DualPrefixKeyVaultSecretManager : KeyVaultSecretManager
{
    private readonly string _orgPrefixWithSep;
    private readonly string _svcEnvPrefixWithSep;

    public DualPrefixKeyVaultSecretManager(string orgPrefix, string serviceEnvPrefix)
    {
        _orgPrefixWithSep = (orgPrefix ?? "Org") + "--";
        _svcEnvPrefixWithSep = serviceEnvPrefix + "--";
    }

    public override bool Load(SecretProperties sp) =>
        sp.Name.StartsWith(_orgPrefixWithSep, StringComparison.OrdinalIgnoreCase) ||
        sp.Name.StartsWith(_svcEnvPrefixWithSep, StringComparison.OrdinalIgnoreCase);

    public override string GetKey(KeyVaultSecret secret)
    {
        var name = secret.Name.StartsWith(_orgPrefixWithSep, StringComparison.OrdinalIgnoreCase)
            ? secret.Name[_orgPrefixWithSep.Length..]
            : secret.Name[_svcEnvPrefixWithSep.Length..];

        return name.Replace("--", ":");
    }
}
