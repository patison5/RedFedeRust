using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;

using Rust;

using Oxide.Core;
using Oxide.Game.Rust;
using ProtoBuf;

using UnityEngine;

namespace Oxide.Plugins
{
    [Info("Friends", "Ernieleo / vk.com/ernieleo", "2.1.51")]
	//	friendly fire
	//	friend request
    class Friends : RustPlugin
    {
        private ConfigData configData;
        private Dictionary<ulong, PlayerData> FriendsData;
        private readonly Dictionary<ulong, HashSet<ulong>> ReverseData = new Dictionary<ulong, HashSet<ulong>>();
        private readonly Dictionary<ulong, Timer> PendingRequests = new Dictionary<ulong, Timer>();
        private readonly Dictionary<ulong, BasePlayer> PlayersRequests = new Dictionary<ulong, BasePlayer>();

        class ConfigData
        {
            public int MaxFriends { get; set; }
        }

        class PlayerData
        {
            public string Name { get; set; } = string.Empty;
            public HashSet<ulong> Friends { get; set; } = new HashSet<ulong>();
        }

        protected override void LoadDefaultConfig()
        {
            var config = new ConfigData
            {
                MaxFriends = 5,
            };
            Config.WriteObject(config, true);
        }

        private void Init()
        {
            lang.RegisterMessages(new Dictionary<string, string>
            {
                {"List", "<color=#0EFF6A>Друзья:</color><size=7>\n\n</size>  {1}"},
                {"NoFriends", "<color=#0EFF6A>Друзья</color><size=7>\n\n</size>  У вас нет друзей!"},
                {"NotOnFriendlist", "<color=#0EFF6A>Друзья</color><size=7>\n\n</size>  <color=#0EFF6A>{0}</color> не ваш друг!"},
                {"FriendRemoved", "<color=#0EFF6A>Друзья</color><size=7>\n\n</size>  <color=#0EFF6A>{0}</color> больше не ваш друг!"},
                {"PlayerNotFound", "<color=#0EFF6A>Друзья</color><size=7>\n\n</size>  Игрок <color=#0EFF6A>{0}</color> не найден!"},
                {"CantAddSelf", "<color=#0EFF6A>Друзья</color><size=7>\n\n</size>  Нельзя добавить себя в друзья!"},
                {"AlreadyOnList", "<color=#0EFF6A>Друзья</color><size=7>\n\n</size>  <color=#0EFF6A>{0}</color> уже ваш друг!"},
                {"FriendlistFull", "<color=#0EFF6A>Друзья</color><size=7>\n\n</size>  Вы достигли лимита друзей!"},
                {"Syntax", "<color=#0EFF6A>Друзья</color><size=7>\n\n</size>  Используйте команду <color=#0EFF6A>/friend add ИмяИгрока</color> или <color=#0EFF6A>/addfriend ИмяИгрока</color> чтобы отправить запрос на дружбу<size=7>\n\n</size>  Для того чтобы удалить из друзей используйте команду <color=#0EFF6A>/friend remove ИмяИгрока</color> или <color=#0EFF6A>/removefriend ИмяИгрока</color>"},
				{"AcceptFriend", "<color=#0EFF6A>Друзья</color><size=7>\n\n</size>  <color=#0EFF6A>{0}</color> теперь ваш друг!"},
				{"MultiplePlayers", "<color=#0EFF6A>Друзья</color><size=7>\n\n</size>  Найдено несколько игроков с похожим именем: {0}"},
				{"Request", "<color=#0EFF6A>Друзья</color><size=7>\n\n</size>  Игроку <color=#0EFF6A>{0}</color> был отправлен ваш запрос на дружбу!"},
				{"RequestTarget", "<color=#0EFF6A>Друзья</color><size=7>\n\n</size>  Игрок <color=#0EFF6A>{0}</color> отправил вам запрос на дружбу! Используйте команду <color=#0EFF6A>/friend accept</color> чтобы принять!"},
				{"TimedOut", "<color=#0EFF6A>Друзья</color><size=7>\n\n</size>  Игрок <color=#0EFF6A>{0}</color> не ответил на ваш запрос!"},
				{"TimedOutTarget", "<color=#0EFF6A>Друзья</color><size=7>\n\n</size>  Вы не ответили на запрос игрока <color=#0EFF6A>{0}</color>. Запрос отменен!"},
				{"RequestTargetOff", "<color=#0EFF6A>Друзья</color><size=7>\n\n</size>  Игрок <color=#0EFF6A>{0}</color> отключился. Запрос отменен!"},
				{"NoPendingRequest", "<color=#0EFF6A>Друзья</color><size=7>\n\n</size>  Нет активных запросов!"},
				{"PendingRequest", "<color=#0EFF6A>Друзья</color><size=7>\n\n</size>  У вас уже имеется активный запрос на дружбу!"},
				{"PendingRequestTarget", "<color=#0EFF6A>Друзья</color><size=7>\n\n</size>  Игрок <color=#0EFF6A>{0}</color> уже имеет активный запрос на дружбу!"}
            }, this);
            configData = Config.ReadObject<ConfigData>();
            try
            {
                FriendsData = Interface.Oxide.DataFileSystem.ReadObject<Dictionary<ulong, PlayerData>>(nameof(Friends));
            }
            catch
            {
                FriendsData = new Dictionary<ulong, PlayerData>();
            }
			
			//	Проверяем недостающих друзей у игроков
            foreach (var data in FriendsData)
                foreach (var friend in data.Value.Friends)
                    AddFriend(friend, data.Key);
			
			//	Сохраняем
			saveDataImmediate();
			
            foreach (var data in FriendsData)
                foreach (var friend in data.Value.Friends)
                    AddFriendReverse(data.Key, friend);
        }

