using System;
using System.Security.Principal;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace Wpc_SutilBox.Core
{
    public class PermissionService : IPermissionService
    {
        [DllImport("shell32.dll")]
        private static extern bool IsUserAnAdmin();

        public bool IsAdministrator => IsUserAnAdmin();

        public async Task<bool> IsElevatedAsync()
        {
            try
            {
                bool elevated = await Task.Run(() =>
                {
                    var identity = WindowsIdentity.GetCurrent();
                    var principal = new WindowsPrincipal(identity);
                    return principal.IsInRole(WindowsBuiltInRole.Administrator);
                });
                return elevated;
            }
            catch
            {
                return IsUserAnAdmin();
            }
        }

        public async Task<(bool Success, string Message)> RequireElevationAsync()
        {
            bool elevated = await IsElevatedAsync();
            if (elevated)
            {
                return (true, "La aplicación tiene permisos de administrador.");
            }
            return (false, "Esta operación requiere permisos de administrador. Ejecute WPC-SutilBox como administrador.");
        }

        public async Task<bool> CanModifyRegistryAsync()
        {
            return await IsElevatedAsync();
        }

        public async Task<bool> CanManageServicesAsync()
        {
            return await IsElevatedAsync();
        }

        public async Task<bool> CanAccessWmiAsync()
        {
            try
            {
                // WMI puede leer sin elevación, pero escribir requiere admin
                // Verificamos si podemos acceder al namespace raíz
                bool connected = await Task.Run(() =>
                {
                    var scope = new System.Management.ManagementScope("\\\\.\\root\\cimv2");
                    scope.Connect();
                    return scope.IsConnected;
                });
                return connected;
            }
            catch
            {
                return false;
            }
        }
    }
}
