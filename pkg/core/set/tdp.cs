using System.Reflection;
using Dalamud.Data;
using Dalamud.Game.Command;
using Dalamud.Game.Gui;
using Dalamud.IoC;
using Dalamud.Plugin;
using FFXIVClientStructs;
using XIVToDo.Managers;
using SigScanner = Dalamud.Game.SigScanner;

namespace XIVToDo
{
    public class ToDoPlugin : IDalamudPlugin
    {
        private const string Command = "/todo";

        private readonly UI _ui;
        [PluginService] private DataManager DataManager { get; set; }
        [PluginService] private GameGui GameGui { get; set; }
        [PluginService] private SigScanner SigScanner { get; set; }

        private readonly GoldSaucerManager _goldSaucerManager;
        private readonly TeleportManager _teleportManager;

        public ToDoPlugin()
        {
            Resolver.Initialize();
            var config = PluginInterface.Create<Configuration>();
            var texStore = PluginInterface.Create<TextureStore>();
            var teleportTexture = texStore.GetTexture(111);
            _teleportManager = PluginInterface.Create<TeleportManager>();
            var beastTribeManager = PluginInterface.Create<BeastTribeManager>(teleportTexture, _teleportManager);
            _goldSaucerManager = PluginInterface.Create<GoldSaucerManager>(teleportTexture, _teleportManager);
            _ui = PluginInterface.Create<UI>(config, texStore, beastTribeManager, _goldSaucerManager);

            CommandManager.AddHandler(Command, new CommandInfo(OnCommand)
            {
                HelpMessage = "Open the ToDo window"
            });

            PluginInterface.UiBuilder.OpenConfigUi += () => { OnCommand(null, null); };
            PluginInterface.UiBuilder.Draw += DrawUI;
        }

        [PluginService] private DalamudPluginInterface PluginInterface { get; set; }

        [PluginService] private CommandManager CommandManager { get; set; }

        public string AssemblyLocation { get; set; } = Assembly.GetExecutingAssembly().Location;

        public void Dispose()
        {
            _ui?.Dispose();
            _goldSaucerManager?.Dispose();
            CommandManager.RemoveHandler(Command);
            PluginInterface?.Dispose();
        }

        public string Name => "XIVTODO List";

        private void DrawUI()
        {
            _ui.Draw();
        }

        private void OnCommand(string command, string args)
        {
            _ui.Visible = !_ui.Visible;
        }

        internal static string GetID(string spec)
        {
            return $"#xivToDo{spec}";
        }
    }
}