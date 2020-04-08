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

using Oxide.Core.SQLite.Libraries;
using Oxide.Core.Database;
using Newtonsoft.Json;


namespace Oxide.Plugins
{
    [Info("TopCustom", "TopCustom.ru", "0.0.1")]

    class TopCustom : RustPlugin
    {
        [PluginReference]
        private Plugin Rep;


        Core.SQLite.Libraries.SQLite Sqlite = Interface.GetMod().GetLibrary<Core.SQLite.Libraries.SQLite>();
        Connection Sqlite_conn;


        //ID Сообщения в чат
        private int MessageNum = 0; 

        // Список игроков
        public List<PlayerData> playersData = new List<PlayerData>();
        public List<PlayerData> playersDataTMP = null;

        private static  List<string> Ents = new List<string>() {
            "AutoTurret",
            "FlameTurret",
            "GunTrap",
            "Landmine",
            "BearTrap",
            "SamSite",
            "Bear",
            "Wolf",
            "Deer",
            "Boar",
            "Chicken",
            "Horse",
            "Zombie",
            "Scientist",
            "Murderer",
            "BaseHelicopter",
            "BradleyAPC"
        };


        [ChatCommand("showtop")]
        void showMyCustomTop(BasePlayer player, string cmd, string[] Args)
        {
            //SendReply(player, $"Kills: { MessageNum }");

            getTopByKey(player,  "PVP_KD", Args);
            // getTopByKey(player,  "Resources");
            // getTopByKey(player,  "Explosions");
            // getTopByKey(player,  "Reputation");
        }

        [ChatCommand("getshoot")]
        void getMyBestShoot(BasePlayer player, string cmd, string[] Args)
        {
            PlayerData con = (from x in playersData where x.UID == Convert.ToString(player.userID) select x).FirstOrDefault();
            SendReply(player, $"$Твой самый дальний выстрел: { con.shootDistance } метров.");
        }


        private void startChecking () {
            Saved();
            // Puts("saving players data...");
        }

        private void OnServerInitialized()
        {
            timer.Every(60, () => { startChecking(); });
        }





         // Считаем кол-во взорванных с4
        void addRadhouseToCustomTop(BasePlayer player)
        {
            PlayerData con = (from x in playersData where x.UID == Convert.ToString(player.userID) select x).FirstOrDefault();
            con.radHousesLooted += 1;
            // Saved();
        }

        private void getTopByKey (BasePlayer tmpPlayer, string sortKey, string[] Args) {

            int i = 5;

            if (Args.Length == 1) {
                i = Convert.ToInt32(Args[0]);
            }

            SendReply(tmpPlayer, $"<color=#FFEB3B>####### Top {i} #######</color>");

            switch (sortKey) {
                case "PVP_KD":
                    var tmpTop = (List<PlayerData>)getSortedPlayersTop("PVPKills");

                    // для теста...
                    foreach (var player in tmpTop) {
                        // SendReply(tmpPlayer, $"{ player.nickname } ({i}): {player.explosionMade}");

                        i--;

                        if (i <= 0)
                            break;
                    }

                    tmpTop = null;

                    break;
            }
        }



        private List<PlayerData> getSortedPlayersTop (string sortKey) {
            switch (sortKey) {
                case "PVPKills":
                    return (List<PlayerData>)playersData.OrderByDescending(x => x.PVPKills);

                case "PVPDeath":
                    return (List<PlayerData>)playersData.OrderByDescending(x => x.PVPDeath);

                case "Reputation":
                    return (List<PlayerData>)playersData.OrderByDescending(x => x.explosionMade);
            }

            return null;
        }
 
