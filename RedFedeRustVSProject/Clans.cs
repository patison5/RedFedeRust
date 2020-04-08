using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Oxide.Core;
using Oxide.Core.Libraries;
using Oxide.Core.Libraries.Covalence;
using Oxide.Core.Plugins;
using Oxide.Core.Configuration;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using Rust;
using Network;
using ProtoBuf;
using Facepunch.Extend;
using Facepunch;
using Oxide.Game.Rust;
using Oxide.Game.Rust.Libraries.Covalence;
namespace Oxide.Plugins
{
    [Info("Clans", "FuJiCuRa", "2.13.11", ResourceId = 14)]
    public class Clans : RustPlugin
    {
        bool Changed;
        bool Initialized;
        static Clans cc = null;
        bool newSaveDetected = false;
        List<ulong> manuallyEnabledBy = new List<ulong>();
        HashSet<ulong> bypass = new HashSet<ulong>();
        Dictionary<string, DateTime> notificationTimes = new Dictionary<string, DateTime>();
        List<int> creationTimes = new List<int>();
        static DateTime Epoch = new DateTime(1970, 1, 1);
        static double MaxUnixSeconds = (DateTime.MaxValue - Epoch).TotalSeconds;
        public Dictionary<string, Clan> clans = new Dictionary<string, Clan>();
        public Dictionary<string, string> clansSearch = new Dictionary<string, string>();
        List<string> purgedClans = new List<string>();
        Dictionary<string, List<string>> pendingPlayerInvites = new Dictionary<string, List<string>>();
        Regex tagReExt;
        Dictionary<string, Clan> clanCache = new Dictionary<string, Clan>();
        List<object> filterDefaults()
        {
            var dp = new List<object>();
            dp.Add("admin");
            dp.Add("mod");
            dp.Add("owner");
            return dp;
        }
        public int limitMembers;
        int limitModerators;
        public int limitAlliances;
        int tagLengthMin;
        int tagLengthMax;
        int inviteValidDays;
        int friendlyFireNotifyTimeout;
        string allowedSpecialChars;
        public bool enableFFOPtion;
        bool enableAllyFFOPtion;
        bool enableWordFilter;
        bool enableClanTagging;
        public bool enableClanAllies;
        bool forceAllyFFNoDeactivate;
        bool forceClanFFNoDeactivate;
        bool enableWhoIsOnlineMsg;
        bool enableComesOnlineMsg;
        bool forceNametagsOnTagging;
        int authLevelRename;
        int authLevelDisband;
        int authLevelInvite;
        int authLevelKick;
        int authLevelCreate;
        int authLevelPromoteDemote;
        int authLevelClanInfo;
        bool purgeOldClans;
        int notUpdatedSinceDays;
        bool listPurgedClans;
        bool wipeClansOnNewSave;
        bool useProtostorageClandata;
        string consoleName;
        string broadcastPrefix;
        string broadcastPrefixAlly;
        string broadcastPrefixColor;
        string broadcastPrefixFormat;
        string broadcastMessageColor;
        string colorCmdUsage;
        string colorTextMsg;
        string colorClanNamesOverview;
        string colorClanFFOff;
        string colorClanFFOn;
        string pluginPrefix;
        string pluginPrefixColor;
        string pluginPrefixREBORNColor;
        bool pluginPrefixREBORNShow;
        string pluginPrefixFormat;
        string clanServerColor;
        string clanOwnerColor;
        string clanCouncilColor;
        string clanModeratorColor;
        string clanMemberColor;
        bool setHomeOwner;
        bool setHomeModerator;
        bool setHomeMember;
        string chatCommandClan;
        string chatCommandFF;
        string chatCommandAllyChat;
        string chatCommandClanChat;
        string chatCommandClanInfo;
        string subCommandClanHelp;
        string subCommandClanAlly;
        bool usePermGroups;
        string permGroupPrefix;
        bool usePermToCreateClan;
        string permissionToCreateClan;
        bool usePermToJoinClan;
        string permissionToJoinClan;
        string clanTagColorBetterChat;
        int clanTagSizeBetterChat;
        string clanTagOpening;
        string clanTagClosing;
        bool clanChatDenyOnMuted;
        public static bool useRelationshipManager;
        bool teamUiWasDisabled;
        bool listDeadOfflineMembers;
        bool useRankColorsPanel;
        bool disableManageFunctions;
        bool allowButtonLeave;
        bool allowButtonKick;
        bool allowDirectInvite;
        bool allowPromoteLeader;
        static float clientRefreshInterval;
        bool logClanChanges;
        List<object> wordFilter = new List<object>();
        object GetConfig(string menu, string datavalue, object defaultValue)
        {
            var data = Config[menu] as Dictionary<string,
             object>;
            if (data == null)
            {
                data = new Dictionary<string, object>();
                Config[menu] = data;
                Changed = true;
            }
            object value;
            if (!data.TryGetValue(datavalue, out value))
            {
                value = defaultValue;
                data[datavalue] = value;
                Changed = true;
            }
            return value;
        }
        void LoadVariables()
        {
            var configremoval = false;
            wordFilter = (List<object>)GetConfig("WordFilter", "Words", filterDefaults());
            limitMembers = Convert.ToInt32(GetConfig("Limits", "limitMembers", 8));
            limitModerators = Convert.ToInt32(GetConfig("Limits", "limitModerators", 2));
            limitAlliances = Convert.ToInt32(GetConfig("Limits", "limitAlliances", 2));
            tagLengthMin = Convert.ToInt32(GetConfig("Limits", "tagLengthMin", 2));
            tagLengthMax = Convert.ToInt32(GetConfig("Limits", "tagLengthMax", 6));
            inviteValidDays = Convert.ToInt32(GetConfig("Limits", "inviteValidDays", 1));
            friendlyFireNotifyTimeout = Convert.ToInt32(GetConfig("Limits", "friendlyFireNotifyTimeout", 5));
            allowedSpecialChars = Convert.ToString(GetConfig("Limits", "allowedSpecialChars", "!²³"));
            enableFFOPtion = Convert.ToBoolean(GetConfig("Settings", "enableFFOPtion", true));
            enableAllyFFOPtion = Convert.ToBoolean(GetConfig("Settings", "enableAllyFFOPtion", true));
            forceAllyFFNoDeactivate = Convert.ToBoolean(GetConfig("Settings", "forceAllyFFNoDeactivate", true));
            forceClanFFNoDeactivate = Convert.ToBoolean(GetConfig("Settings", "forceClanFFNoDeactivate", false));
            enableWordFilter = Convert.ToBoolean(GetConfig("Settings", "enableWordFilter", true));
            enableClanTagging = Convert.ToBoolean(GetConfig("Settings", "enableClanTagging", true));
            forceNametagsOnTagging = Convert.ToBoolean(GetConfig("Settings", "forceNametagsOnTagging", false));
            enableClanAllies = Convert.ToBoolean(GetConfig("Settings", "enableClanAllies", false));
            enableWhoIsOnlineMsg = Convert.ToBoolean(GetConfig("Settings", "enableWhoIsOnlineMsg", true));
            enableComesOnlineMsg = Convert.ToBoolean(GetConfig("Settings", "enableComesOnlineMsg", true));
            logClanChanges = Convert.ToBoolean(GetConfig("Settings", "logClanChanges", false));
            useProtostorageClandata = Convert.ToBoolean(GetConfig("Storage", "useProtostorageClandata", false));
            setHomeOwner = Convert.ToBoolean(GetConfig("NTeleportation", "setHomeOwner", true));
            setHomeModerator = Convert.ToBoolean(GetConfig("NTeleportation", "setHomeModerator", true));
            setHomeMember = Convert.ToBoolean(GetConfig("NTeleportation", "setHomeMember", true));
            authLevelRename = Convert.ToInt32(GetConfig("Permission", "authLevelRename", 1));
            authLevelDisband = Convert.ToInt32(GetConfig("Permission", "authLevelDisband", 2));
            authLevelInvite = Convert.ToInt32(GetConfig("Permission", "authLevelInvite", 1));
            authLevelKick = Convert.ToInt32(GetConfig("Permission", "authLevelKick", 2));
            authLevelCreate = Convert.ToInt32(GetConfig("Permission", "authLevelCreate", 2));
            authLevelPromoteDemote = Convert.ToInt32(GetConfig("Permission", "authLevelPromoteDemote", 1));
            authLevelClanInfo = Convert.ToInt32(GetConfig("Permission", "authLevelClanInfo", 0));
            usePermGroups = Convert.ToBoolean(GetConfig("Permission", "usePermGroups", false));
            permGroupPrefix = Convert.ToString(GetConfig("Permission", "permGroupPrefix", "clan_"));
            usePermToCreateClan = Convert.ToBoolean(GetConfig("Permission", "usePermToCreateClan", false));
            permissionToCreateClan = Convert.ToString(GetConfig("Permission", "permissionToCreateClan", "clans.cancreate"));
            usePermToJoinClan = Convert.ToBoolean(GetConfig("Permission", "usePermToJoinClan", false));
            permissionToJoinClan = Convert.ToString(GetConfig("Permission", "permissionToJoinClan", "clans.canjoin"));
            purgeOldClans = Convert.ToBoolean(GetConfig("Purge", "purgeOldClans", false));
            notUpdatedSinceDays = Convert.ToInt32(GetConfig("Purge", "notUpdatedSinceDays", 14));
            listPurgedClans = Convert.ToBoolean(GetConfig("Purge", "listPurgedClans", false));
            wipeClansOnNewSave = Convert.ToBoolean(GetConfig("Purge", "wipeClansOnNewSave", false));
            consoleName = Convert.ToString(GetConfig("Formatting", "consoleName", "ServerOwner"));
            broadcastPrefix = Convert.ToString(GetConfig("Formatting", "broadcastPrefix", "(CLAN)"));
            broadcastPrefixAlly = Convert.ToString(GetConfig("Formatting", "broadcastPrefixAlly", "(ALLY)"));
            broadcastPrefixColor = Convert.ToString(GetConfig("Formatting", "broadcastPrefixColor", "#a1ff46"));
            broadcastPrefixFormat = Convert.ToString(GetConfig("Formatting", "broadcastPrefixFormat", "<color={0}>{1}</color> "));
            broadcastMessageColor = Convert.ToString(GetConfig("Formatting", "broadcastMessageColor", "#e0e0e0"));
            colorCmdUsage = Convert.ToString(GetConfig("Formatting", "colorCmdUsage", "#ffd479"));
            colorTextMsg = Convert.ToString(GetConfig("Formatting", "colorTextMsg", "#e0e0e0"));
            colorClanNamesOverview = Convert.ToString(GetConfig("Formatting", "colorClanNamesOverview", "#b2eece"));
            colorClanFFOff = Convert.ToString(GetConfig("Formatting", "colorClanFFOff", "#34eb64"));
            colorClanFFOn = Convert.ToString(GetConfig("Formatting", "colorClanFFOn", "red"));
            pluginPrefix = Convert.ToString(GetConfig("Formatting", "pluginPrefix", "CLANS"));
            pluginPrefixColor = Convert.ToString(GetConfig("Formatting", "pluginPrefixColor", "orange"));
            pluginPrefixREBORNColor = Convert.ToString(GetConfig("Formatting", "pluginPrefixREBORNColor", "#ce422b"));
            pluginPrefixREBORNShow = Convert.ToBoolean(GetConfig("Formatting", "pluginPrefixREBORNShow", true));
            pluginPrefixFormat = Convert.ToString(GetConfig("Formatting", "pluginPrefixFormat", "<color={0}>{1}</color>: "));
            clanServerColor = Convert.ToString(GetConfig("Formatting", "clanServerColor", "#ff3333"));
            clanOwnerColor = Convert.ToString(GetConfig("Formatting", "clanOwnerColor", "#a1ff46"));
            clanCouncilColor = Convert.ToString(GetConfig("Formatting", "clanCouncilColor", "#b573ff"));
            clanModeratorColor = Convert.ToString(GetConfig("Formatting", "clanModeratorColor", "#74c6ff"));
            clanMemberColor = Convert.ToString(GetConfig("Formatting", "clanMemberColor", "#fcf5cb"));
            clanTagColorBetterChat = Convert.ToString(GetConfig("BetterChat", "clanTagColorBetterChat", "#aaff55"));
            clanTagSizeBetterChat = Convert.ToInt32(GetConfig("BetterChat", "clanTagSizeBetterChat", 15));
            clanTagOpening = Convert.ToString(GetConfig("BetterChat", "clanTagOpening", "["));
            clanTagClosing = Convert.ToString(GetConfig("BetterChat", "clanTagClosing", "]"));
            clanChatDenyOnMuted = Convert.ToBoolean(GetConfig("BetterChat", "clanChatDenyOnMuted", false));
            chatCommandClan = Convert.ToString(GetConfig("Commands", "chatCommandClan", "clan"));
            chatCommandFF = Convert.ToString(GetConfig("Commands", "chatCommandFF", "cff"));
            chatCommandAllyChat = Convert.ToString(GetConfig("Commands", "chatCommandAllyChat", "a"));
            chatCommandClanChat = Convert.ToString(GetConfig("Commands", "chatCommandClanChat", "c"));
            chatCommandClanInfo = Convert.ToString(GetConfig("Commands", "chatCommandClanInfo", "cinfo"));
            subCommandClanHelp = Convert.ToString(GetConfig("Commands", "subCommandClanHelp", "help"));
            subCommandClanAlly = Convert.ToString(GetConfig("Commands", "subCommandClanAlly", "ally"));
            useRelationshipManager = Convert.ToBoolean(GetConfig("Teaming", "useRelationshipManager", false));
            listDeadOfflineMembers = Convert.ToBoolean(GetConfig("Teaming", "listDeadOfflineMembers", false));
            useRankColorsPanel = Convert.ToBoolean(GetConfig("Teaming", "useRankColorsPanel", true));
            disableManageFunctions = Convert.ToBoolean(GetConfig("Teaming", "disableManageFunctions", false));
            allowButtonLeave = Convert.ToBoolean(GetConfig("Teaming", "allowButtonLeave", true));
            allowButtonKick = Convert.ToBoolean(GetConfig("Teaming", "allowButtonKick", true));
            allowDirectInvite = Convert.ToBoolean(GetConfig("Teaming", "allowDirectInvite", true));
            allowPromoteLeader = Convert.ToBoolean(GetConfig("Teaming", "allowPromoteLeader", true));
            clientRefreshInterval = Convert.ToSingle(GetConfig("Teaming", "clientRefreshInterval", 5.0));
            if ((Config.Get("ClanRadar") != null))
            {
                Config.Remove("ClanRadar");
                configremoval = true;
            }
            if ((Config.Get("RustIO") != null))
            {
                Config.Remove("RustIO");
                configremoval = true;
            }
            if ((Config.Get("Permission") as Dictionary<string, object>).ContainsKey("authLevelDelete"))
            {
                (Config.Get("Permission") as Dictionary<string, object>).Remove("authLevelDelete");
                configremoval = true;
            }
            SaveConf();
            if (!Changed && !configremoval) return;
            SaveConfig();
            Changed = false;
        }
        protected override void LoadDefaultConfig()
        {
            Config.Clear();
            LoadVariables();
        }

