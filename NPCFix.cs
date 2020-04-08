using Oxide.Core;
using System.Collections.Generic;
namespace Oxide.Plugins
{
    [Info("NPCFix", "OxideBro", "0.0.21")]
    class NPCFix : RustPlugin
    {
        void OnServerInitialized()
        {
            timer.Every(30f, () => CheckPlayers());
        }

        void CheckPlayers()
        {
            List<BasePlayer> FakeListed = new List<BasePlayer>();
            foreach(var p in BasePlayer.activePlayerList)
            {
                if (!p.userID.IsSteamId())
                    FakeListed.Add(p);
            }

            if (FakeListed.Count > 0)
            {
                FakeListed.ForEach(f =>
                {
                    if (BasePlayer.activePlayerList.Contains(f))
                    {
                        PrintWarning($"Fake player {f.displayName} ({f.userID}) was removed from Active Player List");
                        BasePlayer.activePlayerList.Remove(f);
                    }
                });
            }
        }
    }
}