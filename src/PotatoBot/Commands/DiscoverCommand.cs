using PotatoBot.Modals;
using PotatoBot.Modals.Commands;
using PotatoBot.Modals.Commands.Data;
using PotatoBot.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace PotatoBot.Commands
{
	[Command("discover", Description = "Discover new things")]
	internal class DiscoverCommand : Service, ICommand, IQueryCallback
	{
		public string UniqueIdentifier => "discover";

		private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

		public async Task<bool> Execute(TelegramBotClient client, Message message, string[] arguments)
		{
			var keyboardMarkup = TelegramService.GetDefaultEntertainmentInlineKeyboardButtons(true);
			if(keyboardMarkup.Count == 0)
			{
				// Nothing enabled or supported so nothing to search for
				await TelegramService.SimpleReplyToMessage(message, LanguageManager.GetTranslation("Commands", "Discover", "NotEnabled"));
				return true;
			}

			var markup = new InlineKeyboardMarkup(keyboardMarkup);
			var title = LanguageManager.GetTranslation("Commands", "Discover", "Start");
			await TelegramService.ReplyWithMarkupAndData(this, message, title, markup, new DiscoveryData()
			{
				SelectedSearch = ServarrType.Unknown
			});
			return true;
		}

		public async Task<bool> OnCallbackQueryReceived(TelegramBotClient client, CallbackQuery callbackQuery)
		{
			var messageData = callbackQuery.Data;
			var message = callbackQuery.Message;
			var cacheData = TelegramService.GetCachedData<DiscoveryData>(message);

			if(cacheData.SelectedSearch == ServarrType.Unknown)
			{
				return await HandleSearchSelection(client, message, messageData, cacheData);
			}
			return await HandleServiceSelection(client, message, messageData, cacheData);
		}

		private async Task<bool> HandleSearchSelection(TelegramBotClient client, Message message, string messageData, DiscoveryData cacheData)
		{
			if(!Enum.TryParse<ServarrType>(messageData, out var selectedSearch))
			{
				_logger.Warn($"Failed to parse {messageData} to {nameof(ServarrType)}");
				return true;
			}

			cacheData.SelectedSearch = selectedSearch;
			_logger.Trace($"{message.From.Username} is discovering {selectedSearch}");

			var title = Program.LanguageManager.GetTranslation("Commands", "Discover", "Selection");
			var keyboardMarkup = new List<List<InlineKeyboardButton>>();
			foreach(var service in Program.ServiceManager.GetAllServices().Where(s => s is IServarr apiBase && apiBase.Type == selectedSearch))
			{
				keyboardMarkup.Add(new List<InlineKeyboardButton>()
				{
					InlineKeyboardButton.WithCallbackData(service.Name, service.Name)
				});
			}

			var markup = new InlineKeyboardMarkup(keyboardMarkup);

			await TelegramService.ReplyWithMarkupAndData(this, message, title, markup, cacheData);

			return true;
		}

		private async Task<bool> HandleServiceSelection(TelegramBotClient client, Message message, string messageData, DiscoveryData cacheData)
		{
			if(
				Program.ServiceManager.GetAllServices().FirstOrDefault(s =>
					 s is IServarr apiBase &&
					 apiBase.Type == cacheData.SelectedSearch &&
					 apiBase.Name == messageData
				)
				is not IServarr service
			)
			{
				_logger.Warn($"Failed to find service of type {cacheData.SelectedSearch} with name {messageData}");
				return false;
			}

			cacheData.API = service;
			_logger.Trace($"{message.From.Username} is discovery in {cacheData.SelectedSearch} with {service.Type}");

			await client.SendChatActionAsync(message.Chat.Id, Telegram.Bot.Types.Enums.ChatAction.Typing);
			await client.DeleteMessageAsync(message.Chat.Id, message.MessageId);

			cacheData.SearchResults = (cacheData.API as IServarrSupportsDiscovery).GetDiscoveryQueue();
			var resultCount = cacheData.SearchResults?.Count() ?? 0;
			_logger.Trace($"Found {resultCount} results in discovery queue");

			if(resultCount > 0)
			{
				//cacheData.SearchResults = cacheData.SearchResults.OrderByDescending((r) => r.Year);
				var title = string.Format(
					Program.LanguageManager.GetTranslation("Commands", "Discover", "Results"),
					resultCount
				);

				await TelegramService.ReplyWithPageination(message, title, cacheData.SearchResults, OnPageinationSelection);
			}
			else
			{
				await TelegramService.SimpleReplyToMessage(message, LanguageManager.GetTranslation("Commands", "Discover", $"NoResults"));
			}

			return true;
		}

		public async Task<bool> OnPageinationSelection(TelegramBotClient client, Message message, int selectedIndex)
		{
			var cacheData = TelegramService.GetCachedData<DiscoveryData>(message);

			var selectedItem = TelegramService.GetPageinationResult(message, selectedIndex);
			if(selectedItem == null)
			{
				_logger.Warn($"Failed to find pageination result with index {selectedIndex}");
				await client.SendTextMessageAsync(message.Chat.Id, string.Format(LanguageManager.GetTranslation("Commands", "Discover", "Fail"), "UNKNOWN"));
				return true;
			}

			var result = cacheData.API.Add(selectedItem);
			if(result.Added)
			{
				StatisticsService.IncreaseAdds();
				await client.SendTextMessageAsync(message.Chat.Id, string.Format(Program.LanguageManager.GetTranslation("Commands", "Discover", "Success"), selectedItem.Title));
			}
			else if(result.AlreadyAdded)
			{
				await client.SendTextMessageAsync(message.Chat.Id, string.Format(Program.LanguageManager.GetTranslation("Commands", "Discover", "Exists"), selectedItem.Title));
			}
			else
			{
				await client.SendTextMessageAsync(message.Chat.Id, string.Format(Program.LanguageManager.GetTranslation("Commands", "Discover", "Fail"), selectedItem.Title));
			}

			return true;
		}
	}
}