        private object OnTurretTarget(AutoTurret turret, BaseCombatEntity targ)
        {
            if (!(targ is BasePlayer) || turret.OwnerID <= 0) return null;
            var player = (BasePlayer) targ;
            if (!HasFriend(turret.OwnerID, player.userID)) return null;
			
            return false;
        }

        private object CanUseLockedEntity(BasePlayer player, BaseLock @lock)
        {
            if (!(@lock is CodeLock) || @lock.GetParentEntity().OwnerID <= 0) return null;
            if (HasFriend(@lock.GetParentEntity().OwnerID, player.userID)) return true;
			
            return null;
        }
		
		
        Timer saveDataBatchedTimer = null;

        // Collects all save calls within delay and saves once there are no more updates.
        void saveData(float delay = 3f)
        {
            if (saveDataBatchedTimer == null)
                saveDataBatchedTimer = timer.Once(delay, saveDataImmediate);
            else
                saveDataBatchedTimer.Reset(delay);
        }

        void saveDataImmediate()
        {
            if (saveDataBatchedTimer != null)
            {
                saveDataBatchedTimer.DestroyToPool();
                saveDataBatchedTimer = null;
            }
            Interface.Oxide.DataFileSystem.WriteObject("Friends", FriendsData);
        }
		
        void Unload()
        {
			saveDataImmediate();
        }
		
        void OnServerSave()
        {
			saveDataImmediate();
        }
		
		void OnServerShutdown()
        {
			saveDataImmediate();
        }

        private bool AddFriendS(string playerS, string friendS)
        {
            if (string.IsNullOrEmpty(playerS) || string.IsNullOrEmpty(friendS)) return false;
            var playerId = Convert.ToUInt64(playerS);
            var friendId = Convert.ToUInt64(friendS);
            return AddFriend(playerId, friendId);
        }

        private bool AddFriend(ulong playerId, ulong friendId)
        {
            var playerData = GetPlayerData(playerId);
            if (playerData.Friends.Count >= configData.MaxFriends || !playerData.Friends.Add(friendId)) return false;
            AddFriendReverse(playerId, friendId);
            Interface.Oxide.CallHook("OnFriendAdded", playerId, friendId);
            return true;
        }

        private bool RemoveFriendS(string playerS, string friendS)
        {
            if (string.IsNullOrEmpty(playerS) || string.IsNullOrEmpty(friendS)) return false;
            var playerId = Convert.ToUInt64(playerS);
            var friendId = Convert.ToUInt64(friendS);
            return RemoveFriend(playerId, friendId);
        }

