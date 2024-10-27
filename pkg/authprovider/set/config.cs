using Dalamud.Configuration;
using Dalamud.IoC;
using Dalamud.Plugin;

namespace XIVToDo
{
    public class Configuration : IPluginConfiguration
    {
        [PluginService]
        [RequiredVersion("1.0")]
        public static DalamudPluginInterface PluginInterface { get; private set; }

        public int Version { get; set; } = 0;
        public bool BeastTribesComplete { get; set; } = false;
    }
}