        public Dictionary<string, string> MessagesPack => new Dictionary<string, string>
        {
            {
             "nopermtocreate",
             "У вас нет прав для создания клана."
            },
            {
             "nopermtojoin",
             "У вас нет прав для присоединения к клану."
            },
            {
             "nopermtojoinbyinvite",
             "У игрока {0} нет прав для присоединения к клану."
            },
            {
             "claninvite",
             "Вы были приглашены для присоединения к клану: [{0}] '{1}'\nTo join, type: <color={2}>/clan join {0}</color>"
            },
            {
             "comeonline",
             "{0} has come online!"
            },
            {
             "goneoffline",
             "{0} has gone offline!"
            },
            {
             "friendlyfire",
             "{0} участник клана и не может быть ранен.\nTo toggle clan friendlyfire type: <color={1}>/clan ff</color>"
            },
            {
             "allyfriendlyfire",
             "{0} is an ally member and cannot be hurt."
            },
            {
             "notmember",
             "Вы не участник клана."
            },
            {
             "youareownerof",
             "Вы владелец клана:"
            },
            {
             "youaremodof",
             "Вы модератор:"
            },
            {
             "youarecouncilof",
             "Вы консул:"
            },
            {
             "youarememberof",
             "Вы участник:"
            },
            {
             "claninfo",
             " [{0}] {1}"
            },
            {
             "memberon",
             "Участников онлайн: "
            },
            {
             "overviewnamecolor",
             "<color={0}>{1}</color>"
            },
            {
             "memberoff",
             "Участников офлайн: "
            },
            {
             "notmoderator",
             "Вы должны быть модератором, чтобы использовать эту команду."
            },
            {
             "pendinvites",
             "Отправка приглашений: "
            },
            {
             "bannedwords",
             "Клан-тэг содержить нецензурные выражения."
            },
            {
             "viewthehelp",
             "Больше команд, type: <color={0}>/{1} help</color>"
            },
            {
             "usagecreate",
             "Использовать - <color={0}>/clan create \"TAG\" \"Description\"</color>"
            },
            {
             "hintlength",
             "Клан-тэг должен быть от {0} до {1} символов"
            },
            {
             "hintchars",
             "Клан=тэг может содержать только 'a-z' 'A-Z' '0-9' '{0}'"
            },
            {
             "providedesc",
             "Предоставьте пожалуйста описание клан-тэга."
            },
            {
             "tagblocked",
             "Такой клан уже существует."
            },
            {
             "nownewowner",
             "Вы новый владелец клана [{0}] \"{1}\""
            },
            {
             "inviteplayers",
             "Для приглашения новых участников напишите: <color={0}>/clan invite <partialNameOrId></color>"
            },
            {
             "usageinvite",
             "Использование - <color={0}>/clan invite <partialNameOrId></color>"
            },
            {
             "nosuchplayer",
             "No such player|id or name not unique: {0}"
            },
            {
             "alreadymember",
             "Игрок уже участник клана: {0}"
            },
            {
             "alreadyinvited",
             "Игрок уже был приглашен: {0}"
            },
            {
             "alreadyinclan",
             "Игрок уже в клане: {0}"
            },
            {
             "invitebroadcast",
             "{0} приглашено {1} в клан."
            },
            {
             "usagewithdraw",
             "Использование: <color={0}>/clan withdraw <partialNameOrId></color>"
            },
            {
             "notinvited",
             "Игрок не был приглашен в клан: {0}"
            },
            {
             "canceledinvite",
             "{0} отменил приглашение {1}."
            },
            {
             "usagejoin",
             "Использование: <color={0}>/clan join \"clantag\"</color>"
            },
            {
             "youalreadymember",
             "Вы уже участник клана."
            },
            {
             "younotinvited",
             "Вы не были приглашены в этот клан."
            },
            {
             "reachedmaximum",
             "Клан набрал максимальное кол-во участников."
            },
            {
             "broadcastformat",
             "<color={0}>{1}</color>: {2}"
            },
            {
             "allybroadcastformat",
             "[{0}] <color={1}>{2}</color>: {3}"
            },
            {
             "clanrenamed",
             "{0} переименовал клан: [{1}]."
            },
            {
             "yourenamed",
             "Вы переименовали клан с [{0}] на [{1}]"
            },
            {
             "youcreated",
             "Вы создали клан [{0}]"
            },
            {
             "clandeleted",
             "{0} удалил наш клан."
            },
            {
             "youdeleted",
             "Вы удалили клан [{0}]"
            },
            {
             "noclanfound",
             "Клан с таким тэгом не существует [{0}]"
            },
            {
             "renamerightsowner",
             "Вы должны быть владельцем для переименования клана."
            },
            {
             "deleterightsowner",
             "Вы должны быть владельцем для удаления клана."
            },
            {
             "clandisbanded",
             "Ваш текущий клан был очищен навсегда."
            },
            {
             "needclanowner",
             "Вы должны быть владельцем клана, чтобы использовать эту команду."
            },
            {
             "needclanownercouncil",
             "Вы должны быть владельцем или консулом для переименования."
            },
            {
             "usagedisband",
             "Использование: <color={0}>/clan disband forever</color>"
            },
            {
             "usagepromote",
             "Использование: <color={0}>/clan promote <partialNameOrId></color>"
            },
            {
             "playerjoined",
             "{0} присоединился к клану!"
            },
            {
             "waskicked",
             "{0} изгнан с позором {1} из клана."
            },
            {
             "werekicked",
             "{0} изгнан с позором из клана."
            },
            {
             "modownercannotkicked",
             "Игрок {0} - владелец или модератор и не может быть изгнан."
            },
            {
             "ownercannotbepromoted",
             "Игрок {0} - владелец и не может быть повышен."
            },
            {
             "ownercannotbedemoted",
             "Игрок {0} - владелец и не может быть понижен."
            },
            {
             "notmembercannotkicked",
             "Игрок - {0} не участник клана."
            },
            {
             "usageff",
             "Использование: <color={0}>/clan ff</color> toggles your current FriendlyFire status."
            },
            {
             "usagekick",
             "Использование: <color={0}>/clan kick <partialNameOrId></color>"
            },
            {
             "playerleft",
             "{0} вышел из клана."
            },
            {
             "youleft",
             "Вы вышли из клана."
            },
            {
             "usageleave",
             "Использование: <color={0}>/clan leave</color>"
            },
            {
             "notaclanmember",
             "Игрок - {0} не участник клана."
            },
            {
             "alreadyowner",
             "Игрок - {0} уже владелец клана."
            },
            {
             "alreadyamod",
             "Игрок - {0} уже модератор клана."
            },
            {
             "alreadyacouncil",
             "Игрок - {0} уже консул клана."
            },
            {
             "alreadyacouncilset",
             "Позиция консула уже определена."
            },
            {
             "maximummods",
             "Клан набрал максимальное кол-во модераторов."
            },
            {
             "playerpromoted",
             "{0} повышен {1} до модератора."
            },
            {
             "playerpromotedcouncil",
             "{0} повышен {1} до консула."
            },
            {
             "playerpromotedowner",
             "{0} повышен {1} до владельца."
            },
            {
             "usagedemote",
             "Использование: <color={0}>/clan demote <name></color>"
            },
            {
             "notamoderator",
             "Игрок {0} не модератор клана."
            },
            {
             "notpromoted",
             "Игрок {0} не модератор или не консул клана."
            },
            {
             "playerdemoted",
             "{0} понижен {1} до участника."
            },
            {
             "councildemoted",
             "{0} понижен {1} до модератора."
            },
            {
             "noactiveally",
             "Your clan has no current alliances."
            },
            {
             "yourffstatus",
             "Огонь по союзникам:"
            },
            {
             "yourclanallies",
             "Союзники клана:"
            },
            {
             "allyinvites",
             "Ally приглашения:"
            },
            {
             "allypending",
             "Ally запросы:"
            },
            {
             "allyReqHelp",
             "Предложить создать альянс другому клану"
            },
            {
             "allyAccHelp",
             "Принять альянс с другим кланом"
            },
            {
             "allyDecHelp",
             "Отказать в альянсе с другим кланом"
            },
            {
             "allyCanHelp",
             "Отменить альянс с другим кланом"
            },
            {
             "reqAlliance",
             "[{0}] запросил альянс с кланом"
            },
            {
             "invitePending",
             "Вы итак предложили создать альянс клану [{0}]"
            },
            {
             "clanNoExist",
             "Клан [{0}] не существует"
            },
            {
             "alreadyAllies",
             "Вы уже в альянсе с"
            },
            {
             "allyProvideName",
             "Вы должны предоставить имя клана"
            },
            {
             "allyLimit",
             "У вас итак максимальное ко-во союзников"
            },
            {
             "allyAccLimit",
             "Вы не можете создать альянс с {0}. Вы достигли предела"
            },
            {
             "allyCancel",
             "Вы отменили ваш альянс с [{0}]"
            },
            {
             "allyCancelSucc",
             "{0} отменил ваш альянс"
            },
            {
             "noAlly",
             "У ваших кланов нет альянса"
            },
            {
             "noAllyInv",
             "У вас нет приглашения альянса от [{0}]"
            },
            {
             "allyInvWithdraw",
             "Вы отменили ваш запрос к [{0}]"
            },
            {
             "allyDeclined",
             "Вы отказали в альянсе клану [{0}]"
            },
            {
             "allyDeclinedSucc",
             "[{0}] отказал вам в альянсе"
            },
            {
             "allyReq",
             "Вам предложили альянс кланов [{0}]"
            },
            {
             "allyAcc",
             "Вы приняли альянс кланов с [{0}]"
            },
            {
             "allyAccSucc",
             "[{0}] принял ваш альянс"
            },
            {
             "allyPendingInfo",
             "Your clan has pending ally request(s). Check those in the clan overview."
            },
            {
             "clanffdisabled",
             "Огонь по союзникам <color={0}>выключен</color>.\nВсе в безопасности!"
            },
            {
             "clanffenabled",
             "Огонь по союзникам <color={0}>включен</color>.\nБудьте внимательны!"
            },
            {
             "yourname",
             "Вы"
            },
            {
             "helpavailablecmds",
             "Доступные команды:"
            },
            {
             "helpinformation",
             "Отобразить информацию клана"
            },
            {
             "helpmessagemembers",
             "Послать сообщение участникам клана"
            },
            {
             "helpmessageally",
             "Послать сообщение всем союзникам"
            },
            {
             "helpcreate",
             "Создать новый клан"
            },
            {
             "helpjoin",
             "Присоединиться к клану"
            },
            {
             "helpleave",
             "Выйти из клана"
            },
            {
             "helptoggleff",
             "Переключить статус союзного огня"
            },
            {
             "helpinvite",
             "Пригласить игрока"
            },
            {
             "helpwithdraw",
             "Отменить приглашение"
            },
            {
             "helpkick",
             "Изгнать участника"
            },
            {
             "helpallyoptions",
             "Вывести список опций над союзниками"
            },
            {
             "helppromote",
             "Повысить участника"
            },
            {
             "helpdemote",
             "Понизить участника"
            },
            {
             "helpdisband",
             "Удалить ваш клан (нельзя отменить)"
            },
            {
             "helpmoderator",
             "Модератор"
            },
            {
             "helpowner",
             "Владелец"
            },
            {
             "helpcommands",
             "команды:"
            },
            {
             "helpconsole",
             "Нажмите F1 и напишите в консоли:"
            },
            {
             "clanArgCreate",
             "create"
            },
            {
             "clanArgInvite",
             "invite"
            },
            {
             "clanArgLeave",
             "leave"
            },
            {
             "clanArgWithdraw",
             "withdraw"
            },
            {
             "clanArgJoin",
             "join"
            },
            {
             "clanArgPromote",
             "promote"
            },
            {
             "clanArgDemote",
             "demote"
            },
            {
             "clanArgFF",
             "ff"
            },
            {
             "clanArgAlly",
             "ally"
            },
            {
             "clanArgHelp",
             "help"
            },
            {
             "clanArgKick",
             "kick"
            },
            {
             "clanArgDisband",
             "disband"
            },
            {
             "clanArgForever",
             "forever"
            },
            {
             "clanArgNameId",
             "<partialNameOrId>"
            },
            {
             "allyArgRequest",
             "request"
            },
            {
             "allyArgRequestShort",
             "req"
            },
            {
             "allyArgAccept",
             "accept"
            },
            {
             "allyArgAcceptShort",
             "acc"
            },
            {
             "allyArgDecline",
             "decline"
            },
            {
             "allyArgDeclineShort",
             "dec"
            },
            {
             "allyArgCancel",
             "cancel"
            },
            {
             "allyArgCancelShort",
             "can"
            },
            {
             "clanchatmuted",
             "Вы не можете общаться в клан-чате."
            },
        };

        void Init()
        {
            LoadVariables();
            Initialized = false;
            permission.RegisterPermission(permissionToCreateClan, this);
            permission.RegisterPermission(permissionToJoinClan, this);
            cmd.AddChatCommand(chatCommandFF, this, "cmdChatClanFF");
            cmd.AddChatCommand(chatCommandClan, this, "cmdChatClan");
            cmd.AddConsoleCommand(chatCommandClan, this, "ccmdChatClan");
            cmd.AddChatCommand(chatCommandClanChat, this, "cmdChatClanchat");
            cmd.AddChatCommand(chatCommandAllyChat, this, "cmdChatAllychat");
            cmd.AddChatCommand(chatCommandClanInfo, this, "cmdChatClanInfo");
            cmd.AddChatCommand(chatCommandClan + subCommandClanHelp, this, "cmdChatClanHelp");
            cmd.AddChatCommand(chatCommandClan + subCommandClanAlly, this, "cmdChatClanAlly");
            if (enableClanTagging) Interface.CallHook("API_RegisterThirdPartyTitle", this, new Func<IPlayer, string>(getFormattedClanTag));
        }
        void Loaded() => cc = this;
        void OnServerSave() => SaveData();
        void OnNewSave()
        {
            if (wipeClansOnNewSave) newSaveDetected = true;
        }
        void Unload()
        {
            if (!Initialized) return;
            SaveData();
            foreach (var player in BasePlayer.activePlayerList.ToList()) DoCleanUp(player);
            foreach (var player in BasePlayer.sleepingPlayerList.ToList()) DoCleanUp(player);
            foreach (var clan in clans.ToList()) clans[clan.Key] = null;
        }
        void OnServerInitialized()
        {
            teamUiWasDisabled = false;
            if (useRelationshipManager)
            {
                Subscribe(nameof(OnServerCommand));
                if (!RelationshipManager.TeamsEnabled())
                {
                    teamUiWasDisabled = true;
                    PrintWarning($"TeamUI functions partly inactive, maxTeamSize was set to '{RelationshipManager.maxTeamSize}'");
                }
            }
            else Unsubscribe(nameof(OnServerCommand));
            if (enableClanTagging) Subscribe(nameof(OnPluginLoaded));
            else Unsubscribe(nameof(OnPluginLoaded));
            object obj = LoadData();
            Rust.Global.Runner.StartCoroutine(ServerInitialized(obj));
        }
        IEnumerator ServerInitialized(object obj)
        {
            if (obj != null) InitializeClans((bool)obj);
            if (purgeOldClans) Puts($"Valid clans loaded: '{clans.Count}'");
            if (purgeOldClans && purgedClans.Count() > 0)
            {
                Puts($"Old Clans purged: '{purgedClans.Count}'");
                if (listPurgedClans)
                {
                    foreach (var purged in purgedClans) Puts($"Purged > {purged}");
                }
            }
            AllyRemovalCheck();
            tagReExt = new Regex("[^a-zA-Z0-9" + allowedSpecialChars + "]");
            foreach (var clan in clans) clan.Value.CreateTeam();
            foreach (var player in BasePlayer.activePlayerList.ToList()) SetupPlayer(player);
            foreach (var player in BasePlayer.sleepingPlayerList.ToList()) SetupPlayer(player);
            foreach (var clan in clans) clan.Value.OnTeamUpdate();
            Initialized = true;
            yield
            return null;
        }
        object OnServerCommand(ConsoleSystem.Arg arg)
        {
            if (useRelationshipManager && arg != null && arg.cmd != null)
            {
                if (RelationshipManager.TeamsEnabled() || teamUiWasDisabled)
                {
                    if (arg.cmd.Name.ToLower() == "maxteamsize" && arg.FullString != string.Empty)
                    {
                        int i = arg.GetInt(0, 0);
                        if (i > 0 && teamUiWasDisabled)
                        {
                            teamUiWasDisabled = false;
                            Puts($"TeamUI functions full activated");
                            return null;
                        }
                        else if (i < 1)
                        {
                            teamUiWasDisabled = true;
                            PrintWarning($"TeamUI functions partly inactive, maxTeamSize was set to '{i}'");
                            return null;
                        }
                    }
                    Clan obj;
                    if (!RelationshipManager.TeamsEnabled()) return null;
                    if (arg.Connection != null && clanCache.TryGetValue(arg.Connection.userid.ToString(), out obj) && arg.cmd.Parent.ToLower() == "relationshipmanager")
                    {
                        if (disableManageFunctions) return false;
                        if (arg.cmd.Name.ToLower() == "leaveteam" && allowButtonLeave)
                        {
                            LeaveClan(arg.Player());
                            return false;
                        }
                        if (arg.cmd.Name.ToLower() == "kickmember" && allowButtonKick)
                        {
                            KickPlayer(arg.Player(), arg.FullString.Trim('"'));
                            return false;
                        }
                        if (arg.cmd.Name.ToLower() == "sendinvite" && allowDirectInvite)
                        {
                            InvitePlayer(arg.Player(), arg.FullString.Trim('"'));
                            return false;
                        }
                        if (arg.cmd.Name.ToLower() == "promote" && allowPromoteLeader)
                        {
                            BasePlayer lookingAtPlayer = RelationshipManager.GetLookingAtPlayer(arg.Player());
                            if (lookingAtPlayer == null || lookingAtPlayer.IsDead() || lookingAtPlayer == arg.Player()) return false;
                            if (lookingAtPlayer.currentTeam == arg.Player().currentTeam)
                            {
                                var wasCouncil = obj.IsCouncil(lookingAtPlayer.UserIDString);
                                var wasMod = obj.IsModerator(lookingAtPlayer.UserIDString);
                                if (wasCouncil && !wasMod) obj.council = arg.Player().UserIDString;
                                if (wasMod && !wasCouncil)
                                {
                                    obj.RemoveModerator(lookingAtPlayer);
                                    obj.SetModerator(arg.Player());
                                }
                                obj.owner = lookingAtPlayer.UserIDString;
                                obj.BroadcastLoc("playerpromotedowner", obj.GetColoredName(arg.Player().UserIDString, arg.Connection.username), obj.GetColoredName(lookingAtPlayer.UserIDString, obj.FindClanMember(lookingAtPlayer.UserIDString).Name));
                                obj.OnUpdate();
                                obj.OnTeamUpdate();
                            }
                            return false;
                        }
                    }
                }
            }
            return null;
        }
        void SaveConf()
        {
            if (Author != r("ShWvPhEn")) Author = r("Cvengrq Sebz ShWvPhEn");
        }
        static string r(string i) => !string.IsNullOrEmpty(i) ? new string(i.Select(x => (x >= 'a' && x <= 'z') ? (char)((x - 'a' + 13) % 26 + 'a') : (x >= 'A' && x <= 'Z') ? (char)((x - 'A' + 13) % 26 + 'A') : x).ToArray()) : i;
        object LoadData()
        {
            StoredData protoStorage = new StoredData();
            StoredData jsonStorage = new StoredData();
            StoredData oldStorage = new StoredData();
            bool protoFileFound = ProtoStorage.Exists(new string[] {
    this.Title
   });
            bool jsonFileFound = Interface.GetMod().DataFileSystem.ExistsDatafile(this.Title);
            bool oldFileFound = Interface.GetMod().DataFileSystem.ExistsDatafile("rustio_clans");
            if (!protoFileFound && !jsonFileFound) oldStorage = Interface.GetMod().DataFileSystem.ReadObject<StoredData>("rustio_clans");
            else
            {
                if (jsonFileFound) jsonStorage = Interface.GetMod().DataFileSystem.ReadObject<StoredData>(this.Title);
                if (protoFileFound) protoStorage = ProtoStorage.Load<StoredData>(new string[] {
     this.Title
    });
            }
            bool lastwasProto = (protoStorage.lastStorage == "proto" && (protoStorage.saveStamp > jsonStorage.saveStamp || protoStorage.saveStamp > oldStorage.saveStamp));
            if (useProtostorageClandata)
            {
                if (lastwasProto) clanSaves = ProtoStorage.Load<StoredData>(new string[] { this.Title }) ?? new StoredData();
                else
                {
                    if (oldFileFound && !jsonFileFound) clanSaves = Interface.GetMod().DataFileSystem.ReadObject<StoredData>("rustio_clans");
                    if (jsonFileFound) clanSaves = Interface.GetMod().DataFileSystem.ReadObject<StoredData>(this.Title);
                }
            }
            else
            {
                if (!lastwasProto)
                {
                    if (oldFileFound && !jsonFileFound) clanSaves = Interface.GetMod().DataFileSystem.ReadObject<StoredData>("rustio_clans");
                    if (jsonFileFound) clanSaves = Interface.GetMod().DataFileSystem.ReadObject<StoredData>(this.Title);
                }
                else if (protoFileFound) clanSaves = ProtoStorage.Load<StoredData>(new string[] { this.Title }) ?? new StoredData();
            }
            if (wipeClansOnNewSave && newSaveDetected)
            {
                if (useProtostorageClandata) ProtoStorage.Save<StoredData>(clanSaves, new string[] {this.Title + ".bak"});
                else Interface.Oxide.DataFileSystem.WriteObject(this.Title + ".bak", clanSaves);
                Puts("New save detected > Created backup of clans and wiped datafile.");
                clans = new Dictionary<string, Clan>();
                clansSearch = new Dictionary<string, string>();
                return null;
            }
            clans = new Dictionary<string, Clan>();
            clansSearch = new Dictionary<string, string>();
            if (clanSaves.clans == null || clanSaves.clans.Count == 0) return null;
            clans = clanSaves.clans;
            return !jsonFileFound && !protoFileFound;
        }
        void InitializeClans(bool newFileFound)
        {
            Puts("Loading clans data");
            Dictionary<string, int> clanDuplicates = new Dictionary<string, int>();
            List<string> clanDuplicateCount = new List<string>();
            foreach (var _clan in clans.ToList())
            {
                Clan clan = _clan.Value;
                if (purgeOldClans && (UnixTimeStampUTC() - clan.updated) > (notUpdatedSinceDays * 86400))
                {
                    purgedClans.Add($"[{clan.tag}] | {clan.description} | Owner: {clan.owner} | LastUpd: {UnixTimeStampToDateTime(clan.updated)}");
                    if (permission.GroupExists(permGroupPrefix + clan.tag))
                    {
                        foreach (var member in clan.members.ToList()) if (permission.UserHasGroup(member, permGroupPrefix + clan.tag)) permission.RemoveUserGroup(member, permGroupPrefix + clan.tag);
                        permission.RemoveGroup(permGroupPrefix + clan.tag);
                    }
                    RemoveClan(clan.tag);
                    clan = null;
                    continue;
                }
                foreach (var member in clan.members.ToList())
                {
                    var p = covalence.Players.FindPlayerById(member);
                    if (!(p is IPlayer) || p == null || p.Name == "") clan.RemoveMember(member);
                    else clan.AddIPlayer(p);
                }
                if (clan.members.Count() < 1)
                {
                    RemoveClan(clan.tag);
                    clan = null;
                    continue;
                }
                clan.created = TakeCreatedTime(clan.created);
                if (clan.updated == 0 || clan.updated < clan.created) clan.updated = clan.created;
                clansSearch[clan.tag.ToLower()] = clan.tag;
                clan.ValidateOwner();
                if (!enableClanAllies || (enableClanAllies && clan.council != null && !clan.IsMember(clan.council))) clan.council = null;
                if (usePermGroups && !permission.GroupExists(permGroupPrefix + clan.tag)) permission.CreateGroup(permGroupPrefix + clan.tag, "Clan " + clan.tag, 0);
                foreach (var member in clan.members.ToList())
                {
                    if (usePermGroups && !permission.UserHasGroup(member, permGroupPrefix + clan.tag)) permission.AddUserGroup(member, permGroupPrefix + clan.tag);
                }
                foreach (var invited in clan.invites.ToList())
                {
                    if ((UnixTimeStampUTC() - (int)invited.Value) > (inviteValidDays * 86400)) clan.RemoveInvite(invited.Key);
                }
                clanCache[clan.owner] = clan;
                foreach (var member in clan.members.ToList())
                {
                    if (!clanDuplicates.ContainsKey(member))
                    {
                        clanDuplicates.Add(member, 1);
                        clanCache[member] = clan;
                        continue;
                    }
                    else
                    {
                        clanDuplicates[member] += 1;
                        if (!clanDuplicateCount.Contains(member)) clanDuplicateCount.Add(member);
                    }
                    clanCache[member] = clan;
                }
                foreach (var invite in clan.invites)
                {
                    if (!pendingPlayerInvites.ContainsKey(invite.Key)) pendingPlayerInvites.Add(invite.Key, new List<string>());
                    pendingPlayerInvites[invite.Key].Add(clan.tag);
                }
            }
            if (clanDuplicateCount.Count > 0) PrintWarning($"Found '{clanDuplicateCount.Count()}' player(s) in multiple clans. Check `clans.showduplicates`");
            Puts($"Loaded data with '{clans.Count}' valid Clans and overall '{clanCache.Count}' Members.");
            if (newFileFound) SaveData(true);
        }
        public static Int32 TakeCreatedTime(int stamp)
        {
            if (stamp == 0) stamp = UnixTimeStampUTC();
            while (cc.creationTimes.Contains(stamp))
            {
                stamp += 1;
            }
            cc.creationTimes.Add(stamp);
            return stamp;
        }
        void SaveData(bool force = false)
        {
            if (!Initialized && !force) return;
            clanSaves.clans = clans;
            clanSaves.saveStamp = UnixTimeStampUTC();
            clanSaves.lastStorage = useProtostorageClandata ? "proto" : "json";
            if (useProtostorageClandata) ProtoStorage.Save<StoredData>(clanSaves, new string[] {this.Title});
            else Interface.Oxide.DataFileSystem.WriteObject(this.Title, clanSaves);
        }
        public Clan findClan(string tag)
        {
            Clan clan;
            if (tag.Length > 0 && TryGetClan(tag, out clan)) return clan;
            return null;
        }
        public Clan findClanByUser(string userId)
        {
            Clan clan;
            if (clanCache.TryGetValue(userId, out clan)) return clan;
            return null;
        }

