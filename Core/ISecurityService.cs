using System.Threading.Tasks;

namespace Wpc_SutilBox.Core
{
    public interface ISecurityService
    {
        Task<SecurityStatus> GetSecurityStatusAsync();
    }
}

