using System;
using System.Threading.Tasks;
using XO.Requests;

namespace DEVICE_CORE
{
    public interface IDeviceApplication
    {
        void Initialize(string pluginPath);
        Task Run();
        Task Command(LinkDeviceActionType action);
        void Shutdown();
    }
}
