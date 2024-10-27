using Dalamud.Game.ClientState;
using Dalamud.Game.Gui;
using Dalamud.IoC;
using Dalamud.Logging;
using FFXIVClientStructs.FFXIV.Client.Game.UI;

namespace XIVToDo
{
    public unsafe class TeleportManager
    {
        [PluginService] private ClientState ClientState { get; set; }
        [PluginService] private ChatGui ChatGui { get; set; }
        public bool Teleport(uint aetheryteID)
        {
            if (ClientState.LocalPlayer == null)
                return false;

            var telepo = Telepo.Instance();
            if (telepo == null)
                return false;

            if (telepo->TeleportList.Size() == 0)
                telepo->UpdateAetheryteList();

            var endPtr = telepo->TeleportList.Last;
            for (var it = telepo->TeleportList.First; it != endPtr; ++it)
            {
                if (it->AetheryteId == aetheryteID)
                    return telepo->Teleport(aetheryteID, 0);
            }
            
            PluginLog.LogError("Error trying to teleport");
            return false;
        }
    }
}