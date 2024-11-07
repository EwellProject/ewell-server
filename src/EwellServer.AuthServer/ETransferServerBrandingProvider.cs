using Volo.Abp.DependencyInjection;
using Volo.Abp.Ui.Branding;

namespace EwellServer.Auth;

[Dependency(ReplaceServices = true)]
public class EwellServerBrandingProvider : DefaultBrandingProvider
{
    public override string AppName => "EwellServer";
}
