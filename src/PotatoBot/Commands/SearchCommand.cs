using PotatoBot.API;
using PotatoBot.Managers;
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
	[Command("search", Description = "Searched for new things to add")]
	internal class SearchCommand : Service, ICommand, IReplyCallback, IQueryCallback
	{
		public string UniqueIdentifier => "search";

		private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

		public async Task<bool> Execute(TelegramBotClient client, Message message, string[] arguments)
		{
			var keyboardMarkup = TelegramService.GetDefaultEntertainmentInlineKeyboardButtons();
			if(keyboardMarkup.Count == 0)
			{
				// Nothing enabled so nothing to search for
				await TelegramService.SimpleReplyToMessage(message, LanguageManager.GetTranslation("Commands", "Search", "NotEnabled"));
				return true;
			}

			var markup = new InlineKeyboardMarkup(keyboardMarkup);
			var title = LanguageManager.GetTranslation("Commands", "Search", "Start");
			await TelegramService.ReplyWithMarkupAndData(this, message, title, markup, new SearchData()
			{
				SelectedSearch = ServarrType.Unknown
			});
			return true;
		}

		public async Task<bool> OnCallbackQueryReceived(TelegramBotClient client, CallbackQuery callbackQuery)
		{
			var messageData = callbackQuery.Data;
			var message = callbackQuery.Message;
			var cacheData = TelegramService.GetCachedData<SearchData>(message);

			if(cacheData.SelectedSearch == ServarrType.Unknown)
			{
				return await HandleSearchSelection(client, message, messageData, cacheData);
			}
			return await HandleServiceSelection(message, messageData, cacheData);
		}

		private async Task<bool> HandleSearchSelection(TelegramBotClient client, Message message, string messageData, SearchData cacheData)
		{
			if(!Enum.TryParse<ServarrType>(messageData, out var selectedSearch))
			{
				_logger.Warn($"Failed to parse {messageData} to {nameof(ServarrType)}");
				return true;
			}

			cacheData.SelectedSearch = selectedSearch;
			_logger.Trace($"{message.From.Username} is searching for {selectedSearch}");

			var title = Program.LanguageManager.GetTranslation("Commands", "Search", "Selection");
			var keyboardMarkup = new List<List<InlineKeyboardButton>>();
			foreach(var service in Program.ServiceManager.GetAllServices().Where(s => s is APIBase apiBase && apiBase.Type == selectedSearch))
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

		private async Task<bool> HandleServiceSelection(Message message, string messageData, SearchData cacheData)
		{
			if(
				Program.ServiceManager.GetAllServices().FirstOrDefault(s =>
					 s is APIBase apiBase &&
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
			_logger.Trace($"{message.From.Username} is searching for {cacheData.SelectedSearch} with {service.Type}");

			var title = string.Format(
				Program.LanguageManager.GetTranslation("Commands", "Search", "Selected"),
				Program.LanguageManager.GetTranslation(cacheData.SelectedSearch.ToString()),
				(service as IService).Name
			);

			await TelegramService.ForceReply(this, message, title);

			return true;
		}

		public async Task<bool> OnReplyReceived(TelegramBotClient client, Message message)
		{
			var searchText = message.Text;
			var cacheData = TelegramService.GetCachedData<SearchData>(message);
			cacheData.SearchText = searchText;

			_logger.Trace($"Searching in {cacheData.API.Type} for '{searchText}'");

			StatisticsService.IncreaseSearches();

			await client.SendChatActionAsync(message.Chat.Id, Telegram.Bot.Types.Enums.ChatAction.Typing);

			cacheData.SearchResults = cacheData.API.Search(searchText);
			var resultCount = cacheData.SearchResults?.Count() ?? 0;
			_logger.Trace($"Found {resultCount} results for '{searchText}'");

			if(resultCount > 0)
			{
				cacheData.SearchResults = cacheData.SearchResults.OrderByDescending((r) => r.Year);
				var title = string.Format(
					Program.LanguageManager.GetTranslation("Commands", "Search", cacheData.SelectedSearch.ToString()),
					resultCount
				);

				await TelegramService.ReplyWithPageination(message, title, cacheData.SearchResults, OnPageinationSelection);
			}
			else
			{
				await TelegramService.SimpleReplyToMessage(message, string.Format(LanguageManager.GetTranslation("Commands", "Search", $"No{cacheData.SelectedSearch}"), searchText));
			}

			return true;
		}

		public async Task<bool> OnPageinationSelection(TelegramBotClient client, Message message, int selectedIndex)
		{
			var cacheData = TelegramService.GetCachedData<SearchData>(message);

			var selectedItem = TelegramService.GetPageinationResult(message, selectedIndex);
			if(selectedItem == null)
			{
				_logger.Warn($"Failed to find pageination result with index {selectedIndex}");
				await client.SendTextMessageAsync(message.Chat.Id, string.Format(LanguageManager.GetTranslation("Commands", "Search", "Fail"), cacheData.SearchText));
				return true;
			}

			var result = cacheData.API.Add(selectedItem);
			if(result.Added)
			{
				StatisticsService.IncreaseAdds();
				await client.SendTextMessageAsync(message.Chat.Id, string.Format(Program.LanguageManager.GetTranslation("Commands", "Search", "Success"), selectedItem.Title));
			}
			else if(result.AlreadyAdded)
			{
				await client.SendTextMessageAsync(message.Chat.Id, string.Format(Program.LanguageManager.GetTranslation("Commands", "Search", "Exists"), selectedItem.Title));
			}
			else
			{
				await client.SendTextMessageAsync(message.Chat.Id, string.Format(Program.LanguageManager.GetTranslation("Commands", "Search", "Fail"), selectedItem.Title));
			}

			return true;
		}
	}
}
