using System.Collections.Concurrent;
using Dalamud.Data;
using Dalamud.Interface;
using Dalamud.IoC;
using Dalamud.Logging;
using Dalamud.Plugin;
using Dalamud.Utility;
using ImGuiScene;

namespace XIVToDo
{
    public class TextureStore
    {
        [PluginService] private DataManager DataManager { get; set; }
        [PluginService] private DalamudPluginInterface PluginInterface { get; set; }
        private readonly ConcurrentDictionary<int, TextureWrap> _textureCache = new();

        public D3DTextureWrap GetTexture(int textureId)
        {
            if (textureId < 0)
            {
                PluginLog.LogError("Invalid IconID");
                return null;
            }

            if (_textureCache.TryGetValue(textureId, out var textureWrap))
            {
                return (D3DTextureWrap)textureWrap;
            }
            
            var tex = DataManager.GetIcon((uint)textureId);
            if (tex == null)
            {
                PluginLog.LogError("Unable to lookup icon");
                return null;
            }

            var texWrap = PluginInterface.UiBuilder.LoadImageRaw(tex.GetRgbaImageData(), tex.Header.Width, tex.Header.Height, 4);
            _textureCache.TryAdd(textureId, texWrap);

            return (D3DTextureWrap)texWrap;
        }
    }
}