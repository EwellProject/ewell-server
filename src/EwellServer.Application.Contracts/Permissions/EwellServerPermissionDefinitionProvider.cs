using EwellServer.Localization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;

namespace EwellServer.Permissions;

public class EwellServerPermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var myGroup = context.AddGroup(EwellServerPermissions.GroupName);
        //Define your own permissions here. Example:
        //myGroup.AddPermission(EwellServerPermissions.MyPermission1, L("Permission:MyPermission1"));
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<EwellServerResource>(name);
    }
}
