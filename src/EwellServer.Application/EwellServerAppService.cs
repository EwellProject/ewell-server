using System;
using System.Collections.Generic;
using System.Text;
using EwellServer.Localization;
using Volo.Abp.Application.Services;

namespace EwellServer;

/* Inherit your application services from this class.
 */
public abstract class EwellServerAppService : ApplicationService
{
    protected EwellServerAppService()
    {
        LocalizationResource = typeof(EwellServerResource);
    }
}
