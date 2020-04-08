using Newtonsoft.Json;
using Oxide.Core.Libraries.Covalence;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Oxide.Core;
using System;
using Oxide.Core.Configuration;
using System.Linq;


namespace Oxide.Plugins 
{
	[Info("chatCommands", "Lulex.py", "0.0.1")]
	public class chatCommands : RustPlugin 
	{	


		[ChatCommand("justdoit")]
		private void justStart (BasePlayer player, string command, string[] args) {
			SendReply(player, $"<color=#3999D5>Ну погнали епт.. твой айди: { player.displayName }</color>");

			rust.RunServerCommand($"oxide.usergroup remove '{ player.displayName }' duelist");
		}





		[ChatCommand("help")]
		private void TestCommand(BasePlayer player, string command, string[] args)
		{
			Message(player, $"<color=#3999D5>/info</color> -  Информация о сервере");
			Message(player, $"<color=#3999D5>/kit</color> -  Показать киты");
		    Message(player, $"<color=#3999D5>/marker</color> -  Настройка HitMarker");
		    Message(player, $"<color=#3999D5>/backpack</color> -  Открыть рюкзак");
		    Message(player, $"<color=#3999D5>/fmenu</color> -  Панель управления друзьями");
		    Message(player, $"<color=#3999D5>/remove</color> -  Команда удаления построек");
		    Message(player, $"<color=#3999D5>/grade 1-4</color> -  Автоулучшение построек");
		    Message(player, $"<color=#3999D5>/tpr</color> -  Команда телепортации к игроку");


		    Message(player, $"<color=#3999D5>/store</color> -  Внутриигровой магазин");
            Message(player, $"<color=#3999D5>/case</color> -  Ежедневные награды");			
		    Message(player, $"<color=#3999D5>/top</color> -  Показать топ 10");	   
			Message(player, $"<color=#3999D5>/trade</color> -  Команда обмена лутом между игроками");	   

		    Message(player, $"<color=#3999D5>/ad</color> -  Команда отключения автозакрытия дверей");	   
		    Message(player, $"<color=#3999D5>/ad [5-30]</color> -  Команда включения автозакрытия дверей спустя [5-30] секунд");	 
			Message(player, $"<color=#3999D5>/al or autolock</color> -  Команда автоматической установки пароля на дверь");	 
			Message(player, $"<color=#3999D5>/map</color> -  Открыть карту");	 
			Message(player, $"<color=#3999D5>/pm [reciever]</color> -  Отправить приватное сообщение [получатель]");
			Message(player, $"<color=#3999D5>/duel</color> -  Команда создания дуэлей");

			// SendReply(player, $"<color=#3999D5>Ну погнали епт.. твой айди: { player.displayName }</color>");	
			// rust.RunServerCommand($"oxide.usergroup add '{ player.displayName }' duelist");
		}

		private void Message(BasePlayer player, string message)
        {
            Player.Message(player, message, 0);
        }

	}
}


// rh start - rad housew
// chat
// info
