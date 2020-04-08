using Facepunch;
using Oxide.Core;
using Oxide.Core.Configuration;
using Oxide.Core.Plugins;
using Oxide.Game.Rust.Cui;
using Rust;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ProtoBuf;

namespace Oxide.Plugins
{
    [Info("Beorn", "KatushaLauncher", "1.0.0")]


    class KatushaLauncher : RustPlugin
    {
        private static float RocketSpeed = 75f;
        private static int RocketTimerAmountMin = 90;
        private static int RocketTimerAmountMax = 90;
        private static Vector3 LaunchAboveHeadDistanceVector = new Vector3(0, 10, 0);
        private static float RocketDamage = 70f;
        private static int AmountOfRockets = 100;
        private static float PeriodEachRocketLaunches = 0.2f;
        private static float SprayOfRockets = 4f;
        private static LayerMask GROUND_MASKS = LayerMask.GetMask("Terrain", "World", "Construction");
        private static Vector3 LauncherPosition = new Vector3(-190, 0, 1113);

        public Dictionary<BasePlayer, Vector3> Marker { get; set; }

        void OnServerInitialized()
        {
            Marker = new Dictionary<BasePlayer, Vector3>() { };
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

        [ChatCommand("markerkatusha")]
        private void CmdMarker(BasePlayer player, string command, string[] args)
        {
            if (Marker.ContainsKey(player))
            {
                Vector3 v = player.ServerPosition;
                SendReply(player, $"Вы отметили место на карте под координатами: x: {v.x}, y: {v.y}, z: {v.z}. Уверены что хотите продолжить. Напишите /startkatusha");
            }
            else
            {
                SendReply(player, "Пожалуйста, поставьте метку на карте или обновите её");
            }
        }

        [ChatCommand("testrocketlauncherst")]
        private void CmdStartkatusha(BasePlayer player, string command, string[] args)
        {
            if (!Marker.ContainsKey(player))
            {
                SendReply(player, "Пожалуйста, поставьте метку на карте или обновите её");
                return;
            }
            if (args.Length == 1)
            {
                SprayOfRockets = Convert.ToInt32(args[0]);
            }
            BaseEntity entity = null;
            var dist = Vector3.Distance(LauncherPosition, Marker[player]);
            SendReply(player, dist.ToString());
            timer.Repeat(PeriodEachRocketLaunches, AmountOfRockets, () =>
            {
                if (TOD_Sky.Instance.IsNight)
                    entity = GameManager.server.CreateEntity("assets/prefabs/npc/patrol helicopter/rocket_heli_airburst.prefab", LauncherPosition + LaunchAboveHeadDistanceVector, new Quaternion(0, 0, 0, 0), true);
                else
                    entity = GameManager.server.CreateEntity("assets/prefabs/ammo/rocket/rocket_smoke.prefab", LauncherPosition + LaunchAboveHeadDistanceVector, new Quaternion(), true);
                entity.GetComponent<TimedExplosive>().timerAmountMin = RocketTimerAmountMin;
                entity.GetComponent<TimedExplosive>().timerAmountMax = RocketTimerAmountMax;
                entity.GetComponent<ServerProjectile>().gravityModifier = (RocketSpeed * RocketSpeed * 1.41421356237f / 2f) / (dist * 3.468f); // g сила притяжения
                var direction = (Marker[player] - LauncherPosition).normalized + Vector3.up; // вектор (угол под которым мы бросаем)
                entity.GetComponent<ServerProjectile>().InitializeVelocity(direction * RocketSpeed + RandomRocketSprayVector()); // v - скорость
                for (int k = 0; k < entity.GetComponent<TimedExplosive>().damageTypes.Count; k++)
                {
                    entity.GetComponent<TimedExplosive>().damageTypes[k].amount *= RocketDamage;
                }
                entity.Spawn();
            });
        }

        [ConsoleCommand("testrocketlauncher")]
        private void CmdHorse(ConsoleSystem.Arg arg)
        {
            BasePlayer player = arg.Player();
            var target = FindBasePlayer(arg.GetString(0));
            if (target == null)
            {
                SendReply(player, "Игрок не найден");
                return;
            }
            StartRockets(player, target);
        }

        private void StartRockets(BasePlayer player, BasePlayer target)
        {
            BaseEntity entity = null;
            var a = player.GetNetworkPosition();
            timer.Repeat(PeriodEachRocketLaunches, AmountOfRockets, () =>
            {
                var dist = Vector3.Distance(a, target.transform.position); // 100 метров
                if (TOD_Sky.Instance.IsNight)
                    entity = GameManager.server.CreateEntity("assets/prefabs/npc/patrol helicopter/rocket_heli_airburst.prefab", a + LaunchAboveHeadDistanceVector, new Quaternion(0, 0, 0, 0), true);
                else
                    entity = GameManager.server.CreateEntity("assets/prefabs/ammo/rocket/rocket_smoke.prefab", a + LaunchAboveHeadDistanceVector, new Quaternion(), true);
                entity.GetComponent<TimedExplosive>().timerAmountMin = RocketTimerAmountMin;
                entity.GetComponent<TimedExplosive>().timerAmountMax = RocketTimerAmountMax;
                entity.GetComponent<ServerProjectile>().gravityModifier = (RocketSpeed * RocketSpeed * 1.41421356237f / 2f) / (dist * 3.468f); // g сила притяжения
                var direction = (target.GetNetworkPosition() - entity.GetNetworkPosition()).normalized + Vector3.up; // вектор (угол под которым мы бросаем)
                entity.GetComponent<ServerProjectile>().InitializeVelocity(direction * RocketSpeed + RandomRocketSprayVector()); // v - скорость
                for (int k = 0; k < entity.GetComponent<TimedExplosive>().damageTypes.Count; k++)
                {
                    entity.GetComponent<TimedExplosive>().damageTypes[k].amount *= RocketDamage;
                }
                entity.Spawn();
            });
        }

        private object OnMapMarkerAdd(BasePlayer player, MapNote note)
        {
            Puts("OnMapMarkerAdd works!");
            if (Marker.ContainsKey(player))
            {
                Marker[player] = note.worldPosition;
            } else
            {
                Marker.Add(player, note.worldPosition);
            }

            return null;
        }

        private Vector3 RandomRocketSprayVector()
        {
            System.Random rnd = new System.Random();
            var randX = SprayOfRockets * ((float)rnd.NextDouble() - 0.5f);
            var randY = SprayOfRockets * ((float)rnd.NextDouble() - 0.5f);
            var randZ = SprayOfRockets * ((float)rnd.NextDouble() - 0.5f);
            return new Vector3(randX, randY, randZ);
        }

    }
}