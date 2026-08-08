using System.Collections.Generic;
using System.Threading.Tasks;
using Wpc_SutilBox.Models;

namespace Wpc_SutilBox.Core
{
    public interface IPrivacyService
    {
        Task<IEnumerable<PrivacySetting>> GetPrivacySettingsAsync();
        Task<bool> UpdatePrivacySettingAsync(PrivacySetting setting, bool newValue);
    }
}