        private bool RemoveFriend(ulong playerId, ulong friendId)
        {
            var playerData = GetPlayerData(playerId);
            if (!playerData.Friends.Remove(friendId)) return false;
            HashSet<ulong> friends;
            if (ReverseData.TryGetValue(friendId, out friends))
                friends.Remove(playerId);
            Interface.Oxide.CallHook("OnFriendRemoved", playerId, friendId);
            return true;
        }

        private bool HasFriendS(string playerS, string friendS)
        {
            if (string.IsNullOrEmpty(playerS) || string.IsNullOrEmpty(friendS)) return false;
            var playerId = Convert.ToUInt64(playerS);
            var friendId = Convert.ToUInt64(friendS);
            return HasFriend(playerId, friendId);
        }

        private bool HasFriend(ulong playerId, ulong friendId)
        {
            return GetPlayerData(playerId).Friends.Contains(friendId);
        }

        private bool HadFriendS(string playerS, string friendS)
        {
            if (string.IsNullOrEmpty(playerS) || string.IsNullOrEmpty(friendS)) return false;
            var playerId = Convert.ToUInt64(playerS);
            var friendId = Convert.ToUInt64(friendS);
            return HadFriend(playerId, friendId);
        }

        private bool HadFriend(ulong playerId, ulong friendId)
        {
            var playerData = GetPlayerData(playerId);
            return playerData.Friends.Contains(friendId);
        }

        private bool AreFriendsS(string playerS, string friendS)
        {
            if (string.IsNullOrEmpty(playerS) || string.IsNullOrEmpty(friendS)) return false;
            var playerId = Convert.ToUInt64(playerS);
            var friendId = Convert.ToUInt64(friendS);
            return AreFriends(playerId, friendId);
        }

        private bool AreFriends(ulong playerId, ulong friendId)
        {
            return GetPlayerData(playerId).Friends.Contains(friendId) && GetPlayerData(friendId).Friends.Contains(playerId);
        }

        private bool IsFriendS(string playerS, string friendS)
        {
            if (string.IsNullOrEmpty(playerS) || string.IsNullOrEmpty(friendS)) return false;
            var playerId = Convert.ToUInt64(playerS);
            var friendId = Convert.ToUInt64(friendS);
            return IsFriend(playerId, friendId);
        }

        private bool IsFriend(ulong playerId, ulong friendId)
        {
            return GetPlayerData(friendId).Friends.Contains(playerId);
        }

        private string[] GetFriendsS(string playerS)
        {
            var playerId = Convert.ToUInt64(playerS);
            return GetPlayerData(playerId).Friends.ToList().ConvertAll(f => f.ToString()).ToArray();
        }

        private ulong[] GetFriends(ulong playerId)
        {
            return GetPlayerData(playerId).Friends.ToArray();
        }

        private string[] GetFriendListS(string playerS)
        {
            return GetFriendList(Convert.ToUInt64(playerS));
        }

        private string[] GetFriendList(ulong playerId)
        {
            var playerData = GetPlayerData(playerId);
            var players = new List<string>();
            foreach (var friend in playerData.Friends)
                players.Add(GetPlayerData(friend).Name);
            return players.ToArray();
        }

        private string[] IsFriendOfS(string playerS)
        {
            var playerId = Convert.ToUInt64(playerS);
            var friends = IsFriendOf(playerId);
            return friends.ToList().ConvertAll(f => f.ToString()).ToArray();
        }

        private ulong[] IsFriendOf(ulong playerId)
        {
            HashSet<ulong> friends;
            return ReverseData.TryGetValue(playerId, out friends) ? friends.ToArray() : new ulong[0];
        }

        private PlayerData GetPlayerData(ulong playerId)
        {
            var player = RustCore.FindPlayerById(playerId);
            PlayerData playerData;
            if (!FriendsData.TryGetValue(playerId, out playerData))
                FriendsData[playerId] = playerData = new PlayerData();
            if (player != null) playerData.Name = player.displayName;
            return playerData;
        }