        private string getSinglePlayersTopPositionByKey(BasePlayer player, string sortKey) {
            string result = "";
            int length = 1;
            int playerTopPos = -1;

            switch (sortKey) {
                case "Gathering":
                    var tmpTop1 = playersData.OrderByDescending(x => x.ResoursesCollectedByHads);

                    foreach (var tmpPlayer in tmpTop1) {
                        if ((tmpPlayer.PVPDeath == 0) && (tmpPlayer.ResoursesCollectedByHads == 0)) continue;

                        if (tmpPlayer.UID == Convert.ToString(player.userID)) {
                            playerTopPos = length;
                        }
                        length++;
                    }

                    result = $"{ (playerTopPos == -1) ? "N/A" : playerTopPos.ToString() }    из    { length }";

                break;

                case "PVP_KD":
                    var tmpTop2 = playersData.OrderByDescending(x => ((float)x.PVPKills / ((x.PVPDeath != 0) ? x.PVPDeath : 1 )));

                    foreach (var tmpPlayer in tmpTop2) {
                        if ((tmpPlayer.PVPDeath == 0) && (tmpPlayer.PVPKills == 0)) continue;

                        if (tmpPlayer.UID == Convert.ToString(player.userID)) {
                            playerTopPos = length;
                        }
                        length++;
                    }

                    result = $"{ (playerTopPos == -1) ? "N/A" : playerTopPos.ToString() }    из    { length }";

                break;

                case "Explosion":
                    var tmpTop3 = playersData.OrderByDescending(x => x.explosionMade);

                    foreach (var tmpPlayer in tmpTop3) {
                        if (tmpPlayer.explosionMade == 0) continue;

                        if (tmpPlayer.UID == Convert.ToString(player.userID)) {
                            playerTopPos = length;
                        }
                        length++;
                    }

                    result = $"{ (playerTopPos == -1) ? "N/A" : playerTopPos.ToString() }    из    { length }";

                break;

                case "radHouse":
                var tmpTop4 = playersData.OrderByDescending(x => x.radHousesLooted);

                foreach (var tmpPlayer in tmpTop4) {
                    if (tmpPlayer.radHousesLooted == 0) continue;

                    if (tmpPlayer.UID == Convert.ToString(player.userID)) {
                        playerTopPos = length;
                    }
                    length++;
                }

                result = $"{ (playerTopPos == -1) ? "N/A" : playerTopPos.ToString() }    из    { length }";

                break;
            }

            return result;
        }

        private void OnPlayerDie(BasePlayer victim, HitInfo hitInfo) {
            if (victim == null || victim.IsNpc)
                return;

            if (hitInfo == null)
                return;

            if (IsTrap(hitInfo.Initiator) || (IsNpc(hitInfo.Initiator)))
                return;

            if (IsRadiation(hitInfo))
                return;

            var attacker = hitInfo.InitiatorPlayer;

            if (attacker == null || attacker.IsNpc)
                return;

            if (attacker == victim)
                return;

            if (IsExplosion(hitInfo))
                return;

            if (IsFlame(hitInfo))
                return;

            var distance = !hitInfo.IsProjectile() ? (int)Vector3.Distance(hitInfo.PointStart, hitInfo.HitPositionWorld) : (int)hitInfo.ProjectileDistance;
                OnKilled(attacker, victim, hitInfo, distance);

        }

        private void OnKilled(BasePlayer attacker, BasePlayer victim, HitInfo hitInfo, int dist)
        {
            Interface.Oxide.LogDebug($"{ attacker.displayName } убил { victim.displayName } Дистанция: { dist }");

            PlayerData con = (from x in playersData where x.UID == Convert.ToString(attacker.userID) select x).FirstOrDefault();

            if (con.shootDistance < dist)
                con.shootDistance = dist;         
        }

        private static bool IsExplosion(HitInfo hit) => (hit.WeaponPrefab != null && (hit.WeaponPrefab.ShortPrefabName.Contains("grenade") || hit.WeaponPrefab.ShortPrefabName.Contains("explosive")))
                                                        || hit.damageTypes.GetMajorityDamageType() == DamageType.Explosion || (!hit.damageTypes.IsBleedCausing() && hit.damageTypes.Has(DamageType.Explosion));
        private static bool IsFlame(HitInfo hit) => hit.damageTypes.GetMajorityDamageType() == DamageType.Heat || (!hit.damageTypes.IsBleedCausing() && hit.damageTypes.Has(DamageType.Heat));
        private static bool IsRadiation(HitInfo hit) => hit.damageTypes.GetMajorityDamageType() == DamageType.Radiation || (!hit.damageTypes.IsBleedCausing() && hit.damageTypes.Has(DamageType.Radiation));
        private static bool IsTrap(BaseEntity entity) => Ents.Contains(entity?.GetType().ToString());
        private static bool IsNpc(BaseEntity npc) => Ents.Contains(npc?.GetType().ToString());