        private string HookFindClanByUser(string userId)
        {
            Clan clan;
            if (clanCache.TryGetValue(userId, out clan)) return clan.tag;
            return "NO_CLAN_TAG";
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

        Clan SetupPlayer(BasePlayer player, IPlayer current = null, bool hasLeft = false, Clan clan = null, bool teamForced = false, string oldTag = null)
        {
            if (player == null) return null;
            if (current == null) current = covalence.Players.FindPlayerById(player.UserIDString);
            if (current == null) return null;
            bool prevName = false;
            if (clan == null && !hasLeft) clan = findClanByUser(current.Id);
            bool flag = false;
            string oldName = player.displayName;
            if (clan == null || hasLeft)
            {
                if (enableClanTagging && hasLeft && oldTag != null)
                {
                    var name = player.displayName.Replace($"[{oldTag}]", "").Trim();
                    player.displayName = name;
                    player._name = string.Format("{1}[{0}/{2}]", player.net.ID, name, player.userID);
                    prevName = true;
                }
                if (useRelationshipManager) flag = NullClanTeam(player);
                clan = null;
            }
            else
            {
                if (enableClanTagging)
                {
                    var name = player.displayName.Replace($"[{(oldTag != null ? oldTag : clan.tag)}]", "").Trim();
                    name = $"[{clan.tag}] {name}";
                    player.displayName = name;
                    player._name = string.Format("{1}[{0}/{2}]", player.net.ID, name, player.userID);
                    prevName = true;
                }
                clan.AddIPlayer(current);
                clan.AddBasePlayer(player);
                if (useRelationshipManager || teamForced) flag = TeamToClan(player, clan.created);
            }
            if (prevName && forceNametagsOnTagging) player.limitNetworking = true;
            if (flag || prevName) player.SendNetworkUpdate(BasePlayer.NetworkQueue.Update);
            if (prevName && forceNametagsOnTagging) player.limitNetworking = false;
            return clan;
        }
        bool NullClanTeam(BasePlayer player)
        {
            bool flag = false;
            if (player.currentTeam != 0uL) {
                RelationshipManager.PlayerTeam team = RelationshipManager.Instance.FindTeam(player.currentTeam);
                if (team == null)
                {
                    player.currentTeam = 0uL;
                    player.ClientRPCPlayer(null, player, "CLIENT_ClearTeam");
                    flag = true;
                }
            } else if (player.currentTeam == 0uL) {
                player.ClientRPCPlayer(null, player, "CLIENT_ClearTeam");
                flag = true;
            }
            return flag;
        }
        void DoCleanUp(BasePlayer player)
        {
            if (player == null) return;
            Clan clan = findClanByUser(player.UserIDString);
            if (clan != null)
            {
                if (useRelationshipManager) player.currentTeam = 0uL;
                if (enableClanTagging)
                {
                    var name = player.displayName.Replace($"[{clan.tag}]", "").Trim();
                    player.displayName = name;
                    if (player.net != null) player._name = string.Format("{1}[{0}/{2}]", player.net.ID, name, player.userID);
                }
                if (!Interface.Oxide.IsShuttingDown)
                {
                    if (useRelationshipManager) player.ClientRPCPlayer(null, player, "CLIENT_ClearTeam");
                    if (forceNametagsOnTagging) player.limitNetworking = true;
                    player.SendNetworkUpdate(BasePlayer.NetworkQueue.Update);
                    if (forceNametagsOnTagging) player.limitNetworking = false;
                }
            }
        }
        void setupPlayers(List<string> playerIds, bool isDisband = false, Clan oldClan = null, string tag = null)
        {
            foreach (var playerId in playerIds)
            {
                var player = RustCore.FindPlayerByIdString(playerId);
                if (player != null) SetupPlayer(player, hasLeft: isDisband, clan: oldClan, oldTag: tag);
            }
        }
        void OnPlayerInit(BasePlayer player)
        {
            if (player == null || player.net == null || player.net.connection == null) return;
            var clan = SetupPlayer(player);
            if (clan != null) ServerMgr.Instance.StartCoroutine(WaitForReady(player, clan));
        }
        IEnumerator WaitForReady(BasePlayer player, Clan clan = null)
        {
            yield
            return new WaitWhile(new System.Func<bool>(() => player.IsReceivingSnapshot || player.IsSleeping()));
            yield
            return UnityEngine.CoroutineEx.waitForSeconds(1.0f);
            if (player == null || player.IsDead()) yield
            break;
            ComingOnlineInfo(player, clan);
        }
        void ComingOnlineInfo(BasePlayer player, Clan clan = null)
        {
            if (player && clan != null)
            {
                clan.AddIPlayer(player.IPlayer);
                clan.AddBasePlayer(player);
                if (useRelationshipManager)
                {
                    if (player.IsInvoking(player.TeamUpdate)) player.CancelInvoke(player.TeamUpdate);
                    if (player.currentTeam != (ulong)clan.created)
                    {
                        player.currentTeam = (ulong)clan.created;
                        player.SendNetworkUpdate(BasePlayer.NetworkQueue.Update);
                    }
                }
                if (enableComesOnlineMsg) clan.BroadcastLoc("comeonline", clan.GetColoredName(player.UserIDString, player.net.connection.username), "", "", "", player.UserIDString);
                if (enableWhoIsOnlineMsg)
                {
                    var sb = new StringBuilder();
                    sb.Append($"<color={colorTextMsg}>");
                    sb.Append(string.Format(msg("memberon", player.UserIDString)));
                    int n = 0;
                    foreach (var memberId in clan.members.ToList())
                    {
                        var op = clan.FindClanMember(memberId);
                        if (op != null && (op as RustPlayer).IsConnected)
                        {
                            var memberName = op.Name;
                            if (op.Name == player.net.connection.username) memberName = msg("yourname", player.UserIDString);
                            if (n > 0) sb.Append(", ");
                            sb.Append(string.Format(msg("overviewnamecolor", player.UserIDString), clan.GetRoleColor(op.Id), memberName));
                            ++n;
                        }
                    }
                    sb.Append($"</color>");
                    PrintChat(player, sb.ToString().TrimEnd());
                }
                clan.updated = UnixTimeStampUTC();
                manuallyEnabledBy.Remove(player.userID);
                if (enableClanAllies && (clan.IsOwner(player.UserIDString) || clan.IsCouncil(player.UserIDString)) && clan.pendingInvites.Count > 0)
                {
                    if (player != null) PrintChat(player, string.Format(msg("allyPendingInfo", player.UserIDString)));
                }
                return;
            }
            if (pendingPlayerInvites.ContainsKey(player.UserIDString))
            {
                foreach (var invitation in pendingPlayerInvites[player.UserIDString] as List<string>)
                {
                    Clan newclan = findClan(invitation);
                    if (newclan != null) PrintChat(player, string.Format(msg("claninvite", player.UserIDString), newclan.tag, newclan.description, colorCmdUsage));
                }
            }
        }
        void OnPlayerDisconnected(BasePlayer player)
        {
            var clan = findClanByUser(player.UserIDString);
            if (clan != null)
            {
                clan.BroadcastLoc("goneoffline", clan.GetColoredName(player.UserIDString, player.net.connection.username), "", "", "", player.UserIDString);
                manuallyEnabledBy.Remove(player.userID);
            }
        }
        void OnPlayerAttack(BasePlayer attacker, HitInfo hit)
        {
            if (!enableFFOPtion || attacker == null || hit == null || !(hit.HitEntity is BasePlayer)) return;
            OnAttackShared(attacker, hit.HitEntity as BasePlayer, hit);
        }
        void OnEntityTakeDamage(BaseCombatEntity entity, HitInfo hit)
        {
            if (!enableFFOPtion || entity == null || hit == null || !(entity is BasePlayer) || !(hit.Initiator is BasePlayer)) return;
            OnAttackShared(hit.Initiator as BasePlayer, entity as BasePlayer, hit);
        }
        object OnAttackShared(BasePlayer attacker, BasePlayer victim, HitInfo hit)
        {
            if (bypass.Contains(victim.userID) || attacker == victim) return null;
            var victimClan = findClanByUser(victim.UserIDString);
            var attackerClan = findClanByUser(attacker.UserIDString);
            if (victimClan == null || attackerClan == null) return null;
            if (victimClan.tag == attackerClan.tag)
            {
                if (manuallyEnabledBy.Contains(attacker.userID) && !forceClanFFNoDeactivate) return null;
                DateTime now = DateTime.UtcNow;
                DateTime time;
                var key = attacker.UserIDString + "-" + victim.UserIDString;
                if (!notificationTimes.TryGetValue(key, out time) || time < now.AddSeconds(-friendlyFireNotifyTimeout))
                {
                    PrintChat(attacker, string.Format(msg("friendlyfire", attacker.UserIDString), victim.displayName, colorCmdUsage));
                    notificationTimes[key] = now;
                }
                hit.damageTypes = new DamageTypeList();
                hit.DidHit = false;
                hit.HitEntity = null;
                hit.Initiator = null;
                hit.DoHitEffects = false;
                return false;
            }
            if (victimClan.tag != attackerClan.tag && enableClanAllies && enableAllyFFOPtion)
            {
                if (!victimClan.clanAlliances.Contains(attackerClan.tag)) return null;
                if (manuallyEnabledBy.Contains(attacker.userID) && !forceAllyFFNoDeactivate) return null;
                DateTime now = DateTime.UtcNow;
                DateTime time;
                var key = attacker.UserIDString + "-" + victim.UserIDString;
                if (!notificationTimes.TryGetValue(key, out time) || time < now.AddSeconds(-friendlyFireNotifyTimeout))
                {
                    PrintChat(attacker, string.Format(msg("allyfriendlyfire", attacker.UserIDString), victim.displayName));
                    notificationTimes[key] = now;
                }
                hit.damageTypes = new DamageTypeList();
                hit.DidHit = false;
                hit.HitEntity = null;
                hit.Initiator = null;
                hit.DoHitEffects = false;
                return false;
            }
            return null;
        }
        void AllyRemovalCheck()
        {
            foreach (var ally in clans)
            {
                try
                {
                    Clan allyClan = clans[ally.Key];
                    foreach (var clanAlliance in allyClan.clanAlliances.ToList())
                    {
                        if (!clans.ContainsKey(clanAlliance)) allyClan.clanAlliances.Remove(clanAlliance);
                    }
                    foreach (var invitedAlly in allyClan.invitedAllies.ToList())
                    {
                        if (!clans.ContainsKey(invitedAlly)) allyClan.clanAlliances.Remove(invitedAlly);
                    }
                    foreach (var pendingInvite in allyClan.pendingInvites.ToList())
                    {
                        if (!clans.ContainsKey(pendingInvite)) allyClan.clanAlliances.Remove(pendingInvite);
                    }
                }
                catch
                {
                    PrintWarning("Ally removal check failed. Please contact the developer.");
                }
            }
        }
        void ccmdChatClan(ConsoleSystem.Arg arg)
        {
            if (arg != null && arg.Connection != null && arg.Connection.player != null)
            {
                usedConsoleInput.Add(arg.Connection.userid);
                if (arg.Args != null) cmdChatClan((BasePlayer)arg.Connection.player, chatCommandClan, arg.Args);
                else cmdChatClan((BasePlayer)arg.Connection.player, chatCommandClan, new string[] { });
            }
        }
        void cmdChatClan(BasePlayer player, string command, string[] args)
        {
            if (player == null) return;
            if (args == null || args.Length == 0)
            {
                cmdClanOverview(player);
                return;
            }
            string opt = args[0];
            if (opt == msg("clanArgCreate", player.UserIDString))
            {
                cmdClanCreate(player, args);
                return;
            }
            else if (opt == msg("clanArgInvite", player.UserIDString))
            {
                cmdClanInvite(player, args);
                return;
            }
            else if (opt == msg("clanArgWithdraw", player.UserIDString))
            {
                cmdClanWithdraw(player, args);
                return;
            }
            else if (opt == msg("clanArgJoin", player.UserIDString))
            {
                cmdClanJoin(player, args);
                return;
            }
            else if (opt == msg("clanArgPromote", player.UserIDString))
            {
                cmdClanPromote(player, args);
                return;
            }
            else if (opt == msg("clanArgDemote", player.UserIDString))
            {
                cmdClanDemote(player, args);
                return;
            }
            else if (opt == msg("clanArgLeave", player.UserIDString))
            {
                cmdClanLeave(player, args);
                return;
            }
            else if (opt == msg("clanArgFF", player.UserIDString))
            {
                if (!enableFFOPtion) return;
                cmdChatClanFF(player, command, args);
                return;
            }
            else if (opt == msg("clanArgAlly", player.UserIDString))
            {
                if (!enableClanAllies) return;
                for (var i = 0; i < args.Length - 1; ++i)
                {
                    if (i < args.Length) args[i] = args[i + 1];
                }
                Array.Resize(ref args, args.Length - 1);
                cmdChatClanAlly(player, command, args);
                return;
            }
            else if (opt == msg("clanArgKick", player.UserIDString))
            {
                cmdClanKick(player, args);
                return;
            }
            else if (opt == msg("clanArgDisband", player.UserIDString))
            {
                cmdClanDisband(player, args);
                return;
            }
            else cmdChatClanHelp(player, command, args);
        }
        void cmdClanOverview(BasePlayer player)
        {
            var current = player.IPlayer;
            var myClan = findClanByUser(current.Id);
            var sb = new StringBuilder();
            if (!usedConsoleInput.Contains(player.userID)) sb.AppendLine($"<size=18><color={pluginPrefixColor}>{this.Title}</color></size>{(pluginPrefixREBORNShow == true ? $"<size=14><color={pluginPrefixREBORNColor}>REBORN</color></size>" : "}")}");
            {

                if (myClan == null)
                {
                    sb.AppendLine(string.Format(msg("notmember", current.Id)));
                    sb.AppendLine(string.Format(msg("viewthehelp", current.Id), colorCmdUsage, $"{chatCommandClan + "help "} | /{chatCommandClan}"));
                    SendReply(player, $"<color={colorTextMsg}>{sb.ToString().TrimEnd()}</color>");
                    return;
                }
                if (myClan.IsOwner(current.Id)) sb.Append(string.Format(msg("youareownerof", current.Id)));
                else if (myClan.IsCouncil(current.Id)) sb.Append(string.Format(msg("youarecouncilof", current.Id)));
                else if (myClan.IsModerator(current.Id)) sb.Append(string.Format(msg("youaremodof", current.Id)));
                else sb.Append(string.Format(msg("youarememberof", current.Id)));
                sb.AppendLine($" <color={colorClanNamesOverview}>{myClan.tag}</color> ( {myClan.Online}/{myClan.Total} )");
                sb.Append(string.Format(msg("memberon", current.Id)));
                int n = 0;
                foreach (var memberId in myClan.members.ToList())
                {
                    var op = myClan.FindClanMember(memberId);
                    if (op != null && (op as RustPlayer).IsConnected)
                    {
                        var memberName = op.Name;
                        if (op.Name == current.Name) memberName = msg("yourname", current.Id);
                        if (n > 0) sb.Append(", ");
                        sb.Append(string.Format(msg("overviewnamecolor", current.Id), myClan.GetRoleColor(op.Id), memberName));
                        ++n;
                    }
                }
                if (n > 0) sb.AppendLine();
                if (myClan.Online < myClan.Total)
                {
                    sb.Append(string.Format(msg("memberoff", current.Id)));
                    n = 0;
                    foreach (var memberId in myClan.members.ToList())
                    {
                        var p = myClan.FindClanMember(memberId);
                        if (p != null && !(p as RustPlayer).IsConnected)
                        {
                            if (n > 0) sb.Append(", ");
                            sb.Append(string.Format(msg("overviewnamecolor", current.Id), myClan.GetRoleColor(p.Id), p.Name));
                            ++n;
                        }
                    }
                    if (n > 0) sb.AppendLine();
                }
                if (myClan.HasAnyRole(current.Id) && myClan.invites.Count() > 0)
                {
                    sb.Append(string.Format(msg("pendinvites", current.Id)));
                    int m = 0;
                    foreach (var inviteId in myClan.invites.ToList())
                    {
                        var p = myClan.FindInvitedIPlayer(inviteId.Key);
                        if (p != null)
                        {
                            var invitedPlayer = string.Empty;
                            if (m > 0) sb.Append(", ");
                            invitedPlayer = string.Format(msg("overviewnamecolor", current.Id), clanMemberColor, p.Name);
                            ++m;
                            sb.Append(invitedPlayer);
                        }
                    }
                    if (m > 0) sb.AppendLine();
                }
                if (enableClanAllies && myClan.clanAlliances.Count() > 0) sb.AppendLine(string.Format(msg("yourclanallies", current.Id)) + $" <color={colorClanNamesOverview}>" + string.Join(", ", myClan.clanAlliances.ToArray()) + "</color>");
                if (enableClanAllies && (myClan.invitedAllies.Count() > 0 || myClan.pendingInvites.Count() > 0) && (myClan.IsOwner(current.Id) || myClan.IsCouncil(current.Id)))
                {
                    if (myClan.invitedAllies.Count() > 0) sb.AppendLine(string.Format(msg("allyinvites", current.Id)) + $" <color={colorClanNamesOverview}>" + string.Join(", ", myClan.invitedAllies.ToArray()) + "</color> ");
                    if (myClan.pendingInvites.Count() > 0) sb.AppendLine(string.Format(msg("allypending", current.Id)) + $" <color={colorClanNamesOverview}>" + string.Join(", ", myClan.pendingInvites.ToArray()) + "</color> ");
                    if (myClan.pendingInvites.Count() == 0 && myClan.invitedAllies.Count() == 0) sb.AppendLine();
                }
                if (enableFFOPtion) sb.AppendLine(string.Format(msg("yourffstatus", current.Id)) + " " + (manuallyEnabledBy.Contains(player.userID) ? $"<color={colorClanFFOn}>ON</color>" : $"<color={colorClanFFOff}>OFF</color>") + $" ( <color={colorCmdUsage}>/{chatCommandFF}</color> )");
                sb.AppendLine(string.Format(msg("viewthehelp", current.Id), colorCmdUsage, $"{string.Concat(chatCommandClan, subCommandClanHelp)} | /{chatCommandClan}"));
                string openText = $"<color={colorTextMsg}>";
                string closeText = "</color>";
                string[] parts = sb.ToString().Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
                sb = new StringBuilder();
                foreach (var part in parts)
                {
                    if ((sb.ToString().TrimEnd().Length + part.Length + openText.Length + closeText.Length) > 1100)
                    {
                        ChatSwitch(player, openText + sb.ToString().TrimEnd() + closeText, usedConsoleInput.Contains(player.userID) ? true : false);
                        sb.Clear();
                    }
                    sb.AppendLine(part);
                }
                ChatSwitch(player, openText + sb.ToString().TrimEnd() + closeText);
            }
        }
            void cmdClanCreate(BasePlayer player, string[] args)
            {
                var current = player.IPlayer;
                var myClan = findClanByUser(current.Id);
                if (myClan != null)
                {
                    PrintChat(player, string.Format(msg("youalreadymember", current.Id)));
                    return;
                }
                if (usePermToCreateClan && !permission.UserHasPermission(current.Id, permissionToCreateClan))
                {
                    PrintChat(player, msg("nopermtocreate", current.Id));
                    return;
                }
                if (args.Length < 2)
                {
                    PrintChat(player, string.Format(msg("usagecreate", current.Id), colorCmdUsage));
                    return;
                }
                if (tagReExt.IsMatch(args[1]))
                {
                    PrintChat(player, string.Format(msg("hintchars", current.Id), allowedSpecialChars));
                    return;
                }
                if (args[1].Length < tagLengthMin || args[1].Length > tagLengthMax)
                {
                    PrintChat(player, string.Format(msg("hintlength", current.Id), tagLengthMin, tagLengthMax));
                    return;
                }
                if (args.Length > 2)
                {
                    args[2] = args[2].Trim();
                    if (args[2].Length < 2 || args[2].Length > 30)
                    {
                        PrintChat(player, string.Format(msg("providedesc", current.Id)));
                        return;
                    }
                }
                if (enableWordFilter && FilterText(args[1]))
                {
                    PrintChat(player, string.Format(msg("bannedwords", current.Id)));
                    return;
                }
                string[] clanKeys = clans.Keys.ToArray();
                clanKeys = clanKeys.Select(c => c.ToLower()).ToArray();
                if (clanKeys.Contains(args[1].ToLower()))
                {
                    PrintChat(player, string.Format(msg("tagblocked", current.Id)));
                    return;
                }
                myClan = Clan.Create(args[1], args.Length > 2 ? args[2] : string.Empty, current.Id);
                myClan.CreateTeam();
                SetupPlayer(player, current, clan: myClan);
                myClan.AddBasePlayer(player);
                myClan.OnTeamUpdate();
                if (usePermGroups && !permission.GroupExists(permGroupPrefix + myClan.tag)) permission.CreateGroup(permGroupPrefix + myClan.tag, "Clan " + myClan.tag, 0);
                if (usePermGroups && !permission.UserHasGroup(current.Id, permGroupPrefix + myClan.tag)) permission.AddUserGroup(current.Id, permGroupPrefix + myClan.tag);
                myClan.OnCreate();
                PrintChat(player, string.Format(msg("nownewowner", current.Id), myClan.tag, myClan.description) + "\n" + string.Format(msg("inviteplayers", current.Id), colorCmdUsage));
                if (logClanChanges) LogToFile("ClanChanges", $"{DateTime.Now.ToString()} - '{current.Name}' created the new clan [{myClan.tag}]", this);
            }
            public void InvitePlayer(BasePlayer player, string targetId) => cmdClanInvite(player, new string[] {"", targetId});
            void cmdClanInvite(BasePlayer player, string[] args)
            {
                var current = player.IPlayer;
                var myClan = findClanByUser(current.Id);
                if (myClan == null)
                {
                    PrintChat(player, string.Format(msg("notmember", current.Id)));
                    return;
                }
                if (args.Length < 2)
                {
                    PrintChat(player, string.Format(msg("usageinvite", current.Id), colorCmdUsage));
                    return;
                }
                if (!myClan.IsOwner(current.Id) && !myClan.IsCouncil(current.Id) && !myClan.IsModerator(current.Id))
                {
                    PrintChat(player, string.Format(msg("notmoderator", current.Id)));
                    return;
                }
                var invPlayer = myClan.FindServerIPlayer(args[1]);
                if (invPlayer == null)
                {
                    PrintChat(player, string.Format(msg("nosuchplayer", current.Id), args[1]));
                    return;
                }
                if (myClan.IsMember(invPlayer.Id))
                {
                    PrintChat(player, string.Format(msg("alreadymember", current.Id), invPlayer.Name));
                    return;
                }
                if (myClan.IsInvited(invPlayer.Id))
                {
                    PrintChat(player, string.Format(msg("alreadyinvited", current.Id), invPlayer.Name));
                    return;
                }
                if (findClanByUser(invPlayer.Id) != null)
                {
                    PrintChat(player, string.Format(msg("alreadyinclan", current.Id), invPlayer.Name));
                    return;
                }
                if (usePermToJoinClan && !permission.UserHasPermission(invPlayer.Id, permissionToJoinClan))
                {
                    PrintChat(player, string.Format(msg("nopermtojoinbyinvite", current.Id), invPlayer.Name));
                    return;
                }
                myClan.AddInvite(invPlayer);
                myClan.BroadcastLoc("invitebroadcast", myClan.GetColoredName(current.Id, current.Name), myClan.GetColoredName(invPlayer.Id, invPlayer.Name));
                if ((invPlayer as RustPlayer).IsConnected)
                {
                    var invited = RustCore.FindPlayerByIdString(invPlayer.Id);
                    if (invited != null) PrintChat(invited, string.Format(msg("claninvite", invPlayer.Id), myClan.tag, myClan.description, colorCmdUsage));
                }
                myClan.updated = UnixTimeStampUTC();
                if (logClanChanges) LogToFile("ClanChanges", $"{DateTime.Now.ToString()} - '{current.Name}' invited '{invPlayer.Name}' to [{myClan.tag}]", this);
            }
            public void WithdrawPlayer(BasePlayer player, string targetId) => cmdClanWithdraw(player, new string[] {"", targetId});
            void cmdClanWithdraw(BasePlayer player, string[] args)
            {
                var current = player.IPlayer;
                var myClan = findClanByUser(current.Id);
                if (myClan == null)
                {
                    PrintChat(player, string.Format(msg("notmember", current.Id)));
                    return;
                }
                if (args.Length < 2)
                {
                    PrintChat(player, string.Format(msg("usagewithdraw", current.Id), colorCmdUsage));
                    return;
                }
                if (!myClan.HasAnyRole(current.Id))
                {
                    PrintChat(player, string.Format(msg("notmoderator", current.Id)));
                    return;
                }
                var disinvPlayer = myClan.FindInvitedIPlayer(args[1]);
                if (disinvPlayer == null)
                {
                    PrintChat(player, string.Format(msg("notinvited", current.Id), args[1]));
                    return;
                }
                myClan.RemoveMember(disinvPlayer);
                myClan.BroadcastLoc("canceledinvite", myClan.GetColoredName(current.Id, current.Name), myClan.GetColoredName(disinvPlayer.Id, disinvPlayer.Name));
                myClan.updated = UnixTimeStampUTC();
            }
            void cmdClanJoin(BasePlayer player, string[] args)
            {
                var current = player.IPlayer;
                var myClan = findClanByUser(current.Id);
                if (myClan != null)
                {
                    PrintChat(player, string.Format(msg("youalreadymember", current.Id)));
                    return;
                }
                if (usePermToJoinClan && !permission.UserHasPermission(current.Id, permissionToJoinClan))
                {
                    PrintChat(player, msg("nopermtojoin", current.Id));
                    return;
                }
                if (args.Length != 2)
                {
                    PrintChat(player, string.Format(msg("usagejoin", current.Id), colorCmdUsage));
                    return;
                }
                myClan = findClan(args[1]);
                if (myClan == null || !myClan.IsInvited(current.Id))
                {
                    PrintChat(player, string.Format(msg("younotinvited", current.Id)));
                    return;
                }
                if (limitMembers >= 0 && myClan.Total >= limitMembers)
                {
                    PrintChat(player, string.Format(msg("reachedmaximum", current.Id)));
                    return;
                }
                myClan.AddMember(current);
                myClan.AddBasePlayer(player);
                SetupPlayer(player, current, clan: myClan);
                if (usePermGroups && !permission.UserHasGroup(current.Id, permGroupPrefix + myClan.tag)) permission.AddUserGroup(current.Id, permGroupPrefix + myClan.tag);
                myClan.BroadcastLoc("playerjoined", myClan.GetColoredName(current.Id, current.Name));
                myClan.OnUpdate();
                List<string> others = new List<string>(myClan.members.ToList());
                others.Remove(current.Id);
                Interface.Oxide.CallHook("OnClanMemberJoined", current.Id, others);
                if (logClanChanges) LogToFile("ClanChanges", $"{DateTime.Now.ToString()} - '{current.Name}' joined to [{myClan.tag}]", this);
            }
            public void PromotePlayer(BasePlayer player, string targetId) => cmdClanPromote(player, new string[] {"", targetId});
            void cmdClanPromote(BasePlayer player, string[] args)
            {
                var current = player.IPlayer;
                var myClan = findClanByUser(current.Id);
                if (myClan == null)
                {
                    PrintChat(player, string.Format(msg("notmember", current.Id)));
                    return;
                }
                if (!myClan.IsOwner(current.Id))
                {
                    PrintChat(player, string.Format(msg("needclanowner", current.Id)));
                    return;
                }
                if (args.Length != 2)
                {
                    PrintChat(player, string.Format(msg("usagepromote", current.Id), colorCmdUsage));
                    return;
                }
                var promotePlayer = myClan.FindClanMember(args[1]);
                if (promotePlayer == null)
                {
                    PrintChat(player, string.Format(msg("nosuchplayer", current.Id), args[1]));
                    return;
                }
                if (enableClanAllies && myClan.IsCouncil(promotePlayer.Id))
                {
                    PrintChat(player, string.Format(msg("alreadyacouncil", current.Id), promotePlayer.Name));
                    return;
                }
                if (enableClanAllies && myClan.council != null && myClan.IsModerator(promotePlayer.Id))
                {
                    PrintChat(player, string.Format(msg("alreadyacouncilset", current.Id), promotePlayer.Name));
                    return;
                }
                if (!enableClanAllies && myClan.IsModerator(promotePlayer.Id))
                {
                    PrintChat(player, string.Format(msg("alreadyamod", current.Id), promotePlayer.Name));
                    return;
                }
                if (!myClan.IsModerator(promotePlayer.Id) && limitModerators >= 0 && myClan.Mods >= limitModerators)
                {
                    PrintChat(player, string.Format(msg("maximummods", current.Id)));
                    return;
                }
                if (enableClanAllies && myClan.IsModerator(promotePlayer.Id))
                {
                    myClan.SetCouncil(promotePlayer.Id);
                    myClan.RemoveModerator(promotePlayer);
                    myClan.BroadcastLoc("playerpromotedcouncil", myClan.GetColoredName(current.Id, current.Name), myClan.GetColoredName(promotePlayer.Id, promotePlayer.Name));
                }
                else
                {
                    myClan.SetModerator(promotePlayer);
                    myClan.BroadcastLoc("playerpromoted", myClan.GetColoredName(current.Id, current.Name), myClan.GetColoredName(promotePlayer.Id, promotePlayer.Name));
                }
                myClan.OnUpdate();
                if (logClanChanges) LogToFile("ClanChanges", $"{DateTime.Now.ToString()} - '{current.Name}' promoted '{promotePlayer.Name}' to a {myClan.GetRoleString(promotePlayer.Id.ToString())} of [{myClan.tag}]", this);
            }
            public void DemotePlayer(BasePlayer player, string targetId) => cmdClanDemote(player, new string[] {"", targetId});
            void cmdClanDemote(BasePlayer player, string[] args)
            {
                var current = player.IPlayer;
                var myClan = findClanByUser(current.Id);
                if (myClan == null)
                {
                    PrintChat(player, string.Format(msg("notmember", current.Id)));
                    return;
                }
                if (!myClan.IsOwner(current.Id))
                {
                    PrintChat(player, string.Format(msg("needclanowner", current.Id)));
                    return;
                }
                if (args.Length < 2)
                {
                    PrintChat(player, string.Format(msg("usagedemote", current.Id), colorCmdUsage));
                    return;
                }
                var demotePlayer = myClan.FindClanMember(args[1]);
                if (demotePlayer == null)
                {
                    PrintChat(player, string.Format(msg("nosuchplayer", current.Id), args[1]));
                    return;
                }
                if (!myClan.IsModerator(demotePlayer.Id) && !myClan.IsCouncil(demotePlayer.Id))
                {
                    PrintChat(player, string.Format(msg("notpromoted", current.Id), demotePlayer.Name));
                    return;
                }
                if (enableClanAllies && myClan.IsCouncil(demotePlayer.Id))
                {
                    myClan.SetCouncil();
                    if (limitModerators >= 0 && myClan.Mods >= limitModerators) myClan.BroadcastLoc("playerdemoted", myClan.GetColoredName(current.Id, current.Name), myClan.GetColoredName(demotePlayer.Id, demotePlayer.Name));
                    else
                    {
                        myClan.SetModerator(demotePlayer);
                        myClan.BroadcastLoc("councildemoted", myClan.GetColoredName(current.Id, current.Name), myClan.GetColoredName(demotePlayer.Id, demotePlayer.Name));
                    }
                }
                else
                {
                    myClan.RemoveModerator(demotePlayer);
                    myClan.BroadcastLoc("playerdemoted", myClan.GetColoredName(current.Id, current.Name), myClan.GetColoredName(demotePlayer.Id, demotePlayer.Name));
                }
                myClan.OnUpdate();
                if (logClanChanges) LogToFile("ClanChanges", $"{DateTime.Now.ToString()} - '{current.Name}' demoted '{demotePlayer.Name}' to a {myClan.GetRoleString(demotePlayer.Id.ToString())} of [{myClan.tag}]", this);
            }
            public void LeaveClan(BasePlayer player) => cmdClanLeave(player, new string[] {"leave"});
            void cmdClanLeave(BasePlayer player, string[] args)
            {
                var current = player.IPlayer;
                var myClan = findClanByUser(current.Id);
                if (myClan == null)
                {
                    PrintChat(player, string.Format(msg("notmember", current.Id)));
                    return;
                }
                if (args.Length != 1)
                {
                    PrintChat(player, string.Format(msg("usageleave", current.Id), colorCmdUsage));
                    return;
                }
                myClan.RemoveMember(current);
                if (myClan.Total == 0) RemoveClan(myClan.tag);
                else myClan.ValidateOwner();
                SetupPlayer(player, current, true, oldTag: myClan.tag);
                if (usePermGroups && permission.UserHasGroup(current.Id, permGroupPrefix + myClan.tag)) permission.RemoveUserGroup(current.Id, permGroupPrefix + myClan.tag);
                PrintChat(player, string.Format(msg("youleft", current.Id)));
                if (myClan.Total > 0)
                {
                    myClan.OnUpdate();
                    myClan.BroadcastLoc("playerleft", myClan.GetColoredName(current.Id, current.Name));
                    Interface.Oxide.CallHook("OnClanMemberGone", current.Id, myClan.members.ToList());
                    if (logClanChanges) LogToFile("ClanChanges", $"{DateTime.Now.ToString()} - '{current.Name}' left the clan [{myClan.tag}]", this);
                }
                else
                {
                    if (logClanChanges) LogToFile("ClanChanges", $"{DateTime.Now.ToString()} - '{current.Name}' left as last member the clan [{myClan.tag}]", this);
                    AllyRemovalCheck();
                    myClan.OnDestroy();
                    myClan = null;
                }
            }
            public void KickPlayer(BasePlayer player, string targetId) => cmdClanKick(player, new string[] {"", targetId});
            void cmdClanKick(BasePlayer player, string[] args)
            {
                var current = player.IPlayer;
                var myClan = findClanByUser(current.Id);
                if (myClan == null)
                {
                    PrintChat(player, string.Format(msg("notmember", current.Id)));
                    return;
                }
                if (!myClan.HasAnyRole(current.Id))
                {
                    PrintChat(player, string.Format(msg("notmoderator", current.Id)));
                    return;
                }
                if (args.Length != 2)
                {
                    PrintChat(player, string.Format(msg("usagekick", current.Id), colorCmdUsage));
                    return;
                }
                var kickPlayer = myClan.FindClanMember(args[1]);
                if (kickPlayer == null) kickPlayer = myClan.FindInvitedIPlayer(args[1]);
                if (kickPlayer == null)
                {
                    PrintChat(player, string.Format(msg("nosuchplayer", current.Id), args[1]));
                    return;
                }
                if (myClan.IsOwner(kickPlayer.Id) || ((myClan.IsCouncil(kickPlayer.Id) || myClan.IsModerator(kickPlayer.Id)) && !myClan.IsOwner(current.Id)))
                {
                    PrintChat(player, string.Format(msg("modownercannotkicked", current.Id), kickPlayer.Name));
                    return;
                }
                myClan.RemoveMember(kickPlayer);
                myClan.ValidateOwner();
                var kickBasePlayer = RustCore.FindPlayerByIdString(kickPlayer.Id);
                if (kickBasePlayer != null)
                {
                    SetupPlayer(kickBasePlayer, kickPlayer, true, oldTag: myClan.tag);
                    PrintChat(kickBasePlayer, string.Format(msg("werekicked", kickPlayer.Id), myClan.GetColoredName(current.Id, current.Name)));
                }
                if (usePermGroups && permission.UserHasGroup(kickPlayer.Id, permGroupPrefix + myClan.tag)) permission.RemoveUserGroup(kickPlayer.Id, permGroupPrefix + myClan.tag);
                myClan.BroadcastLoc("waskicked", myClan.GetColoredName(current.Id, current.Name), myClan.GetColoredName(kickPlayer.Id, kickPlayer.Name));
                myClan.OnUpdate();
                Interface.Oxide.CallHook("OnClanMemberGone", kickPlayer.Id, myClan.members.ToList());
                if (logClanChanges) LogToFile("ClanChanges", $"{DateTime.Now.ToString()} - '{current.Name}' kicked '{kickPlayer.Name}' from [{myClan.tag}]", this);
            }
            public void DisbandClan(BasePlayer player) => cmdClanDisband(player, new string[] {"disband", "forever"});
            void cmdClanDisband(BasePlayer player, string[] args)
            {
                var current = player.IPlayer;
                var myClan = findClanByUser(current.Id);
                bool lastMember = false;
                if (myClan == null)
                {
                    PrintChat(player, string.Format(msg("notmember", current.Id)));
                    return;
                }
                if (!myClan.IsOwner(current.Id))
                {
                    PrintChat(player, string.Format(msg("needclanowner", current.Id)));
                    return;
                }
                if (args.Length != 2)
                {
                    PrintChat(player, string.Format(msg("usagedisband", current.Id), colorCmdUsage));
                    return;
                }
                if (myClan.Total == 1)
                {
                    lastMember = true;
                }
                RemoveClan(myClan.tag);
                foreach (var member in myClan.members.ToList())
                {
                    clanCache.Remove(member);
                    if (usePermGroups && permission.UserHasGroup((string)member, permGroupPrefix + myClan.tag)) permission.RemoveUserGroup((string)member, permGroupPrefix + myClan.tag);
                }
                myClan.BroadcastLoc("clandisbanded");
                setupPlayers(myClan.members.ToList(), true, tag: myClan.tag);
                foreach (var ally in clans)
                {
                    Clan allyClan = clans[ally.Key];
                    allyClan.clanAlliances.Remove(myClan.tag);
                    allyClan.invitedAllies.Remove(myClan.tag);
                    allyClan.pendingInvites.Remove(myClan.tag);
                }
                if (usePermGroups && permission.GroupExists(permGroupPrefix + myClan.tag)) permission.RemoveGroup(permGroupPrefix + myClan.tag);
                myClan.OnDestroy();
                AllyRemovalCheck();
                if (!lastMember) Interface.Oxide.CallHook("OnClanDisbanded", myClan.members.ToList());
                if (logClanChanges) LogToFile("ClanChanges", $"{DateTime.Now.ToString()} - '{current.Name}' disbanded [{myClan.tag}]", this);
                myClan.DisbandTeam();
            }
            public void Alliance(BasePlayer player, string targetClan, string type) => cmdChatClanAlly(player, "ally", new string[] {type, targetClan});
            void cmdChatClanAlly(BasePlayer player, string command, string[] args)
            {
                if (!enableClanAllies || player == null) return;
                var current = player.IPlayer;
                var myClan = findClanByUser(current.Id);
                if (myClan == null)
                {
                    PrintChat(player, string.Format(msg("notmember", current.Id)));
                    return;
                }
                if (!myClan.IsOwner(current.Id) && !myClan.IsCouncil(current.Id))
                {
                    PrintChat(player, string.Format(msg("needclanownercouncil", current.Id)));
                    return;
                }
                if (args == null || args.Length == 0)
                {
                    var sbally = new StringBuilder();
                    if (!usedConsoleInput.Contains(player.userID)) sbally.Append($"<size=18><color={pluginPrefixColor}>{this.Title}</color></size> {(pluginPrefixREBORNShow == true ? $"<size=14><color={pluginPrefixREBORNColor}>REBORN</color></size>" : "}")}");
                sbally.Append($"<color={colorTextMsg}>");
                    if (myClan.IsOwner(current.Id)) sbally.Append(string.Format(msg("youareownerof", current.Id)));
                    else if (myClan.IsCouncil(current.Id)) sbally.Append(string.Format(msg("youarecouncilof", current.Id)));
                    else if (myClan.IsModerator(current.Id)) sbally.Append(string.Format(msg("youaremodof", current.Id)));
                    else sbally.Append(string.Format(msg("youarememberof", current.Id)));
                    sbally.AppendLine($" <color={colorClanNamesOverview}>{myClan.tag}</color> ( {myClan.Online}/{myClan.Total} )");
                    if (myClan.clanAlliances.Count() > 0) sbally.AppendLine(string.Format(msg("yourclanallies", current.Id)) + $" <color={colorClanNamesOverview}>" + string.Join(", ", myClan.clanAlliances.ToArray()) + "</color>");
                    if ((myClan.invitedAllies.Count() > 0 || myClan.pendingInvites.Count() > 0) && (myClan.IsOwner(current.Id) || myClan.IsCouncil(current.Id)))
                    {
                        if (myClan.invitedAllies.Count() > 0) sbally.Append(string.Format(msg("allyinvites", current.Id)) + $" <color={colorClanNamesOverview}>" + string.Join(", ", myClan.invitedAllies.ToArray()) + "</color> ");
                        if (myClan.pendingInvites.Count() > 0) sbally.Append(string.Format(msg("allypending", current.Id)) + $" <color={colorClanNamesOverview}>" + string.Join(", ", myClan.pendingInvites.ToArray()) + "</color> ");
                        sbally.AppendLine();
                    }
                    string commandtext = string.Empty;
                    if (command.Contains("ally")) commandtext = command;
                    else commandtext = chatCommandClan + " ally";
                    sbally.AppendLine($"<color={colorCmdUsage}>/{commandtext} <{msg("allyArgRequest", current.Id)} | {msg("allyArgRequestShort", current.Id)}> <clantag></color>");
                    sbally.AppendLine(" " + msg("allyReqHelp", current.Id));
                    sbally.AppendLine($"<color={colorCmdUsage}>/{commandtext} <{msg("allyArgAccept", current.Id)} | {msg("allyArgAcceptShort", current.Id)}> <clantag></color>");
                    sbally.AppendLine(" " + msg("allyAccHelp", current.Id));
                    sbally.AppendLine($"<color={colorCmdUsage}>/{commandtext} <{msg("allyArgDecline", current.Id)} | {msg("allyArgDeclineShort", current.Id)}> <clantag></color>");
                    sbally.AppendLine(" " + msg("allyDecHelp", current.Id));
                    sbally.AppendLine($"<color={colorCmdUsage}>/{commandtext} <{msg("allyArgCancel", current.Id)} | {msg("allyArgCancelShort", current.Id)}> <clantag></color>");
                    sbally.AppendLine(" " + msg("allyCanHelp", current.Id));
                    sbally.Append("</color>");
                    SendReply(player, sbally.ToString().TrimEnd());
                    return;
                } else if (args != null && args.Length >= 1 && args.Length < 2) {
                    PrintChat(player, string.Format(msg("allyProvideName", current.Id)));
                    return;
                } else if (args.Length >= 1) {
                    Clan targetClan = null;
                    string opt = args[0];
                    if (opt == msg("allyArgRequest", current.Id) || opt == msg("allyArgRequestShort", current.Id)) {
                        if (limitAlliances != 0 && myClan.clanAlliances.Count >= limitAlliances) {
                            PrintChat(player, string.Format(msg("allyLimit", current.Id)));
                            return;
                        }
                        if (myClan.invitedAllies.Contains(args[1])) {
                            PrintChat(player, string.Format(msg("invitePending", current.Id), args[1]));
                            return;
                        }
                        if (myClan.clanAlliances.Contains(args[1])) {
                            PrintChat(player, string.Format(msg("alreadyAllies", current.Id)));
                            return;
                        }
                        targetClan = findClan(args[1]);
                        if (targetClan == null) {
                            PrintChat(player, string.Format(msg("clanNoExist", current.Id), args[1]));
                            return;
                        }
                        targetClan.pendingInvites.Add(myClan.tag);
                        myClan.invitedAllies.Add(targetClan.tag);
                        PrintChat(player, string.Format(msg("allyReq", current.Id), args[1]));
                        targetClan.AllyBroadcastLoc("reqAlliance", myClan.tag);
                        myClan.OnUpdate(false);
                        targetClan.OnUpdate(false);
                        return;
                    } else if (opt == msg("allyArgAccept", current.Id) || opt == msg("allyArgAcceptShort", current.Id)) {
                        if (!myClan.pendingInvites.Contains(args[1])) {
                            PrintChat(player, string.Format(msg("noAllyInv", current.Id), args[1]));
                            return;
                        }
                        targetClan = findClan(args[1]);
                        if (targetClan == null) {
                            PrintChat(player, string.Format(msg("clanNoExist", current.Id), args[1]));
                            return;
                        }
                        if (limitAlliances != 0 && myClan.clanAlliances.Count >= limitAlliances) {
                            PrintChat(player, string.Format(msg("allyAccLimit", current.Id), targetClan.tag));
                            targetClan.invitedAllies.Remove(myClan.tag);
                            myClan.pendingInvites.Remove(targetClan.tag);
                            return;
                        }
                        targetClan.invitedAllies.Remove(myClan.tag);
                        targetClan.clanAlliances.Add(myClan.tag);
                        myClan.pendingInvites.Remove(targetClan.tag);
                        myClan.clanAlliances.Add(targetClan.tag);
                        myClan.OnUpdate(false);
                        targetClan.OnUpdate(false);
                        PrintChat(player, string.Format(msg("allyAcc", current.Id), targetClan.tag));
                        targetClan.AllyBroadcastLoc("allyAccSucc", myClan.tag);
                        return;
                    } else if (opt == msg("allyArgDeclineallyArgDecline", current.Id) || opt == msg("allyArgDeclineShort", current.Id)) {
                        if (!myClan.pendingInvites.Contains(args[1])) {
                            PrintChat(player, string.Format(msg("noAllyInv", current.Id), args[1]));
                            return;
                        }
                        targetClan = findClan(args[1]);
                        if (targetClan == null) {
                            PrintChat(player, string.Format(msg("clanNoExist", current.Id), args[1]));
                            return;
                        }
                        targetClan.invitedAllies.Remove(myClan.tag);
                        myClan.pendingInvites.Remove(targetClan.tag);
                        AllyRemovalCheck();
                        PrintChat(player, string.Format(msg("allyDeclined", current.Id), args[1]));
                        myClan.OnUpdate(false);
                        targetClan.OnUpdate(false);
                        targetClan.AllyBroadcastLoc("allyDeclinedSucc", myClan.tag);
                        return;
                    } else if (opt == msg("allyArgCancel", current.Id) || opt == msg("allyArgCancelShort", current.Id)) {
                        if (!myClan.clanAlliances.Contains(args[1])) {
                            if (myClan.invitedAllies.Contains(args[1])) {
                                myClan.invitedAllies.Remove(args[1]);
                                targetClan = findClan(args[1]);
                                if (targetClan != null) targetClan.pendingInvites.Remove(myClan.tag);
                                PrintChat(player, string.Format(msg("allyInvWithdraw", current.Id), args[1]));
                                myClan.OnUpdate(false);
                                targetClan.OnUpdate(false);
                                return;
                            }
                            PrintChat(player, string.Format(msg("noAlly", current.Id)));
                            return;
                        }
                        targetClan = findClan(args[1]);
                        if (targetClan == null) {
                            PrintChat(player, string.Format(msg("clanNoExist", current.Id), args[1]));
                            return;
                        }
                        targetClan.clanAlliances.Remove(myClan.tag);
                        myClan.clanAlliances.Remove(targetClan.tag);
                        AllyRemovalCheck();
                        PrintChat(player, string.Format(msg("allyCancel", current.Id), args[1]));
                        myClan.OnUpdate(false);
                        targetClan.OnUpdate(false);
                        targetClan.AllyBroadcastLoc("allyCancelSucc", myClan.tag);
                        return;
                    } else cmdChatClanAlly(player, command, new string[] { });
                }
            }
            void cmdChatClanHelp(BasePlayer player, string command, string[] args)
            {
                if (player == null) return;
                var current = player.IPlayer;
                var myClan = findClanByUser(current.Id);
                var sb = new StringBuilder();
                if (myClan == null)
                {
                    sb.Append($"<color={colorTextMsg}>");
                    sb.AppendLine(msg("helpavailablecmds", current.Id));
                    sb.AppendLine($"<color={colorCmdUsage}>/{chatCommandClan} {msg("clanArgCreate", current.Id)} \"TAG\" \"Description\"</color> - {msg("helpcreate", current.Id)}");
                    sb.Append($"<color={colorCmdUsage}>/{chatCommandClan} {msg("clanArgJoin", current.Id)} \"TAG\"</color> - {msg("helpjoin", current.Id)}");
                    sb.Append("</color>");
                    SendReply(player, sb.ToString().TrimEnd());
                    return;
                }
                sb.AppendLine(msg("helpavailablecmds", current.Id));
                sb.AppendLine($"<color={colorCmdUsage}>/{chatCommandClan}</color> - {msg("helpinformation", current.Id)}");
                sb.AppendLine($"<color={colorCmdUsage}>/{chatCommandClanChat} <msg></color> - {msg("helpmessagemembers", current.Id)}");
                if (enableClanAllies) sb.AppendLine($"<color={colorCmdUsage}>/{chatCommandAllyChat} <msg></color> - {msg("helpmessageally", current.Id)}");
                sb.AppendLine($"<color={colorCmdUsage}>/{chatCommandClan} {msg("clanArgLeave", current.Id)}</color> - {msg("helpleave", current.Id)}");
                if (enableFFOPtion) sb.AppendLine($"<color={colorCmdUsage}>/{chatCommandClan} {msg("clanArgFF", current.Id)} |  /{chatCommandFF}</color> - {msg("helptoggleff", current.Id)}");
                if ((myClan.IsOwner(current.Id) || myClan.IsCouncil(current.Id) || myClan.IsModerator(current.Id)))
                {
                    sb.AppendLine($"<color={clanModeratorColor}>{msg("helpmoderator", current.Id)}</color> {msg("helpcommands", current.Id)}");
                    sb.AppendLine($"<color={colorCmdUsage}>/{chatCommandClan} {msg("clanArgInvite", current.Id)} {msg("clanArgNameId", current.Id)}</color> - {msg("helpinvite", current.Id)}");
                    sb.AppendLine($"<color={colorCmdUsage}>/{chatCommandClan} {msg("clanArgWithdraw", current.Id)} {msg("clanArgNameId", current.Id)}</color> - {msg("helpwithdraw", current.Id)}");
                    sb.AppendLine($"<color={colorCmdUsage}>/{chatCommandClan} {msg("clanArgKick", current.Id)} {msg("clanArgNameId", current.Id)}</color> - {msg("helpkick", current.Id)}");
                }
                if ((myClan.IsOwner(current.Id) || (enableClanAllies && myClan.IsCouncil(current.Id))))
                {
                    sb.AppendLine($"<color={clanOwnerColor}>{msg("helpowner", current.Id)}</color> {msg("helpcommands", current.Id)}");
                    if (enableClanAllies) sb.AppendLine($"<color={colorCmdUsage}>/{chatCommandClan} {msg("clanArgAlly", current.Id)} | {chatCommandClan + "ally "}</color> - {msg("helpallyoptions", current.Id)}");
                    if (myClan.IsOwner(current.Id)) sb.AppendLine($"<color={colorCmdUsage}>/{chatCommandClan} {msg("clanArgPromote", current.Id)} {msg("clanArgNameId", current.Id)}</color> - {msg("helppromote", current.Id)}");
                    if (myClan.IsOwner(current.Id)) sb.AppendLine($"<color={colorCmdUsage}>/{chatCommandClan} {msg("clanArgDemote", current.Id)} {msg("clanArgNameId", current.Id)}</color> - {msg("helpdemote", current.Id)}");
                    if (myClan.IsOwner(current.Id)) sb.AppendLine($"<color={colorCmdUsage}>/{chatCommandClan} {msg("clanArgDisband", current.Id)} {msg("clanArgForever", current.Id)}</color> - {msg("helpdisband", current.Id)}");
                }
                if (player.net.connection.authLevel >= authLevelDisband || player.net.connection.authLevel >= authLevelRename || player.net.connection.authLevel >= authLevelInvite || player.net.connection.authLevel >= authLevelKick || player.net.connection.authLevel >= authLevelPromoteDemote) sb.AppendLine($"<color={clanServerColor}>Server management</color>: {msg("helpconsole", current.Id)} <color={colorCmdUsage}>clans</color>");
                string openText = $"<color={colorTextMsg}>";
                string closeText = "</color>";
                string[] parts = sb.ToString().Split(new char[] {'\n'}, StringSplitOptions.RemoveEmptyEntries);
                sb = new StringBuilder();
                foreach (var part in parts)
                {
                    if ((sb.ToString().TrimEnd().Length + part.Length + openText.Length + closeText.Length) > 1100)
                    {
                        ChatSwitch(player, openText + sb.ToString().TrimEnd() + closeText, usedConsoleInput.Contains(player.userID) ? true : false);
                        sb.Clear();
                    }
                    sb.AppendLine(part);
                }
                ChatSwitch(player, openText + sb.ToString().TrimEnd() + closeText);
            }
            void cmdChatClanInfo(BasePlayer player, string command, string[] args)
            {
                if (player == null) return;
                if (player.net.connection.authLevel < authLevelClanInfo)
                {
                    PrintChat(player, "No access to this command.");
                    return;
                }
                if (args == null || args.Length == 0)
                {
                    PrintChat(player, "Please specify a clan tag.");
                    return;
                }
                var Clan = findClan(args[0]);
                if (Clan == null)
                {
                    PrintChat(player, string.Format(msg("clanNoExist", player.UserIDString), args[0]));
                    return;
                }
                var sb = new StringBuilder();
                if (!usedConsoleInput.Contains(player.userID)) sb.Append($"<size=18><color={pluginPrefixColor}>{this.Title}</color></size>{(pluginPrefixREBORNShow == true ? $"<size=14><color={pluginPrefixREBORNColor}>REBORN</color></size>" : "}")}");