        [ChatCommand("friend")]
        private void cmdFriend(BasePlayer player, string command, string[] args)
        {
            if (args == null || args.Length <= 0)
            {
                PrintMessage(player, "Syntax");
                return;
            }
            switch (args[0].ToLower())
            {
				case "list":
                    var friendList = GetFriendList(player.userID);
                    if (friendList.Length > 0)
                        PrintMessage(player, "List", $"{friendList.Length}/{configData.MaxFriends}", string.Join(", ", friendList));
                    else
                        PrintMessage(player, "NoFriends");
                    break;
                case "add":
                case "+":
					try
					{
						var targets = FindPlayersOnline(args[1]);
						if (targets.Count <= 0)
						{
							PrintMessage(player, "PlayerNotFound", args[1]);
							return;
						}
						if (targets.Count > 1)
						{
							PrintMessage(player, "MultiplePlayers", string.Join(", ", targets.ConvertAll(p => p.displayName).ToArray()));
							return;
						}
						var friendPlayer = targets[0];
						if (player == friendPlayer)
						{
							PrintMessage(player, "CantAddSelf");
							return;
						}
						var playerData = GetPlayerData(player.userID);
						if (playerData.Friends.Count >= configData.MaxFriends)
						{
							PrintMessage(player, "FriendlistFull");
							return;
						}
						if (playerData.Friends.Contains(friendPlayer.userID))
						{
							PrintMessage(player, "AlreadyOnList", friendPlayer.displayName);
							return;
						}
						if (PlayersRequests.ContainsKey(player.userID))
						{
							PrintMessage(player, "PendingRequest");
							return;
						}
						if (PlayersRequests.ContainsKey(friendPlayer.userID))
						{
							PrintMessage(player, "PendingRequestTarget", friendPlayer.displayName);
							return;
						}
						PlayersRequests[player.userID] = friendPlayer;
						PlayersRequests[friendPlayer.userID] = player;
						PendingRequests[friendPlayer.userID] = timer.Once(20, () => {
						RequestTimedOut(player, friendPlayer);
						});
						PrintMessage(player, "Request", friendPlayer.displayName);
						PrintMessage(friendPlayer, "RequestTarget", player.displayName);
					}
					catch
					{
					}
                    break;
                case "remove":
                case "-":
					try
					{
						var friend = FindFriend(args[1]);
						if (friend <= 0)
						{
							PrintMessage(player, "NotOnFriendlist", args[1]);
							return;
						}
						var name = GetFriendName(friend);
						var removed = RemoveFriend(player.userID, friend);
						PrintMessage(player, removed ? "FriendRemoved" : "NotOnFriendlist", removed ? name : args[1]);
						if (removed) RemoveFriend(friend, player.userID);
						//saveData();
					}
					catch
					{
					}
					break;
				case "accept":
					Timer reqTimer;
					if (!PendingRequests.TryGetValue(player.userID, out reqTimer))
					{
						PrintMessage(player, "NoPendingRequest");
						return;
					}
                    var playerDataReq = GetPlayerData(player.userID);
                    if (playerDataReq.Friends.Count >= configData.MaxFriends)
                    {
                        PrintMessage(player, "FriendlistFull");
                        return;
                    }
					var friendPlayerReq = PlayersRequests[player.userID];
                    AddFriend(player.userID, friendPlayerReq.userID);
					AddFriend(friendPlayerReq.userID, player.userID);
					PrintMessage(player, "AcceptFriend", friendPlayerReq.displayName);
					PrintMessage(friendPlayerReq, "AcceptFriend", player.displayName);
					reqTimer.Destroy();
					PendingRequests.Remove(player.userID);
					PlayersRequests.Remove(player.userID);
					PlayersRequests.Remove(friendPlayerReq.userID);
					//saveData();
                    break;
            }
        }
		
        [ChatCommand("friends")]
        private void cmdFriends(BasePlayer player, string command, string[] args)
        {
            var friendList = GetFriendList(player.userID);
            if (friendList.Length > 0)
                PrintMessage(player, "List", $"{friendList.Length}/{configData.MaxFriends}", string.Join(", ", friendList));
            else
                PrintMessage(player, "NoFriends");
            return;
		}
		