        private Dictionary<uint, string> LastHeliHit = new Dictionary<uint, string>();
        void OnEntityTakeDamage(BaseCombatEntity entity, HitInfo info)
        {
            if (entity is BaseHelicopter && info.Initiator is BasePlayer)
                LastHeliHit[entity.net.ID] = info.InitiatorPlayer.UserIDString;
        }
        void OnEntityDeath(BaseCombatEntity victim, HitInfo info)
        {
            if (victim == null || info == null) return;
            BasePlayer victimBP = victim.ToPlayer();
            BasePlayer initiator = info.InitiatorPlayer;
            if (victimBP != null && !IsNPC(victimBP))
            {
                string death = victimBP.UserIDString;
                PlayerData con = (from x in playersData where x.UID == death select x).FirstOrDefault();
                con.PVPDeath += 1;
                // Saved();
            }
            if (initiator == null)
            {
                if (victim is BaseHelicopter)
                {
                    if (LastHeliHit.ContainsKey(victim.net.ID))
                    {
                        PlayerData data = playersData.Where(p => p.UID == LastHeliHit[victim.net.ID]).FirstOrDefault();
                        data.helicopterDestroyed += 1;
                        LastHeliHit.Remove(victim.net.ID);
                    }
                }
                return;
            }
            if (initiator != null && !IsNPC(initiator))
            {
                string killer = initiator.UserIDString;
                PlayerData con2 = (from x in playersData where x.UID == killer select x).FirstOrDefault();
                if (IsNPC(victimBP))
                {
                    con2.NPCKilled++;
                    // Saved();
                    return;
                }
                if (victim is BradleyAPC)
                {
                    con2.tanksDestroyed++;
                    // Saved();
                    return;
                }
                if (victimBP != null && victimBP != initiator)
                {
                    con2.PVPKills += 1;
                    // Saved();
                    return;
                }
            }
            return;
        }

        // Надо понять че и как 
        void PutRepInTop(string UserIDString, int rep)
        {
            //Puts("Saved");
            PlayerData con = (from x in playersData where x.UID == UserIDString select x).FirstOrDefault();
            if (con == null) { Puts($"Игрок {UserIDString} не найден в PlayerData.json при перемещении репутации"); return; }
            con.Reputation = rep;
            // Saved
        }


        // Считаем кол-во взорванных с4
        void OnExplosiveThrown(BasePlayer player, BaseEntity entity)
        {
            PlayerData con = (from x in playersData where x.UID == Convert.ToString(player.userID) select x).FirstOrDefault();
            con.explosionMade += 1;
            // Saved();
        }


        // Считаем кол-во выстрелов ракетами
        void OnRocketLaunched(BasePlayer player, BaseEntity entity)
        {
            PlayerData con = (from x in playersData where x.UID == Convert.ToString(player.userID) select x).FirstOrDefault();
            con.explosionMade += 1;
            // Saved();
        }
        

        // Считаем кол-во выстрелов с оружия
        void OnWeaponFired(BaseProjectile projectile, BasePlayer player, ItemModProjectile mod, ProtoBuf.ProjectileShoot projectiles)
        {
            PlayerData con = (from x in playersData where x.UID == Convert.ToString(player.userID) select x).FirstOrDefault();
            //if (projectile.primaryMagazine.ammoType.itemid == -420273765 || projectile.primaryMagazine.ammoType.itemid == -1280058093)
            if (projectile.primaryMagazine.definition.ammoTypes == Rust.AmmoTypes.BOW_ARROW)
            {   //стрелы
                con.shootsMade += 1;
            }
            else
            {   // пули
                con.shootsMade += 1;
            }
            // Saved();
        }


        void OnQuarryGather(MiningQuarry quarry, Item item)
        {
            Item fuel = quarry.fuelStoragePrefab.instance.GetComponent<StorageContainer>().inventory.FindItemsByItemName("lowgradefuel");

            if(quarry.OwnerID == 0) {
                return;             
            }

            BasePlayer player = BasePlayer.FindByID(quarry.OwnerID);
            if(player == null)
            {
                BasePlayer player1 = BasePlayer.FindSleeping(quarry.OwnerID);
                if(player1 == null) return;

                PlayerData con1 = (from x in playersData where x.UID == Convert.ToString(player1.userID) select x).FirstOrDefault();
                con1.ResoursesCollectedByCarrier += item.amount;
                // Saved();
            }
            else {   
                PlayerData con = (from x in playersData where x.UID == Convert.ToString(player.userID) select x).FirstOrDefault();
                con.ResoursesCollectedByCarrier += item.amount;
                // Saved();
            }
        }