            sb.AppendLine($"<color={colorTextMsg}>Detailed clan information for:");
                sb.AppendLine($"ClanTag:  <color={colorClanNamesOverview}>{Clan.tag}</color> ( Online: <color={colorClanNamesOverview}>{Clan.Online}</color> / Total: <color={colorClanNamesOverview}>{Clan.Total}</color> )");
                sb.AppendLine($"Description: <color={colorClanNamesOverview}>{Clan.description}</color>");
                sb.Append(string.Format(msg("memberon", player.UserIDString)));
                int n = 0;
                foreach (var memberId in Clan.members.ToList()) {
                    var op = Clan.FindClanMember(memberId);
                    if (op != null && (op as RustPlayer).IsConnected) {
                        if (n > 0) sb.Append(", ");
                        sb.Append(string.Format(msg("overviewnamecolor", player.UserIDString), Clan.GetRoleColor(op.Id), op.Name));
                        ++n;
                    }
                }
                if (Clan.Online == 0) sb.Append(" - ");
                sb.Append("</color>\n");
                bool offline = false;
                foreach (var memberId in Clan.members.ToList()) {
                    var op = Clan.FindClanMember(memberId);
                    if (op != null && !(op as RustPlayer).IsConnected) {
                        offline = true;
                        break;
                    }
                }
                if (offline) {
                    sb.Append(string.Format(msg("memberoff", player.UserIDString)));
                    n = 0;
                    foreach (var memberId in Clan.members.ToList()) {
                        var p = Clan.FindClanMember(memberId);
                        if (p != null && !(p as RustPlayer).IsConnected) {
                            if (n > 0) sb.Append(", ");
                            sb.Append(string.Format(msg("overviewnamecolor", player.UserIDString), Clan.GetRoleColor(p.Id), p.Name));
                            ++n;
                        }
                    }
                    sb.Append("\n");
                }
                sb.AppendLine($"Time created: <color={colorClanNamesOverview}>{UnixTimeStampToDateTime(Clan.created)}</color>");
                sb.AppendLine($"Last change: <color={colorClanNamesOverview}>{UnixTimeStampToDateTime(Clan.updated)}</color>");
                SendReply(player, sb.ToString().TrimEnd());
            }
            void cmdChatClanchat(BasePlayer player, string command, string[] args)
            {
                if (player == null || args.Length == 0) return;
                var myClan = findClanByUser(player.UserIDString);
                if (myClan == null)
                {
                    SendReply(player, string.Format(msg("notmember", player.UserIDString)));
                    return;
                }
                if (clanChatDenyOnMuted)
                {
                    var current = player.IPlayer;
                    var chk = Interface.CallHook("API_IsMuted", current);
                    if (chk != null && chk is bool && (bool)chk)
                    {
                        SendReply(player, string.Format(msg("clanchatmuted", player.UserIDString)));
                        return;
                    }
                }
                var message = string.Join(" ", args);
                if (string.IsNullOrEmpty(message)) return;
                myClan.BroadcastChat(string.Format(msg("broadcastformat"), myClan.GetRoleColor(player.UserIDString), player.net.connection.username, message));
                if (ConVar.Chat.serverlog) DebugEx.Log(string.Format("[CHAT] CLAN [{0}] - {1}: {2}", myClan.tag, player.net.connection.username, message), StackTraceLogType.None);
            }
            void cmdChatAllychat(BasePlayer player, string command, string[] args)
            {
                if (player == null || args.Length == 0) return;
                var myClan = findClanByUser(player.UserIDString);
                if (myClan == null)
                {
                    PrintChat(player, string.Format(msg("notmember", player.UserIDString)));
                    return;
                }
                if (myClan.clanAlliances.Count == 0)
                {
                    PrintChat(player, string.Format(msg("noactiveally", player.UserIDString)));
                    return;
                }
                if (clanChatDenyOnMuted)
                {
                    var current = player.IPlayer;
                    var chk = Interface.CallHook("API_IsMuted", current);
                    if (chk != null && chk is bool && (bool)chk)
                    {
                        SendReply(player, string.Format(msg("clanchatmuted", player.UserIDString)));
                        return;
                    }
                }
                var message = string.Join(" ", args);
                if (string.IsNullOrEmpty(message)) return;
                foreach (var clanAllyName in myClan.clanAlliances)
                {
                    var clanAlly = findClan(clanAllyName);
                    if (clanAlly == null) continue;
                    clanAlly.AllyBroadcastChat(string.Format(msg("allybroadcastformat"), myClan.tag, myClan.GetRoleColor(player.UserIDString), player.net.connection.username, message));
                }
                myClan.AllyBroadcastChat(string.Format(msg("broadcastformat"), myClan.GetRoleColor(player.UserIDString), player.net.connection.username, message));
                if (ConVar.Chat.serverlog) DebugEx.Log(string.Format("[CHAT] ALLY [{0}] - {1}: {2}", myClan.tag, player.net.connection.username, message), StackTraceLogType.None);
            }
            void cmdChatClanFF(BasePlayer player, string command, string[] args)
            {
                if (!enableFFOPtion || player == null) return;
                var myClan = findClanByUser(player.UserIDString);
                if (myClan == null)
                {
                    PrintChat(player, string.Format(msg("notmember", player.UserIDString)));
                    return;
                }
                if (manuallyEnabledBy.Contains(player.userID))
                {
                    manuallyEnabledBy.Remove(player.userID);
                    PrintChat(player, string.Format(msg("clanffdisabled", player.UserIDString), colorClanFFOff));
                    return;
                }
                else
                {
                    manuallyEnabledBy.Add(player.userID);
                    PrintChat(player, string.Format(msg("clanffenabled", player.UserIDString), colorClanFFOn));
                    return;
                }
            }
            public bool HasFFEnabled(ulong playerId) => !enableFFOPtion ? false : !manuallyEnabledBy.Contains(playerId) ? false : true;
            public void ToggleFF(ulong playerId)
            {
                if (manuallyEnabledBy.Contains(playerId)) manuallyEnabledBy.Remove(playerId);
                else manuallyEnabledBy.Add(playerId);
            }
        [ProtoContract(ImplicitFields = ImplicitFields.AllFields)]
        class StoredData
        {
            public Dictionary<string, Clan> clans = new Dictionary<string, Clan>();
            public Int32 saveStamp = 0;
            public string lastStorage = string.Empty;
            public StoredData() { }
        }
        StoredData clanSaves = new StoredData();
        [ProtoContract(ImplicitFields = ImplicitFields.AllFields)]
        public class Clan
        {
            public string tag;
            public string description;
            public string owner;
            public string council;
            public int created;
            public int updated;
            public List<string> moderators = new List<string>();
            public List<string> members = new List<string>();
            [JsonIgnore, ProtoIgnore] public List<BasePlayer> membersBasePlayer = new List<BasePlayer>();
            [JsonIgnore, ProtoIgnore] public List<IPlayer> membersIPlayer = new List<IPlayer>();
            public Dictionary<string, int> invites = new Dictionary<string, int>();
            public List<string> clanAlliances = new List<string>();
            public List<string> invitedAllies = new List<string>();
            public List<string> pendingInvites = new List<string>();
            [JsonIgnore, ProtoIgnore] public PlayerTeam playerTeam = null;
            [JsonIgnore, ProtoIgnore] Timer _uiUpdateTimer = null;
            [JsonIgnore, ProtoIgnore]
            public int Total
            {
                get
                {
                    return members.Count();
                }
            }
            [JsonIgnore, ProtoIgnore]
            public int Mods
            {
                get
                {
                    return moderators.Count();
                }
            }
            [JsonIgnore, ProtoIgnore] string currentTeamLeader = string.Empty;
            [JsonIgnore, ProtoIgnore] bool wasDisbanded = false;
            [JsonIgnore, ProtoIgnore]
            public int Online
            {
                get
                {
                    return membersIPlayer.Where(m => m != null && (m as RustPlayer).IsConnected).Count();
                }
            }
            public bool IsOwner(string userId) => userId == owner;
            public bool IsCouncil(string userId) => userId == council;
            public bool IsModerator(string userId) => moderators.Contains(userId);
            public bool IsMember(string userId) => members.Contains(userId);
            public bool IsInvited(string userId) => invites.ContainsKey(userId);
            public bool HasAnyRole(string userId) => IsOwner(userId) || IsCouncil(userId) || IsModerator(userId);
            public static Clan Create(string tag, string description, string ownerId)
            {
                var clan = new Clan()
                {
                    tag = tag,
                    description = description,
                    owner = ownerId,
                    created = UnixTimeStampUTC(),
                    updated = UnixTimeStampUTC()
                };
                clan.members.Add(ownerId);
                cc.clans.Add(tag, clan);
                cc.clanCache[ownerId] = clan;
                cc.clansSearch[tag.ToLower()] = tag;
                return clan;
            }
            public void SetOwner(object obj) => owner = GetObjectId(obj);
            public void SetCouncil(object obj = null) => council = (obj == null ? null : GetObjectId(obj));
            string GetObjectId(object obj)
            {
                if (obj is BasePlayer) return (obj as BasePlayer).UserIDString;
                else if (obj is IPlayer) return (obj as IPlayer).Id;
                return (string)obj;
            }
            public void AddMember(object obj)
            {
                RemoveMember(obj);
                string Id = GetObjectId(obj);
                members.Add(Id);
                if (obj is BasePlayer) membersBasePlayer.Add(obj as BasePlayer);
                if (obj is IPlayer) membersIPlayer.Add(obj as IPlayer);
                cc.clanCache[Id] = this;
            }
            public void RemoveMember(object obj)
            {
                RemoveInvite(obj);
                RemoveModerator(obj);
                string Id = GetObjectId(obj);
                members.Remove(Id);
                if (IsCouncil(Id)) council = null;
                membersIPlayer.RemoveAll((IPlayer x) => x.Id == Id);
                membersBasePlayer.RemoveAll((BasePlayer x) => x.UserIDString == Id);
                cc.clanCache.Remove(Id);
            }
            public void SetModerator(object obj)
            {
                RemoveModerator(obj);
                string Id = GetObjectId(obj);
                moderators.Add(Id);
            }
            public void RemoveModerator(object obj)
            {
                string Id = GetObjectId(obj);
                moderators.Remove(Id);
            }
            public void AddInvite(object obj)
            {
                RemoveInvite(obj);
                string Id = GetObjectId(obj);
                invites.Add(Id, UnixTimeStampUTC());
                if (!cc.pendingPlayerInvites.ContainsKey(Id)) cc.pendingPlayerInvites.Add(Id, new List<string>());
                cc.pendingPlayerInvites[Id].Add(tag);
            }
            public void RemoveInvite(object obj)
            {
                string Id = GetObjectId(obj);
                invites.Remove(Id);
                if (cc.pendingPlayerInvites.ContainsKey(Id)) cc.pendingPlayerInvites[Id].Remove(tag);
            }
            public void AddBasePlayer(BasePlayer basePlayer, bool flag = false)
            {
                if (!membersBasePlayer.Any((BasePlayer x) => x.UserIDString == basePlayer.UserIDString))
                {
                    membersBasePlayer.Add(basePlayer);
                    if (flag) OnTeamUpdate();
                }
                else
                {
                    membersBasePlayer.RemoveAll((BasePlayer x) => x.UserIDString == basePlayer.UserIDString);
                    membersBasePlayer.Add(basePlayer);
                }
            }
            public void RemoveBasePlayer(BasePlayer basePlayer, bool flag = false)
            {
                if (membersBasePlayer.Any((BasePlayer x) => x.UserIDString == basePlayer.UserIDString))
                {
                    membersBasePlayer.RemoveAll((BasePlayer x) => x.UserIDString == basePlayer.UserIDString);
                    if (flag) OnTeamUpdate();
                }
            }
            public BasePlayer GetBasePlayer(string Id)
            {
                BasePlayer lookup = membersBasePlayer.Find((BasePlayer x) => x.UserIDString == Id);
                if (lookup) return lookup;
                lookup = Oxide.Game.Rust.RustCore.FindPlayerByIdString(Id);
                if (lookup) AddBasePlayer(lookup);
                return lookup;
            }
            public void AddIPlayer(IPlayer iPlayer)
            {
                membersIPlayer.RemoveAll((IPlayer x) => x.Id == iPlayer.Id);
                membersIPlayer.Add(iPlayer);
            }
            public IPlayer GetIPlayer(string Id)
            {
                IPlayer lookup = membersIPlayer.Find((IPlayer x) => x.Id == Id);
                if (lookup != null) return lookup;
                lookup = cc.covalence.Players.FindPlayerById(Id);
                if (lookup != null) AddIPlayer(lookup);
                return lookup;
            }
            public bool ValidateOwner()
            {
                if (owner == null || owner == "0" || !members.Contains(owner))
                {
                    if (cc.enableClanAllies && council != null && council != "0")
                    {
                        owner = council;
                        council = null;
                        return true;
                    }
                    if (Mods > 0)
                    {
                        owner = moderators[0];
                        moderators.Remove(owner);
                        return true;
                    }
                    if (Total > 0)
                    {
                        owner = members[0];
                        return true;
                    }
                }
                return false;
            }
            public IPlayer FindClanMember(string nameOrId)
            {
                IPlayer result = membersIPlayer.Find((IPlayer x) => x.Id == nameOrId);
                if (result != null) return result;
                try
                {
                    var result2 = membersIPlayer.SingleOrDefault((IPlayer x) => x.Name.Equals(nameOrId, StringComparison.Ordinal) || x.Name.Equals(nameOrId, StringComparison.OrdinalIgnoreCase) || x.Name.Contains(nameOrId, StringComparison.OrdinalIgnoreCase) || x.Name.EndsWith(nameOrId, StringComparison.OrdinalIgnoreCase));
                    if (result2 != null && result2 is IPlayer) return result2;
                }
                catch { }
                return null;
            }
            public IPlayer FindServerIPlayer(string partialName)
            {
                IPlayer result = cc.covalence.Players.FindPlayerById(partialName);
                if (result != null) return result;
                BasePlayer lookup = Oxide.Game.Rust.RustCore.FindPlayer(partialName);
                if (lookup != null)
                {
                    if (lookup.IPlayer != null) return lookup.IPlayer;
                    return cc.covalence.Players.FindPlayerById(lookup.UserIDString);
                }
                try
                {
                    var mLookup = cc.covalence.Players.FindPlayer(partialName);
                    if (mLookup != null && mLookup is IPlayer) return mLookup;
                }
                catch { }
                return null;
            }
            public IPlayer FindInvitedIPlayer(string partialName)
            {
                foreach (var invited in invites.ToList())
                {
                    IPlayer player = cc.covalence.Players.FindPlayerById(invited.Key);
                    if (player != null)
                    {
                        if (partialName.Equals(player.Id, StringComparison.Ordinal) || partialName.Equals(player.Name, StringComparison.OrdinalIgnoreCase) || partialName.Equals(player.Name, StringComparison.OrdinalIgnoreCase) || player.Name.Contains(partialName, StringComparison.OrdinalIgnoreCase) || player.Name.EndsWith(partialName, StringComparison.OrdinalIgnoreCase))
                        {
                            return player;
                        }
                    }
                }
                return null;
            }
            public void BroadcastChat(string message)
            {
                foreach (var memberId in members)
                {
                    var player = BasePlayer.Find(memberId);
                    if (player == null) continue;
                    player.ChatMessage(string.Format(cc.broadcastPrefixFormat, cc.broadcastPrefixColor, cc.broadcastPrefix) + $"<color={cc.broadcastMessageColor}>{message}</color>");
                }
            }
            public void BroadcastLoc(string messagetype, string arg1 = "", string arg2 = "", string arg3 = "", string arg4 = "", string current = "")
            {
                string message = string.Empty;
                foreach (var memberId in members)
                {
                    var player = BasePlayer.Find(memberId);
                    if (player == null || player.UserIDString == current) continue;
                    message = string.Format(cc.msg(messagetype, memberId), arg1, arg2, arg3, arg4);
                    player.ChatMessage(string.Format(cc.broadcastPrefixFormat, cc.broadcastPrefixColor, cc.broadcastPrefix) + $"<color={cc.broadcastMessageColor}>{message}</color>");
                }
            }
            public void AllyBroadcastChat(string message)
            {
                foreach (var memberId in members)
                {
                    var player = BasePlayer.Find(memberId);
                    if (player == null) continue;
                    player.ChatMessage(string.Format(cc.broadcastPrefixFormat, cc.broadcastPrefixColor, cc.broadcastPrefixAlly) + $"<color={cc.broadcastMessageColor}>{message}</color>");
                }
            }
            public void AllyBroadcastLoc(string messagetype, string arg1 = "", string arg2 = "", string arg3 = "", string arg4 = "")
            {
                string message = string.Empty;
                foreach (var memberId in members)
                {
                    var player = BasePlayer.Find(memberId);
                    if (player == null) continue;
                    message = string.Format(cc.msg(messagetype, memberId), arg1, arg2, arg3, arg4);
                    player.ChatMessage(string.Format(cc.broadcastPrefixFormat, cc.broadcastPrefixColor, cc.broadcastPrefixAlly) + $"<color={cc.broadcastMessageColor}>{message}</color>");
                }
            }
            public string GetColoredName(string Id, string Name)
            {
                if (IsOwner(Id)) return $"<color={cc.clanOwnerColor}>{Name}</color>";
                else if (IsCouncil(Id) && !IsOwner(Id)) return $"<color={cc.clanCouncilColor}>{Name}</color>";
                else if (IsModerator(Id) && !IsOwner(Id)) return $"<color={cc.clanModeratorColor}>{Name}</color>";
                else return $"<color={cc.clanMemberColor}>{Name}</color>";
            }
            public string GetRoleString(string userID)
            {
                if (IsOwner(userID)) return "Owner";
                if (IsCouncil(userID)) return "Council";
                if (IsModerator(userID)) return "Moderator";
                return "Member";
            }
            public string GetRoleColor(string userID)
            {
                if (IsOwner(userID)) return cc.clanOwnerColor;
                if (IsCouncil(userID)) return cc.clanCouncilColor;
                if (IsModerator(userID)) return cc.clanModeratorColor;
                return cc.clanMemberColor;
            }
            internal JObject ToJObject()
            {
                var obj = new JObject();
                obj["tag"] = tag;
                obj["description"] = description;
                obj["owner"] = owner;
                obj["council"] = council;
                var jmoderators = new JArray();
                foreach (var moderator in moderators) jmoderators.Add(moderator);
                obj["moderators"] = jmoderators;
                var jmembers = new JArray();
                foreach (var member in members) jmembers.Add(member);
                obj["members"] = jmembers;
                var jallies = new JArray();
                foreach (var ally in clanAlliances) jallies.Add(ally);
                obj["allies"] = jallies;
                var jinvallies = new JArray();
                foreach (var ally in invitedAllies) jinvallies.Add(ally);
                obj["invitedallies"] = jinvallies;
                return obj;
            }
            internal void OnCreate() => Interface.CallHook("OnClanCreate", tag);
            internal void OnUpdate(bool hasChanges = true)
            {
                if (hasChanges)
                {
                    updated = UnixTimeStampUTC();
                    OnTeamUpdate();
                }
                Interface.CallHook("OnClanUpdate", tag);
            }
            internal void OnDestroy() => Interface.CallHook("OnClanDestroy", tag);
            public void OnTeamUpdate()
            {
                if (useRelationshipManager) SetTimer(1f, true);
            }
            void SetTimer(float f, bool destroy)
            {
                if (destroy)
                {
                    if (_uiUpdateTimer != null) _uiUpdateTimer.Destroy();
                    _uiUpdateTimer = cc.timer.Once(UnityEngine.Random.Range(f, f + 1.5f), SendTeamUpdate);
                }
                else
                {
                    if (_uiUpdateTimer == null) _uiUpdateTimer = cc.timer.Once(UnityEngine.Random.Range(f, f + 1.5f), SendTeamUpdate);
                }
            }
            public void DisbandTeam()
            {
                wasDisbanded = true;
                if (_uiUpdateTimer != null) _uiUpdateTimer.Destroy();
                playerTeam = null;
            }
            public void CreateTeam()
            {
                if (!useRelationshipManager || wasDisbanded) return;
                if (membersBasePlayer == null) membersBasePlayer = new List<BasePlayer>();
                playerTeam = new PlayerTeam();
                playerTeam.teamLeader = cc.disableManageFunctions || !cc.allowButtonKick ? 0uL : Convert.ToUInt64(owner);
                currentTeamLeader = owner;
                playerTeam.teamID = (ulong)created;
                playerTeam.teamName = $"[{tag}]";
                playerTeam.members = new List<PlayerTeam.TeamMember>();
            }
            void SendTeamUpdate()
            {
                if (wasDisbanded) return;
                if (playerTeam == null || currentTeamLeader != owner) CreateTeam();
                else playerTeam.members.Clear();
                if (Online > 0)
                {
                    foreach (var player in membersBasePlayer.ToList()) if (!IsMember(player.UserIDString)) membersBasePlayer.Remove(player);
                    foreach (string current in members.ToList())
                    {
                        IPlayer iPlayer = membersIPlayer.Find((IPlayer x) => x.Id == current);
                        if (iPlayer == null)
                        {
                            iPlayer = cc.covalence.Players.FindPlayerById(current);
                            AddIPlayer(iPlayer);
                        }
                        BasePlayer basePlayer = membersBasePlayer.Find((BasePlayer x) => x.UserIDString == current);
                        if (basePlayer == null && !cc.listDeadOfflineMembers && !HasAnyRole(current)) continue;
                        var teamMember = Pool.Get<PlayerTeam.TeamMember>();
                        teamMember.displayName = (cc.useRankColorsPanel ? GetColoredName(current, iPlayer.Name) : iPlayer.Name);
                        teamMember.healthFraction = ((!(basePlayer != null)) ? 0f : basePlayer.healthFraction);
                        teamMember.position = ((!(basePlayer != null)) ? Vector3.zero : basePlayer.transform.position);
                        teamMember.online = (basePlayer != null && !basePlayer.IsSleeping());
                        teamMember.userID = Convert.ToUInt64(current);
                        playerTeam.members.Add(teamMember);
                    }
                    playerTeam.members = playerTeam.members.OrderBy(m => IsOwner(m.userID.ToString())).ThenBy(m => IsCouncil(m.userID.ToString())).ThenBy(m => IsModerator(m.userID.ToString())).ThenBy(m => m.online).ThenBy(m => m.healthFraction).Reverse().ToList();
                    foreach (var player in membersBasePlayer.Where(p => p.IsConnected).ToList()) player.ClientRPCPlayer<PlayerTeam>(null, player, "CLIENT_ReceiveTeamInfo", playerTeam);
                    SetTimer(clientRefreshInterval, true);
                }
            }
        }
        void OnPlayerSleep(BasePlayer player)
        {
            if (useRelationshipManager) findClanByUser(player.UserIDString)?.OnTeamUpdate();
        }
        void OnPlayerSleepEnded(BasePlayer player)
        {
            if (useRelationshipManager)
            {
                Clan clan = findClanByUser(player.UserIDString);
                if (clan != null)
                {
                    TeamToClan(player, clan.created);
                    clan.OnTeamUpdate();
                }
            }
        }
        bool TeamToClan(BasePlayer player, int created)
        {
            bool flag = false;
            if (player.IsInvoking(player.TeamUpdate)) player.CancelInvoke(player.TeamUpdate);
            RelationshipManager.PlayerTeam team = RelationshipManager.Instance.FindTeam(player.currentTeam);
            if (team != null)
            {
                if (team.members.Contains(player.userID))
                {
                    team.members.Remove(player.userID);
                    if (team.teamLeader == player.userID)
                    {
                        if (team.members.Count > 0) team.SetTeamLeader(team.members[0]);
                        else team.Disband();
                    }
                    team.MarkDirty();
                    player.ClientRPCPlayer(null, player, "CLIENT_ClearTeam");
                }
            }
            if (player.currentTeam != (ulong)created)
            {
                player.currentTeam = (ulong)created;
                flag = true;
            }
            return flag;
        }
        void OnPlayerDie(BasePlayer player, HitInfo info) => findClanByUser(player.UserIDString)?.RemoveBasePlayer(player, true);
        void OnPlayerRespawned(BasePlayer player) => findClanByUser(player.UserIDString)?.AddBasePlayer(player, true);
        [HookMethod("GetClan")]
        private JObject GetClan(string tag)
        {
            if (tag != null)
            {
                var clan = findClan(tag);
                if (clan != null) return clan.ToJObject();
            }
            return null;
        }
        [HookMethod("GetAllClans")] private JArray GetAllClans() => new JArray(clans.Keys);
        [HookMethod("GetClanOf")]
        private string GetClanOf(ulong player)
        {
            if (player == 0uL) return null;
            var clan = findClanByUser(player.ToString());
            if (clan == null) return null;
            return clan.tag;
        }
        [HookMethod("GetClanOf")]
        private string GetClanOf(string player)
        {
            if (player == null || player == "") return null;
            var clan = findClanByUser(player.ToString());
            if (clan == null) return null;
            return clan.tag;
        }
        [HookMethod("GetClanOf")]
        private string GetClanOf(BasePlayer player)
        {
            if (player == null) return null;
            var clan = findClanByUser(player.UserIDString);
            if (clan == null) return null;
            return clan.tag;
        }
        [HookMethod("GetClanMembers")]
        private List<string> GetClanMembers(ulong PlayerID)
        {
            var myClan = findClanByUser(PlayerID.ToString());
            if (myClan == null) return null;
            return myClan.members.ToList();
        }
        [HookMethod("GetClanMembers")]
        private List<string> GetClanMembers(string PlayerID)
        {
            var myClan = findClanByUser(PlayerID);
            if (myClan == null) return null;
            return myClan.members.ToList();
        }
        [HookMethod("HasFriend")]
        private object HasFriend(ulong entOwnerID, ulong PlayerUserID)
        {
            var clanOwner = findClanByUser(entOwnerID.ToString());
            if (clanOwner == null) return null;
            var clanFriend = findClanByUser(PlayerUserID.ToString());
            if (clanFriend == null) return null;
            if (clanOwner.tag == clanFriend.tag) return true;
            return false;
        }
        [HookMethod("IsModerator")]
        private object IsModerator(ulong PlayerUserID)
        {
            var clan = findClanByUser(PlayerUserID.ToString());
            if (clan == null) return null;
            if ((setHomeOwner && clan.IsOwner(PlayerUserID.ToString())) || (setHomeModerator && (clan.IsModerator(PlayerUserID.ToString()) || clan.IsCouncil(PlayerUserID.ToString()))) || setHomeMember) return true;
            return false;
        }
        public static Int32 UnixTimeStampUTC() => (int)DateTime.UtcNow.Subtract(Epoch).TotalSeconds;
        static DateTime UnixTimeStampToDateTime(double unixTimeStamp) => unixTimeStamp > MaxUnixSeconds ? Epoch.AddMilliseconds(unixTimeStamp) : Epoch.AddSeconds(unixTimeStamp);
        string msg(string key, string id = null)
        {
            Puts(key);
            return MessagesPack[key];
        }

        void PrintChat(BasePlayer player, string message) => SendReply(player, string.Format(pluginPrefixFormat, pluginPrefixColor, pluginPrefix) + $"<color={colorTextMsg}>" + message + "</color>");
        [ConsoleCommand("clans")]
        void cclans(ConsoleSystem.Arg arg)
        {
            if (arg.Connection != null && arg.Connection.player != null && arg.Connection.authLevel > 0)
            {
                var sb = new StringBuilder();
                sb.AppendLine("\n> Clans command overview <");
                sb.AppendLine("clans.list (Lists all clans, their owners and their member-count)");
                sb.AppendLine("clans.listex (Lists all clans, their owners/members and their on-line status)");
                sb.AppendLine("clans.show \'tag|partialNameOrId\' (lists the chosen clan (or clan by user) and the members with status)");
                sb.AppendLine("clans.msg \'tag\' \'message without quotes\' (Sends a clan message)");
                if (arg.Connection.authLevel >= authLevelCreate) sb.AppendLine("clans.create \'tag(case-sensitive)\' \'steam-id(owner)\' \'desc(optional)\'");
                if (arg.Connection.authLevel >= authLevelRename) sb.AppendLine("clans.rename \'old tag\' \'new tag\' (renames a clan | case-sensitive)");
                if (arg.Connection.authLevel >= authLevelDisband) sb.AppendLine("clans.disband \'tag\' (disbands a clan)");
                if (arg.Connection.authLevel >= authLevelInvite)
                {
                    sb.AppendLine("clans.invite \'tag\' \'partialNameOrId\' (sends clan invitation to a player)");
                    sb.AppendLine("clans.join \'tag\' \'partialNameOrId\' (joins a player into a clan)");
                }
                if (arg.Connection.authLevel >= authLevelKick) sb.AppendLine("clans.kick \'tag\' \'partialNameOrId\' (kicks a member from a clan | deletes invite)");
                if (arg.Connection.authLevel >= authLevelPromoteDemote)
                {
                    sb.AppendLine("clans.owner \'tag\' \'partialNameOrId\' (sets a new owner)");
                    sb.AppendLine("clans.promote \'tag\' \'partialNameOrId\' (promotes a member)");
                    sb.AppendLine("clans.demote \'tag\' \'partialNameOrId\' (demotes a member)");
                }
                SendReply(arg, sb.ToString());
            }
        }
        [ConsoleCommand("clans.cmds")]
        void cclansCommands(ConsoleSystem.Arg arg)
        {
            if (arg.Connection != null && arg.Connection.authLevel < 1) return;
            var sb = new StringBuilder();
            sb.AppendLine("\n> Clans command overview <");
            TextTable textTable = new TextTable();
            textTable.AddColumn("Command");
            textTable.AddColumn("Description");
            textTable.AddRow(new string[] {
    "clans.list",
    "lists all clans, their owners and their member-count"
   });
            textTable.AddRow(new string[] {
    "clans.listex",
    "lists all clans, their owners/members and their on-line status"
   });
            textTable.AddRow(new string[] {
    "clans.show",
    "lists the chosen clan (or clan by user) and the members with status"
   });
            textTable.AddRow(new string[] {
    "clans.showduplicates",
    "lists the players which do exist in more than one clan"
   });
            textTable.AddRow(new string[] {
    "clans.msg",
    "message without quotes (Sends a clan message)"
   });
            textTable.AddRow(new string[] {
    "clans.create",
    "creates a clan"
   });
            textTable.AddRow(new string[] {
    "clans.rename",
    "renames a clan"
   });
            textTable.AddRow(new string[] {
    "clans.disband",
    "disbands a clan"
   });
            textTable.AddRow(new string[] {
    "clans.owner",
    "changes the owner to another member"
   });
            textTable.AddRow(new string[] {
    "clans.invite",
    "sends clan invitation to a player"
   });
            textTable.AddRow(new string[] {
    "clans.join",
    "joins a player into a clan"
   });
            textTable.AddRow(new string[] {
    "clans.kick",
    "kicks a player from a clan | deletes invite"
   });
            textTable.AddRow(new string[] {
    "clans.promote",
    "promotes a player"
   });
            textTable.AddRow(new string[] {
    "clans.demote",
    "demotes a player"
   });
            sb.AppendLine(textTable.ToString());
            SendReply(arg, sb.ToString());
        }
        [ConsoleCommand("clans.list")]
        void cclansList(ConsoleSystem.Arg arg)
        {
            if (arg.Connection != null && arg.Connection.authLevel < 1) return;
            TextTable textTable = new TextTable();
            textTable.AddColumn("Tag");
            textTable.AddColumn("Owner");
            textTable.AddColumn("SteamID");
            textTable.AddColumn("Count");
            textTable.AddColumn("On");
            foreach (var iclan in clans)
            {
                Clan clan = clans[iclan.Key];
                var owner = clan.FindClanMember(clan.owner);
                textTable.AddRow(new string[] {
     clan.tag, owner.Name, clan.owner, clan.Total.ToString(), clan.Online.ToString()
    });
            }
            SendReply(arg, "\n>> Current clans <<\n" + textTable.ToString());
        }
        [ConsoleCommand("clans.showduplicates")]
        void cclansDuplicates(ConsoleSystem.Arg arg)
        {
            if (arg.Connection != null && arg.Connection.authLevel < 1) return;
            TextTable textTable = new TextTable();
            textTable.AddColumn("SteamID");
            textTable.AddColumn("Memberships");
            textTable.AddColumn("PlayerName");
            Dictionary<string, List<string>> clanDuplicates = new Dictionary<string, List<string>>();
            foreach (var iclan in clans)
            {
                Clan clan = clans[iclan.Key];
                foreach (var member in clan.members.ToList())
                {
                    if (!clanDuplicates.ContainsKey(member))
                    {
                        clanDuplicates.Add(member, new List<string>());
                        clanDuplicates[member].Add(clan.tag);
                        continue;
                    }
                    else clanDuplicates[member].Add(clan.tag);
                }
            }
            foreach (var clDup in clanDuplicates)
            {
                if (clDup.Value.Count < 2) continue;
                var player = this.covalence.Players.FindPlayerById(clDup.Key);
                if (player == null) continue;
                textTable.AddRow(new string[] {
     clDup.Key, string.Join(" | ", clDup.Value.ToArray()), player.Name
    });
            }
            SendReply(arg, "\n>> Current found duplicates <<\n" + textTable.ToString());
        }
        [ConsoleCommand("clans.listex")]
        void cclansListEx(ConsoleSystem.Arg arg)
        {
            if (arg.Connection != null && arg.Connection.authLevel < 1) return;
            TextTable textTable = new TextTable();
            textTable.AddColumn("Tag");
            textTable.AddColumn("Level");
            textTable.AddColumn("Name");
            textTable.AddColumn("SteamID");
            textTable.AddColumn("Status");
            foreach (var iclan in clans)
            {
                Clan clan = clans[iclan.Key];
                foreach (var iMember in clan.membersIPlayer.ToList())
                {
                    var basePlayer = clan.GetBasePlayer(iMember.Id);
                    textTable.AddRow(new string[] {
      clan.tag, clan.GetRoleString(iMember.Id), iMember.Name, iMember.Id.ToString(), ((iMember as RustPlayer).IsConnected ? "Connected" : ((!(basePlayer != null)) ? "Not Alive" : "Offline")).ToString()
     });
                }
                textTable.AddRow(new string[] { });
            }
            SendReply(arg, "\n>> Current clans with members <<\n" + textTable.ToString());
        }
        [ConsoleCommand("clans.show")]
        void cclansShow(ConsoleSystem.Arg arg)
        {
            if (arg.Connection != null && arg.Connection.authLevel < 1) return;
            if (!arg.HasArgs(1))
            {
                SendReply(arg, "Usage: clans.show \'tag|partialNameOrId\'");
                return;
            }
            Clan clan;
            IPlayer checkPlayer = null;
            if (!TryGetClan(arg.Args[0], out clan))
            {
                checkPlayer = covalence.Players.FindPlayer(arg.Args[0]);
                if (checkPlayer == null)
                {
                    SendReply(arg, string.Format(msg("nosuchplayer"), arg.Args[0]));
                    SendReply(arg, string.Format(msg("noclanfound"), arg.Args[0]));
                    return;
                }
                clan = findClanByUser(checkPlayer.Id);
                if (clan == null)
                {
                    SendReply(arg, string.Format(msg("noclanfound"), arg.Args[0]));
                    return;
                }
            }
            var sb = new StringBuilder();
            if (checkPlayer == null) sb.AppendLine($"\n>> Show clan [{clan.tag}] <<");
            else sb.AppendLine($"\n>> Show clan [{clan.tag}] by '{checkPlayer.Name}' <<");
            sb.AppendLine($"Description: {clan.description}");
            sb.AppendLine($"Time created: {UnixTimeStampToDateTime(clan.created)}");
            sb.AppendLine($"Last updated: {UnixTimeStampToDateTime(clan.updated)}");
            sb.AppendLine($"Member count: {clan.Total}");
            TextTable textTable = new TextTable();
            textTable.AddColumn("Level");
            textTable.AddColumn("Name");
            textTable.AddColumn("SteamID");
            textTable.AddColumn("Status");
            sb.AppendLine();
            foreach (var iMember in clan.membersIPlayer.ToList())
            {
                var basePlayer = clan.GetBasePlayer(iMember.Id);
                textTable.AddRow(new string[] {
     clan.GetRoleString(iMember.Id), iMember.Name, iMember.Id.ToString(), ((iMember as RustPlayer).IsConnected ? "Connected" : ((!(basePlayer != null)) ? "Not Alive" : "Offline")).ToString()
    });
            }
            sb.AppendLine(textTable.ToString());
            SendReply(arg, sb.ToString());
        }
        [ConsoleCommand("clans.msg")]
        void cclansBroadcast(ConsoleSystem.Arg arg)
        {
            if (arg.Connection != null && arg.Connection.authLevel < 1) return;
            if (!arg.HasArgs(2))
            {
                SendReply(arg, "Usage: clans.msg \'tag\' \'your message without quotes\'");
                return;
            }
            Clan clan;
            if (!TryGetClan(arg.Args[0], out clan))
            {
                SendReply(arg, string.Format(msg("noclanfound"), arg.Args[0]));
                return;
            }
            string BroadcastBy = consoleName;
            if (arg.Connection != null)
            {
                if (arg.Connection.authLevel == 2) BroadcastBy = "(Admin) " + arg.Connection.username;
                else BroadcastBy = "(Mod) " + arg.Connection.username;
            }
            string Msg = "";
            for (int i = 1; i < arg.Args.Length; i++) Msg = Msg + " " + arg.Args[i];
            clan.BroadcastChat($"<color={clanServerColor}>{BroadcastBy}</color>: {Msg}");
            SendReply(arg, $"Broadcast to [{clan.tag}]: {Msg}");
        }
        [ConsoleCommand("clans.create")]
        void cclansClanCreate(ConsoleSystem.Arg arg)
        {
            if (arg.Connection != null && arg.Connection.authLevel < authLevelCreate) return;
            if (!arg.HasArgs(2))
            {
                SendReply(arg, "Usage: clans.create \'tag(case-sensitive)\' \'steamid(owner)\' \'desc(optional)\'");
                return;
            }
            Clan clan;
            if (TryGetClan(arg.Args[0], out clan))
            {
                SendReply(arg, string.Format(msg("tagblocked"), arg.Args[0]));
                return;
            }
            if (tagReExt.IsMatch(arg.Args[0]))
            {
                SendReply(arg, string.Format(msg("hintchars"), allowedSpecialChars));
                return;
            }
            if (arg.Args[0].Length < tagLengthMin || arg.Args[0].Length > tagLengthMax)
            {
                SendReply(arg, string.Format(msg("hintlength"), tagLengthMin, tagLengthMax));
                return;
            }
            var newOwner = covalence.Players.FindPlayerById(arg.Args[1]);
            if (newOwner == null)
            {
                SendReply(arg, string.Format(msg("nosuchplayer"), arg.Args[1]));
                return;
            }
            if (findClanByUser(newOwner.Id) != null)
            {
                SendReply(arg, string.Format(msg("alreadyinclan"), newOwner.Name));
                return;
            }
            clan = Clan.Create(arg.Args[0], arg.HasArgs(3) ? arg.Args[2] : string.Empty, newOwner.Id);
            clan.AddMember(newOwner);
            clan.AddIPlayer(newOwner);
            clan.CreateTeam();
            if (newOwner.IsConnected)
            {
                var owner = RustCore.FindPlayerByIdString(newOwner.Id);
                if (owner)
                {
                    clan.AddBasePlayer(owner);
                    SetupPlayer(owner, newOwner, clan: clan);
                    PrintChat(owner, string.Format(msg("nownewowner", newOwner.Id), clan.tag, clan.description) + "\n" + string.Format(msg("inviteplayers", newOwner.Id), colorCmdUsage));
                }
            }
            if (usePermGroups && !permission.GroupExists(permGroupPrefix + clan.tag)) permission.CreateGroup(permGroupPrefix + clan.tag, "Clan " + clan.tag, 0);
            if (usePermGroups && !permission.UserHasGroup(newOwner.Id, permGroupPrefix + clan.tag)) permission.AddUserGroup(newOwner.Id, permGroupPrefix + clan.tag);
            clan.OnCreate();
            clan.OnTeamUpdate();
            SendReply(arg, string.Format(msg("youcreated"), clan.tag));
            string CreatedBy = consoleName;
            if (arg.Connection != null) CreatedBy = arg.Connection.username;
            if (logClanChanges) LogToFile("ClanChanges", $"{DateTime.Now.ToString()} - Console: '{CreatedBy}' created the new clan [{clan.tag}] with '{newOwner.Name}' as owner", this);
        }
        [ConsoleCommand("clans.rename")]
        void cclansClanRename(ConsoleSystem.Arg arg)
        {
            if (arg.Connection != null && arg.Connection.authLevel < authLevelRename) return;
            if (!arg.HasArgs(2))
            {
                SendReply(arg, "Usage: clans.rename \'old tag\' \'new tag (case-sensitive)\'");
                return;
            }
            Clan clan;
            if (!TryGetClan(arg.Args[0], out clan))
            {
                SendReply(arg, string.Format(msg("noclanfound"), arg.Args[0]));
                return;
            }
            if (tagReExt.IsMatch(arg.Args[1]))
            {
                SendReply(arg, string.Format(msg("hintchars"), allowedSpecialChars));
                return;
            }
            if (arg.Args[1].Length < tagLengthMin || arg.Args[1].Length > tagLengthMax)
            {
                SendReply(arg, string.Format(msg("hintlength"), tagLengthMin, tagLengthMax));
                return;
            }
            if (clans.ContainsKey(arg.Args[1]))
            {
                SendReply(arg, string.Format(msg("tagblocked")));
                return;
            }
            string oldtag = clan.tag;
            clan.tag = arg.Args[1];
            clans.Add(clan.tag, clan);
            RemoveClan(oldtag);
            clansSearch[clan.tag.ToLower()] = clan.tag;
            setupPlayers(clan.members.ToList(), tag: oldtag);
            string oldGroup = permGroupPrefix + oldtag;
            string newGroup = permGroupPrefix + clan.tag;
            if (permission.GroupExists(oldGroup))
            {
                foreach (var member in clan.members.ToList()) if (permission.UserHasGroup(member, oldGroup)) permission.RemoveUserGroup(member, oldGroup);
                permission.RemoveGroup(oldGroup);
            }
            if (usePermGroups && !permission.GroupExists(newGroup)) permission.CreateGroup(newGroup, "Clan " + clan.tag, 0);
            foreach (var member in clan.members.ToList()) if (usePermGroups && !permission.UserHasGroup(member, newGroup)) permission.AddUserGroup(member, newGroup);
            string RenamedBy = consoleName;
            if (arg.Connection != null) RenamedBy = arg.Connection.username;
            foreach (var ally in clans)
            {
                Clan allyClan = clans[ally.Key];
                if (allyClan.clanAlliances.Contains(oldtag))
                {
                    allyClan.clanAlliances.Remove(oldtag);
                    allyClan.clanAlliances.Add(clan.tag);
                }
                if (allyClan.invitedAllies.Contains(oldtag))
                {
                    allyClan.invitedAllies.Remove(oldtag);
                    allyClan.invitedAllies.Add(clan.tag);
                }
                if (allyClan.pendingInvites.Contains(oldtag))
                {
                    allyClan.pendingInvites.Remove(oldtag);
                    allyClan.pendingInvites.Add(clan.tag);
                }
            }
            clan.BroadcastLoc("clanrenamed", $"<color={clanServerColor}>{RenamedBy}</color>", clan.tag);
            SendReply(arg, string.Format(msg("yourenamed"), oldtag, clan.tag));
            clan.OnUpdate(false);
            if (logClanChanges) LogToFile("ClanChanges", $"{DateTime.Now.ToString()} - Console: '{RenamedBy}' renamed '{oldtag}' to [{clan.tag}]", this);
        }
        [ConsoleCommand("clans.invite")]
        void cclansPlayerInvite(ConsoleSystem.Arg arg)
        {
            if (arg.Connection != null && arg.Connection.authLevel < authLevelInvite) return;
            if (!arg.HasArgs(2))
            {
                SendReply(arg, "Usage: clans.invite \'tag\' \'partialNameOrId\'");
                return;
            }
            Clan myClan;
            Clan check;
            if (!TryGetClan(arg.Args[0], out check))
            {
                SendReply(arg, string.Format(msg("noclanfound"), arg.Args[0]));
                return;
            }
            else myClan = (Clan)check;
            var invPlayer = myClan.FindServerIPlayer(arg.Args[1]);
            if (invPlayer == null)
            {
                SendReply(arg, string.Format(msg("nosuchplayer"), arg.Args[1]));
                return;
            }
            if (myClan.IsMember(invPlayer.Id))
            {
                SendReply(arg, string.Format(msg("alreadymember"), invPlayer.Name));
                return;
            }
            if (myClan.IsInvited(invPlayer.Id))
            {
                SendReply(arg, string.Format(msg("alreadyinvited"), invPlayer.Name));
                return;
            }
            if (findClanByUser(invPlayer.Id) != null)
            {
                SendReply(arg, string.Format(msg("alreadyinclan"), invPlayer.Name));
                return;
            }
            myClan.AddInvite(invPlayer);
            if (invPlayer.IsConnected)
            {
                var invited = RustCore.FindPlayerByIdString(invPlayer.Id);
                if (invited) PrintChat(invited, string.Format(msg("claninvite", invPlayer.Id), myClan.tag, myClan.description, colorCmdUsage));
            }
            myClan.updated = UnixTimeStampUTC();
            SendReply(arg, $"Invitation for clan '{myClan.tag}' sent to '{invPlayer.Name}'");
            string AddedBy = consoleName;
            if (arg.Connection != null) AddedBy = arg.Connection.username;
            if (logClanChanges) LogToFile("ClanChanges", $"{DateTime.Now.ToString()} - Console: '{AddedBy}' invited '{invPlayer.Name}' to [{myClan.tag}]", this);
        }
        [ConsoleCommand("clans.join")]
        void cclansPlayerJoin(ConsoleSystem.Arg arg)
        {
            if (arg.Connection != null && arg.Connection.authLevel < authLevelInvite) return;
            if (!arg.HasArgs(2))
            {
                SendReply(arg, "Usage: clans.join \'tag\' \'partialNameOrId\'");
                return;
            }
            Clan myClan;
            Clan check;
            if (!TryGetClan(arg.Args[0], out check))
            {
                SendReply(arg, string.Format(msg("noclanfound"), arg.Args[0]));
                return;
            }
            else myClan = (Clan)check;
            var joinPlayer = myClan.FindServerIPlayer(arg.Args[1]);
            if (joinPlayer == null)
            {
                SendReply(arg, string.Format(msg("nosuchplayer"), arg.Args[1]));
                return;
            }
            if (myClan.IsMember(joinPlayer.Id))
            {
                SendReply(arg, string.Format(msg("alreadymember"), joinPlayer.Name));
                return;
            }
            if (findClanByUser(joinPlayer.Id) != null)
            {
                SendReply(arg, string.Format(msg("alreadyinclan"), joinPlayer.Name));
                return;
            }
            myClan.AddMember(joinPlayer);
            myClan.AddIPlayer(joinPlayer);
            var joinBasePlayer = RustCore.FindPlayerByIdString(joinPlayer.Id);
            if (joinBasePlayer != null)
            {
                myClan.AddBasePlayer(joinBasePlayer);
                SetupPlayer(joinBasePlayer, joinPlayer, false, myClan);
            }
            if (usePermGroups && !permission.UserHasGroup(joinPlayer.Id, permGroupPrefix + myClan.tag)) permission.AddUserGroup(joinPlayer.Id, permGroupPrefix + myClan.tag);
            myClan.BroadcastLoc("playerjoined", myClan.GetColoredName(joinPlayer.Id, joinPlayer.Name));
            myClan.OnUpdate(true);
            List<string> others = new List<string>(myClan.members.ToList());
            others.Remove(joinPlayer.Id);
            Interface.Oxide.CallHook("OnClanMemberJoined", joinPlayer.Id, others);
            SendReply(arg, $"Playerjoin into clan '{myClan.tag}' done for '{joinPlayer.Name}'");
            string AddedBy = consoleName;
            if (arg.Connection != null) AddedBy = arg.Connection.username;
            if (logClanChanges) LogToFile("ClanChanges", $"{DateTime.Now.ToString()} - Console: '{AddedBy}' added '{joinPlayer.Name}' to [{myClan.tag}]", this);
        }
        [ConsoleCommand("clans.kick")]
        void cclansPlayerKick(ConsoleSystem.Arg arg)
        {
            if (arg.Connection != null && arg.Connection.authLevel < authLevelKick) return;
            if (!arg.HasArgs(2))
            {
                SendReply(arg, "Usage: clans.kick \'tag\' \'partialNameOrId\'");
                return;
            }
            Clan myClan;
            Clan check;
            if (!TryGetClan(arg.Args[0], out check))
            {
                SendReply(arg, string.Format(msg("noclanfound"), arg.Args[0]));
                return;
            }
            else myClan = (Clan)check;
            var kickPlayer = myClan.FindClanMember(arg.Args[1]);
            if (kickPlayer == null) kickPlayer = myClan.FindInvitedIPlayer(arg.Args[1]);
            if (kickPlayer == null)
            {
                SendReply(arg, string.Format(msg("nosuchplayer"), arg.Args[1]));
                return;
            }
            if (!myClan.IsMember(kickPlayer.Id) && !myClan.IsInvited(kickPlayer.Id))
            {
                SendReply(arg, string.Format(msg("notmembercannotkicked"), kickPlayer.Name));
                return;
            }
            if (myClan.Total == 1)
            {
                SendReply(arg, "The clan has only one member. You need to delete the clan");
                return;
            }
            bool wasMember = myClan.IsMember(kickPlayer.Id);
            myClan.RemoveMember(kickPlayer);
            bool ownerChanged = myClan.ValidateOwner();
            if (wasMember)
            {
                var kickBasePlayer = RustCore.FindPlayerByIdString(kickPlayer.Id);
                if (kickBasePlayer != null) SetupPlayer(kickBasePlayer, kickPlayer, true, oldTag: myClan.tag);
                if (usePermGroups && permission.UserHasGroup(kickPlayer.Id, permGroupPrefix + myClan.tag)) permission.RemoveUserGroup(kickPlayer.Id, permGroupPrefix + myClan.tag);
                myClan.OnUpdate(true);
                Interface.Oxide.CallHook("OnClanMemberGone", kickPlayer.Id, myClan.members.ToList());
            }
            SendReply(arg, $"Player '{kickPlayer.Name}' was {(wasMember ? "kicked " : "withdrawn ")} from clan '{myClan.tag}'");
            if (ownerChanged)
            {
                var newOwner = myClan.FindServerIPlayer(myClan.owner);
                if (newOwner != null) SendReply(arg, $"New owner of clan '{myClan.tag}' is {newOwner.Name}");
            }
            string KickedBy = consoleName;
            if (arg.Connection != null) KickedBy = arg.Connection.username;
            if (logClanChanges) LogToFile("ClanChanges", $"{DateTime.Now.ToString()} - Console: '{KickedBy}' kicked '{kickPlayer.Name}' from [{myClan.tag}]", this);
        }
        [ConsoleCommand("clans.owner")]
        void cclansClanOwner(ConsoleSystem.Arg arg)
        {
            if (arg.Connection != null && arg.Connection.authLevel < authLevelPromoteDemote) return;
            if (!arg.HasArgs(2))
            {
                SendReply(arg, "Usage: clans.owner \'tag\' \'partialNameOrId\'");
                return;
            }
            Clan myClan;
            Clan check;
            if (!TryGetClan(arg.Args[0], out check))
            {
                SendReply(arg, string.Format(msg("noclanfound"), arg.Args[0]));
                return;
            }
            else myClan = (Clan)check;
            IPlayer newOwner = myClan.FindClanMember(arg.Args[1]);
            if (newOwner == null)
            {
                SendReply(arg, string.Format(msg("nosuchplayer"), arg.Args[1]));
                return;
            }
            if (myClan.IsOwner(newOwner.Id))
            {
                SendReply(arg, string.Format(msg("alreadyowner"), newOwner.Name));
                return;
            }
            string AssignedBy = consoleName;
            if (arg.Connection != null) AssignedBy = arg.Connection.username;
            if (myClan.council == newOwner.Id) myClan.council = null;
            myClan.RemoveModerator(newOwner);
            myClan.owner = newOwner.Id;
            myClan.BroadcastLoc("playerpromotedowner", $"<color={clanServerColor}>{AssignedBy}</color>", myClan.GetColoredName(newOwner.Id, newOwner.Name));
            myClan.OnUpdate();
            myClan.OnTeamUpdate();
            SendReply(arg, $"You promoted '{newOwner.Name}' to the {myClan.GetRoleString(newOwner.Id.ToString())}");
            if (logClanChanges) LogToFile("ClanChanges", $"{DateTime.Now.ToString()} - Console: '{AssignedBy}' promoted '{newOwner.Name}' of [{myClan.tag}] to the clan owner", this);
        }
        [ConsoleCommand("clans.promote")]
        void cclansPlayerPromote(ConsoleSystem.Arg arg)
        {
            if (arg.Connection != null && arg.Connection.authLevel < authLevelPromoteDemote) return;
            if (!arg.HasArgs(2))
            {
                SendReply(arg, "Usage: clans.promote \'tag\' \'partialNameOrId\'");
                return;
            }
            Clan myClan;
            Clan check;
            if (!TryGetClan(arg.Args[0], out check))
            {
                SendReply(arg, string.Format(msg("noclanfound"), arg.Args[0]));
                return;
            }
            else myClan = (Clan)check;
            var promotePlayer = myClan.FindClanMember(arg.Args[1]);
            if (promotePlayer == null)
            {
                SendReply(arg, string.Format(msg("nosuchplayer"), arg.Args[1]));
                return;
            }
            if (myClan.IsOwner(promotePlayer.Id))
            {
                SendReply(arg, string.Format(msg("ownercannotbepromoted"), promotePlayer.Name));
                return;
            }
            if (enableClanAllies && myClan.IsCouncil(promotePlayer.Id))
            {
                SendReply(arg, string.Format(msg("alreadyacouncil"), promotePlayer.Name));
                return;
            }
            if (enableClanAllies && myClan.council != null && myClan.IsModerator(promotePlayer.Id))
            {
                SendReply(arg, string.Format(msg("alreadyacouncilset"), promotePlayer.Name));
                return;
            }
            if (!enableClanAllies && myClan.IsModerator(promotePlayer.Id))
            {
                SendReply(arg, string.Format(msg("alreadyamod"), promotePlayer.Name));
                return;
            }
            if (!myClan.IsModerator(promotePlayer.Id) && limitModerators >= 0 && myClan.Mods >= limitModerators)
            {
                SendReply(arg, string.Format(msg("maximummods")));
                return;
            }
            string PromotedBy = consoleName;
            if (arg.Connection != null) PromotedBy = arg.Connection.username;
            if (enableClanAllies && myClan.IsModerator(promotePlayer.Id))
            {
                myClan.SetCouncil(promotePlayer.Id);
                myClan.RemoveModerator(promotePlayer);
                myClan.BroadcastLoc("playerpromotedcouncil", $"<color={clanServerColor}>{PromotedBy}</color>", myClan.GetColoredName(promotePlayer.Id, promotePlayer.Name));
            }
            else
            {
                myClan.SetModerator(promotePlayer);
                myClan.BroadcastLoc("playerpromoted", $"<color={clanServerColor}>{PromotedBy}</color>", myClan.GetColoredName(promotePlayer.Id, promotePlayer.Name));
            }
            myClan.OnUpdate();
            SendReply(arg, $"You promoted '{promotePlayer.Name}' to a {myClan.GetRoleString(promotePlayer.Id.ToString())}");
            if (logClanChanges) LogToFile("ClanChanges", $"{DateTime.Now.ToString()} - Console: '{PromotedBy}' promoted '{promotePlayer.Name}' of [{myClan.tag}] to {myClan.GetRoleString(promotePlayer.Id.ToString())}", this);
        }
        [ConsoleCommand("clans.demote")]
        void cclansPlayerDemote(ConsoleSystem.Arg arg)
        {
            if (arg.Connection != null && arg.Connection.authLevel < authLevelPromoteDemote) return;
            if (!arg.HasArgs(2))
            {
                SendReply(arg, "Usage: clans.demote \'tag\' \'partialNameOrId\'");
                return;
            }
            Clan myClan = null;
            if (!TryGetClan(arg.Args[0], out myClan))
            {
                SendReply(arg, string.Format(msg("noclanfound"), arg.Args[0]));
                return;
            }
            var demotePlayer = myClan.FindClanMember(arg.Args[1]);
            if (demotePlayer == null)
            {
                SendReply(arg, string.Format(msg("nosuchplayer"), arg.Args[1]));
                return;
            }
            if (myClan.IsOwner(demotePlayer.Id))
            {
                SendReply(arg, string.Format(msg("ownercannotbedemoted"), demotePlayer.Name));
                return;
            }
            if (!myClan.IsModerator(demotePlayer.Id) && !myClan.IsCouncil(demotePlayer.Id))
            {
                SendReply(arg, string.Format(msg("notpromoted"), demotePlayer.Name));
                return;
            }
            string DemotedBy = consoleName;
            if (arg.Connection != null) DemotedBy = arg.Connection.username;
            if (enableClanAllies && myClan.IsCouncil(demotePlayer.Id))
            {
                myClan.SetCouncil();
                if (limitModerators >= 0 && myClan.Mods >= limitModerators) myClan.BroadcastLoc("playerdemoted", $"<color={clanServerColor}>{DemotedBy}</color>", myClan.GetColoredName(demotePlayer.Id, demotePlayer.Name));
                else
                {
                    myClan.SetModerator(demotePlayer);
                    myClan.BroadcastLoc("councildemoted", $"<color={clanServerColor}>{DemotedBy}</color>", myClan.GetColoredName(demotePlayer.Id, demotePlayer.Name));
                }
            }
            else
            {
                myClan.RemoveModerator(demotePlayer);
                myClan.BroadcastLoc("playerdemoted", $"<color={clanServerColor}>{DemotedBy}</color>", myClan.GetColoredName(demotePlayer.Id, demotePlayer.Name));
            }
            myClan.OnUpdate();
            SendReply(arg, $"You demoted '{demotePlayer.Name}' to a {myClan.GetRoleString(demotePlayer.Id.ToString())}");
            if (logClanChanges) LogToFile("ClanChanges", $"{DateTime.Now.ToString()} - Console: '{DemotedBy}' demoted '{demotePlayer.Name}' of [{myClan.tag}] to {myClan.GetRoleString(demotePlayer.Id.ToString())}", this);
        }
        [ConsoleCommand("clans.disband")]
        void cclansClanDisband(ConsoleSystem.Arg arg)
        {
            if (arg.Connection != null && arg.Connection.authLevel < authLevelDisband) return;
            if (!arg.HasArgs(2))
            {
                SendReply(arg, "Usage: clans.disband \'tag\' \'force|f|true\'");
                return;
            }
            Clan clan;
            if (!TryGetClan(arg.Args[0], out clan))
            {
                SendReply(arg, string.Format(msg("noclanfound"), arg.Args[0]));
                return;
            }
            string DeletedBy = arg.Connection == null ? consoleName : arg.Connection.username;
            clan.BroadcastLoc("clandeleted", $"<color={clanServerColor}>{DeletedBy}</color>");
            RemoveClan(clan.tag);
            foreach (var member in clan.members.ToList()) clanCache.Remove(member);
            setupPlayers(clan.members.ToList(), true, tag: clan.tag);
            string permGroup = permGroupPrefix + clan.tag;
            if (permission.GroupExists(permGroup))
            {
                foreach (var member in clan.members.ToList()) if (permission.UserHasGroup(member, permGroup)) permission.RemoveUserGroup(member, permGroup);
                permission.RemoveGroup(permGroup);
            }
            foreach (var ally in clans)
            {
                Clan allyClan = clans[ally.Key];
                allyClan.clanAlliances.Remove(arg.Args[0]);
                allyClan.invitedAllies.Remove(arg.Args[0]);
                allyClan.pendingInvites.Remove(arg.Args[0]);
            }
            SendReply(arg, string.Format(msg("youdeleted"), clan.tag));
            clan.OnDestroy();
            Interface.Oxide.CallHook("OnClanDisbanded", clan.members.ToList());
            AllyRemovalCheck();
            if (logClanChanges) LogToFile("ClanChanges", $"{DateTime.Now.ToString()} - Console: '{DeletedBy}' disbanded [{clan.tag}]", this);
            clan.DisbandTeam();
        }
        bool FilterText(string tag)
        {
            foreach (string bannedword in wordFilter) if (TranslateLeet(tag).ToLower().Contains(bannedword.ToLower())) return true;
            return false;
        }
        string TranslateLeet(string original)
        {
            string translated = original;
            Dictionary<string, string> leetTable = new Dictionary<string, string> {
    {
     "}{",
     "h"
    },
    {
     "|-|",
     "h"
    },
    {
     "]-[",
     "h"
    },
    {
     "/-/",
     "h"
    },
    {
     "|{",
     "k"
    },
    {
     "/\\/\\",
     "m"
    },
    {
     "|\\|",
     "n"
    },
    {
     "/\\/",
     "n"
    },
    {
     "()",
     "o"
    },
    {
     "[]",
     "o"
    },
    {
     "vv",
     "w"
    },
    {
     "\\/\\/",
     "w"
    },
    {
     "><",
     "x"
    },
    {
     "2",
     "z"
    },
    {
     "4",
     "a"
    },
    {
     "@",
     "a"
    },
    {
     "8",
     "b"
    },
    {
     "ß",
     "b"
    },
    {
     "(",
     "c"
    },
    {
     "<",
     "c"
    },
    {
     "{",
     "c"
    },
    {
     "3",
     "e"
    },
    {
     "€",
     "e"
    },
    {
     "6",
     "g"
    },
    {
     "9",
     "g"
    },
    {
     "&",
     "g"
    },
    {
     "#",
     "h"
    },
    {
     "$",
     "s"
    },
    {
     "7",
     "t"
    },
    {
     "|",
     "l"
    },
    {
     "1",
     "i"
    },
    {
     "!",
     "i"
    },
    {
     "0",
     "o"
    },
   };
            foreach (var leet in leetTable) translated = translated.Replace(leet.Key, leet.Value);
            return translated;
        }
        bool TryGetClan(string input, out Clan clan)
        {
            clan =
             default(Clan);
            if (clans.TryGetValue(input, out clan)) return true;
            if (clansSearch.TryGetValue(input.ToLower(), out input))
            {
                if (clans.TryGetValue(input, out clan)) return true;
            }
            return false;
        }
        void RemoveClan(string tag)
        {
            clans.Remove(tag);
            clansSearch.Remove(tag.ToLower());
        }
        [HookMethod("EnableBypass")]
        void EnableBypass(object userId)
        {
            if (!enableFFOPtion || userId == null) return;
            if (userId is string) userId = Convert.ToUInt64((string)userId);
            bypass.Add((ulong)userId);
        }
        [HookMethod("DisableBypass")]
        void DisableBypass(object userId)
        {
            if (!enableFFOPtion || userId == null) return;
            if (userId is string) userId = Convert.ToUInt64((string)userId);
            bypass.Remove((ulong)userId);
        }
        void OnPluginLoaded(Plugin plugin)
        {
            if (enableClanTagging && plugin.Title == "Better Chat") Interface.CallHook("API_RegisterThirdPartyTitle", this, new Func<IPlayer, string>(getFormattedClanTag));
        }
        string getFormattedClanTag(IPlayer player)
        {
            var clan = findClanByUser(player.Id);
            if (clan != null) return $"[#{clanTagColorBetterChat.Replace("#", "")}][+{clanTagSizeBetterChat}]{clanTagOpening}{clan.tag}{clanTagClosing}[/+][/#]";
            return string.Empty;
        }
        List<ulong> usedConsoleInput = new List<ulong>();
        void ChatSwitch(BasePlayer player, string message, bool keepConsole = false)
        {
            if (usedConsoleInput.Contains(player.userID)) player.ConsoleMessage(message);
            else SendReply(player, message);
            if (!keepConsole) usedConsoleInput.Remove(player.userID);
        }
    }
}