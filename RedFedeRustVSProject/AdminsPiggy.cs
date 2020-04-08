using Newtonsoft.Json;
using Oxide.Core.Libraries.Covalence;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Oxide.Core;
using System;
using Oxide.Core.Configuration;
using System.Linq;
using UnityEngine;


namespace Oxide.Plugins
{
    [Info("AdminsPiggy", "Lulex.py", "0.0.1")]
    public class AdminsPiggy : RustPlugin
    {
        private const string permAdminsPiggyCreate = "AdminsPiggy.create";

        string _storagePrefab = "assets/content/vehicles/boats/rhib/subents/rhib_storage.prefab";
        string _boarPrefab = "assets/rust.ai/agents/boar/boar.prefab";

        string test = "assets/bundled/prefabs/autospawn/collectable/hemp/hemp-collectable.prefab";

        [ChatCommand("cpiggy")]
        void createPiggy(BasePlayer player, string command, string[] args)
        {
            if (player.net.connection.authLevel == 1 || player.net.connection.authLevel == 2)
            {
                player.SendConsoleCommand($"createPiggyFromConsole");
            }
            else
            {
                SendReply(player, $"дарова, { player.displayName },  <color=#FFEB3B>У тебя неи прав на эту команду!!</color>");
            }
        }


        [ConsoleCommand("createPiggyFromConsole")]
        private void createPiggyFromConsoleFunction(ConsoleSystem.Arg args)
        {
            BasePlayer player = args.Player();

            SendReply(player, $"trying to make a piggy...");

            var box = GameManager.server?.CreateEntity(test, player.transform.position);

            if (box == null) return;
            box.Spawn();
            box.SetParent(player);
            box.transform.localPosition = new Vector3(0f, 0.5f, 0f);
            box.transform.Rotate(new Vector3(0.0f, 0.0f, 0.0f));
            box.SendNetworkUpdateImmediate(true);


            var piggy = GameManager.server?.CreateEntity(test, player.transform.position);


            if (piggy == null) return;
            // piggy.Spawn();
            //piggy.SetParent(player);
            //piggy.transform.localPosition = new Vector3(0f, 0.85f, 1.75f);

            //piggy.transform.parent.SetParent(player.transform.position);
            // piggy.transform.LookAt(player.transform);
            // piggy.TickFollowPath(player.transform.position);


            // piggy.transform.position = player.transform.position;
            // piggy.transform.Rotate(new Vector3(0.0f, 0.0f, 0.0f));
            // piggy.SendNetworkUpdateImmediate(true);

            //var boar = GameManager.server?.CreateEntity()


            Puts(piggy.transform.position.ToString());
            SendReply(player, piggy.transform.localPosition.ToString());
        }

        void OnServerInitialized()
        {
            foreach (BasePlayer player in BasePlayer.activePlayerList)
            {
                if (player.displayName == "Beorn")
                {
                    player.SendConsoleCommand($"createPiggyFromConsole");
                }
            }

        }

        void Init()
        {
            permission.RegisterPermission(permAdminsPiggyCreate, this);
        }


    }
}
