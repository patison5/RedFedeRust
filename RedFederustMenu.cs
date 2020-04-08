using System;
using System.Collections.Generic;
using Oxide.Core;
using Newtonsoft.Json;
using System.Linq;
using Newtonsoft.Json.Serialization;

using Oxide.Core.Plugins;
using Oxide.Game.Rust.Cui;
using UnityEngine;
using Oxide.Core.Libraries;
using Newtonsoft.Json.Linq;


namespace Oxide.Plugins
{
    [Info("RedFederustMenu", "Lulex.py", "0.0.1")]
    internal class RedFederustMenu : RustPlugin
    {

        [PluginReference] private Plugin NTeleportation;

        private void OnServerInitialized()
        { 
            if (!NTeleportation)
            {
                PrintError("Donwload and install NTeleportation to work with this plugin...");
            }


            testt();
        }


        [ConsoleCommand("testt")]
        private void testt ()
        {
            var FOX_MD  = FindBasePlayer("76561198052209301");
            var FOGHOST = FindBasePlayer("76561198033885552");

            if (FOX_MD == null || FOGHOST == null) return;

            Puts("trying to teleport");

            /*FOGHOST.SendConsoleCommand($"chat.say привет {FOX_MD.displayName}");
            FOGHOST.SendConsoleCommand($"chat.say привет \"{FOX_MD.displayName}, просто здравствуй!\"");
*/
            FOGHOST.SendConsoleCommand($"chat.say /tpr {FOX_MD.displayName}");
        }

        public BasePlayer FindBasePlayer(string nameOrUserId)
        {
            nameOrUserId = nameOrUserId.ToLower();
            foreach (var player in BasePlayer.activePlayerList)
            {
                if (player.displayName.ToLower().Contains(nameOrUserId) || player.UserIDString == nameOrUserId)
                    return player;
            }
            foreach (var player in BasePlayer.sleepingPlayerList)
            {
                if (player.displayName.ToLower().Contains(nameOrUserId) || player.UserIDString == nameOrUserId)
                    return player;
            }
            return default(BasePlayer);
        }
    }
}
        