        //Добываем долбежкой ресы
        void OnDispenserGather(ResourceDispenser dispenser, BaseEntity entity, Item item)
        {
            if (entity == null || !(entity is BasePlayer) || item == null || dispenser == null) return;
            // if (entity.ToPlayer() is BasePlayer){
            //     //Puts($"Amount: {item.amount}");

            //     DoGather(entity.ToPlayer(), item);
            // }

            BasePlayer player = entity?.ToPlayer();
            if (player != null) {
                DoGather(player, item);                
            }
        }

        // поднимаем ресурсы
        void OnCollectiblePickup(Item item, BasePlayer player)
        {
            DoGather(player, item);
        }

        // тут будем считать все ресурсы // необходимо сделать распределение на типы добываемых также
        void DoGather(BasePlayer player, Item item)
        {
            //SendReply(player, $"itemid: {item.info.itemid}");

            if (player == null) return;
            PlayerData con = (from x in playersData where x.UID == Convert.ToString(player.userID) select x).FirstOrDefault();
            con.ResoursesCollectedByHads += item.amount;

            switch (item.info.itemid) {

                // дерево - подобрано
                case -151838493:
                    con.woodCollected += item.amount;
                    break;

                // Камень - подобрано
                case -2099697608:
                    con.stoneCollected += item.amount;
                    break;

                // металл - подобрано
                case -4031221:
                    con.metallCollected += item.amount;
                    break;

                // сера - подобрано
                case -1157596551:
                    con.sulfureCollected += item.amount;
                    break;
            }

            // Saved();
            return;
        }

        // При заходе игрока проверяем, есть ли он в дате. Если нет - создаем. Если есть, обновляем никнейм (на случай, если он его сменил)
        void OnPlayerInit(BasePlayer player)
        {
            var check = (from x in playersData where x.UID == player.UserIDString select x).Count();
            if (check == 0) CreateInfo(player);

            //Обновляем игровой ник
            PlayerData con = (from x in playersData where x.UID == Convert.ToString(player.userID) select x).FirstOrDefault();
            con.nickname = (string)player.displayName;
            // Saved();
        }
        
        // Загружаем если есть дату по игрокам. Создаем нового игрока в дате, если его не существует.
        void Loaded()
        {
            playersData = Interface.Oxide.DataFileSystem.ReadObject<List<PlayerData>>("PlayerData");
            foreach (var player in BasePlayer.activePlayerList)
            {
                var check = (from x in playersData where x.UID == player.UserIDString select x).Count();
                if (check == 0) CreateInfo(player);
            }
        }

        // Сохраняем дату по игрокам
        void Saved()
        {
            Interface.Oxide.DataFileSystem.WriteObject("PlayerData", playersData);
        }

        // Создаем нового игрока в дате, если его не существует
        void CreateInfo(BasePlayer player)
        {
            if (player == null) return;
            playersData.Add(new PlayerData(player.displayName, player.UserIDString));
            // Saved();
        }

        // Проверка на NPC
        private bool IsNPC(BasePlayer player)
        {
            if (player == null) return false;
            //BotSpawn
            if (player is NPCPlayer)
                return true;
            //HumanNPC
            if (!(player.userID >= 76560000000000000L || player.userID <= 0L))
                return true;
            return false;
        }

