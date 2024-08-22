using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PluggableModulesInterface
{
    public interface IPluggableModule : IDisposable
    {
        int Key { get; }
        string Name { get; }
        string Type { get; }
        bool Running { get; }
        int InputQueueCount { get; }
        int OutputQueueCount { get; }
        string AssemblyLocation { set; }

        void Start();
        void Stop();
        void WakeUp();
        string GetAssemblyLocation();
    }
}