        [ChatCommand("addfriend")]
        private void cmdAddFriend(BasePlayer player, string command, string[] args)
		{
			if (args == null || args.Length <= 0)
                return;
			
			var targets = FindPlayersOnline(args[0]);
			if (targets.Count <= 0)
			{
				PrintMessage(player, "PlayerNotFound", args[0]);
				return;
			}
			if (targets.Count > 1)
			{
				PrintMessage(player, "MultiplePlayers", string.Join(", ", targets.ConvertAll(p => p.displayName).ToArray()));
				return;
			}
			var friendPlayer = targets[0];
            if (player == friendPlayer)
            {
                PrintMessage(player, "CantAddSelf");
                return;
            }
			var playerData = GetPlayerData(player.userID);
			if (playerData.Friends.Count >= configData.MaxFriends)
			{
                PrintMessage(player, "FriendlistFull");
                return;
			}
            if (playerData.Friends.Contains(friendPlayer.userID))
			{
                PrintMessage(player, "AlreadyOnList", friendPlayer.displayName);
                return;
			}
			if (PlayersRequests.ContainsKey(player.userID))
			{
				PrintMessage(player, "PendingRequest");
				return;
			}
			if (PlayersRequests.ContainsKey(friendPlayer.userID))
			{
				PrintMessage(player, "PendingRequestTarget");
				return;
			}
			PlayersRequests[player.userID] = friendPlayer;
			PlayersRequests[friendPlayer.userID] = player;
			PendingRequests[friendPlayer.userID] = timer.Once(20, () => {
				RequestTimedOut(player, friendPlayer);
			});
			PrintMessage(player, "Request", friendPlayer.displayName);
			PrintMessage(friendPlayer, "RequestTarget", player.displayName);
		}
		
        [ChatCommand("removefriend")]
        private void cmdremoveFriend(BasePlayer player, string command, string[] args)
		{
			try
			{
				var friend = FindFriend(args[0]);
				if (friend <= 0)
				{
					PrintMessage(player, "NotOnFriendlist", args[0]);
					return;
				}
				var name = GetFriendName(friend);
				var removed = RemoveFriend(player.userID, friend);
				PrintMessage(player, removed ? "FriendRemoved" : "NotOnFriendlist", removed ? name : args[0]);
				if (removed) RemoveFriend(friend, player.userID);
				//saveData();
			}
			catch
			{
			}
		}
		
        private void RequestTimedOut(BasePlayer player, BasePlayer friendPlayer)
        {
            PlayersRequests.Remove(player.userID);
            PlayersRequests.Remove(friendPlayer.userID);
            PendingRequests.Remove(friendPlayer.userID);
			PrintMessage(player, "TimedOut", friendPlayer.displayName);
			PrintMessage(friendPlayer, "TimedOutTarget", player.displayName);
        }
		
        private void AddFriendReverse(ulong playerId, ulong friendId)
        {
            HashSet<ulong> friends;
            if (!ReverseData.TryGetValue(friendId, out friends))
                ReverseData[friendId] = friends = new HashSet<ulong>();
            friends.Add(playerId);
        }

        private void PrintMessage(BasePlayer player, string msgId, params object[] args)
        {
            PrintToChat(player, lang.GetMessage(msgId, this, player.UserIDString), args);
        }

        private ulong FindFriend(string friend)
        {
            if (string.IsNullOrEmpty(friend)) return 0;
            foreach (var playerData in FriendsData)
            {
                if (playerData.Key.ToString().Equals(friend) || playerData.Value.Name.IndexOf(friend, StringComparison.OrdinalIgnoreCase) >= 0)
                    return playerData.Key;
            }
            return 0;
        }
		
        //  Для поиска имени друга
        private string GetFriendName(ulong friend)
        {
            var playerData = GetPlayerData(friend);
			var name = GetPlayerData(friend).Name;
            return name;
        }
		//  Ищем игроков онлайн
        private static List<BasePlayer> FindPlayersOnline(string nameOrIdOrIp)
        {
            var players = new List<BasePlayer>();
            if (string.IsNullOrEmpty(nameOrIdOrIp)) return players;
            foreach (var activePlayer in BasePlayer.activePlayerList)
            {
                if (activePlayer.UserIDString.Equals(nameOrIdOrIp))
                    players.Add(activePlayer);
                else if (!string.IsNullOrEmpty(activePlayer.displayName) && activePlayer.displayName.Contains(nameOrIdOrIp, CompareOptions.IgnoreCase))
                    players.Add(activePlayer);
                else if (activePlayer.net?.connection != null && activePlayer.net.connection.ipaddress.Equals(nameOrIdOrIp))
                    players.Add(activePlayer);
            }
            return players;
        }
    }
}