        private Dictionary<string, string> ReturnPlayersStatistics(BasePlayer player)
        {
            PlayerData dat = (from x in playersData where x.UID == Convert.ToString(player.userID) select x).FirstOrDefault();

            var statistics = new Dictionary<string, string>() {
                { "Kills",              dat.PVPKills.ToString() },
                { "Deaths",             dat.PVPDeath.ToString() },
                { "GatheredByHands",    dat.ResoursesCollectedByHads.ToString() },
                { "GatheredByCareer",   dat.ResoursesCollectedByCarrier.ToString() },
                { "RepPlus",            (string)Rep.Call("GetRepByPlayerPos", player)},
                { "RepMinus",           (string)Rep.Call("GetRepByPlayerNeg", player)},
                { "radHouseSingle",     dat.radHousesLooted.ToString() },
                { "shootDistance",      dat.shootDistance.ToString() },
            };

            return statistics;

        }
        private Dictionary<string, string> ReturnPlayersStatisticsForTop(BasePlayer player)
        {
            Puts("test");

            PlayerData dat = (from x in playersData where x.UID == Convert.ToString(player.userID) select x).FirstOrDefault();

            var statistics = new Dictionary<string, string>() {
                { "Kills",              dat.PVPKills.ToString() },
                { "Deaths",             dat.PVPDeath.ToString() },
                { "Rep",        (string)Rep.Call("GetRepTop", player)},
            };

            Puts("test2");

            return statistics;

        }
        private Dictionary<string, string> ReturnPlayersGathering(BasePlayer player)
        {
            PlayerData dat = (from x in playersData where x.UID == Convert.ToString(player.userID) select x).FirstOrDefault();

            var gathering = new Dictionary<string, string>() {
                { "Sulfure",    dat.sulfureCollected.ToString() },
                { "MetalOre",   dat.metallCollected.ToString() },
                { "Stone",      dat.stoneCollected.ToString() },
                { "Tree",       dat.woodCollected.ToString() }
            };

            return gathering;

        }
        private Dictionary<string, string> ReturnPlayersOther(BasePlayer player)
        {
            PlayerData dat = (from x in playersData where x.UID == Convert.ToString(player.userID) select x).FirstOrDefault();

            var other = new Dictionary<string, string>() {
                    { "Shots",              dat.shootsMade.ToString() },
                    { "Explosions",         dat.explosionMade.ToString() },
                    { "HeliCrashed",        dat.helicopterDestroyed.ToString() },
                    { "PanzerDestroyed",    dat.tanksDestroyed.ToString() },
                    { "NPCKilled",          dat.NPCKilled.ToString() }
                };

            return other;

        }
        private Dictionary<string, string> ReturnPlayersTop(BasePlayer player)
        {
            PlayerData dat = (from x in playersData where x.UID == Convert.ToString(player.userID) select x).FirstOrDefault();

            var top = new Dictionary<string, string>() {
                { "Gathering",  getSinglePlayersTopPositionByKey(player, "Gathering") },
                { "KD",         getSinglePlayersTopPositionByKey(player, "PVP_KD")},
                { "Explosion",  getSinglePlayersTopPositionByKey(player, "Explosion")},
                { "Rep",        (string)Rep.Call("GetRepTop", player)},
                { "radHouse",   getSinglePlayersTopPositionByKey(player, "radHouse")}
            };

            return top;

        }
        // Класс даты игрока
        public class PlayerData {
            public PlayerData (string nickname, string UID) {
                this.nickname = nickname;
                this.UID = UID;

                //Первая панель (не вижу смысла пихать в три разных класса "типа по панелям")
                this.PVPKills                    = 0;
                this.PVPDeath                    = 0;
                this.ResoursesCollectedByHads    = 0;
                this.ResoursesCollectedByCarrier = 0;
                this.Reputation                  = 0;

                // Добыча ресурсов
                this.sulfureCollected   = 0;
                this.metallCollected    = 0;
                this.stoneCollected     = 0;
                this.woodCollected      = 0;

                // Остальное
                this.shootsMade       = 0;
                this.explosionMade    = 0;
                this.helicopterDestroyed = 0;
                this.tanksDestroyed      = 0;
                this.NPCKilled        = 0;

                this.shootDistance = 0;

                // Время на сервере
                this.timePlayer = 0;
                this.radHousesLooted = 0;
            }

            public string nickname                  { get; set; }
            public string UID                       { get; set; }
            public int PVPKills                     { get; set; }
            public int PVPDeath                     { get; set; }
            public int ResoursesCollectedByHads     { get; set; }
            public int ResoursesCollectedByCarrier  { get; set; }
            public int Reputation                   { get; set; }
            public int sulfureCollected             { get; set; }
            public int metallCollected              { get; set; }
            public int stoneCollected               { get; set; }
            public int woodCollected                { get; set; }
            public int shootsMade                   { get; set; }
            public int explosionMade                { get; set; }
            public int helicopterDestroyed          { get; set; }
            public int tanksDestroyed               { get; set; }
            public int NPCKilled                    { get; set; }
            public int shootDistance                { get; set; }
            public int radHousesLooted              { get; set; }

            public int timePlayer                   { get; set; }
        }
    }
}
                                