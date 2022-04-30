using Microsoft.Extensions.DependencyInjection;
using PotatoBot.Model;
using PotatoBot.Model.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace PotatoBot.Services
{
    public class CommandService
    {
        internal List<Command> Commands = new();

        private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        private readonly IServiceProvider _serviceProvider;
        private readonly StatisticsService _statisticsService;
        private readonly LanguageService _languageService;

        private readonly Dictionary<string, ICommand> _commands = new();
        private readonly Dictionary<string, IQueryCallback> _queryCommands = new();
        private readonly Dictionary<string, IReplyCallback> _replyCommands = new();

        public CommandService(IServiceProvider provider, StatisticsService statisticsService, LanguageService languageService)
        {
            _serviceProvider = provider;
            _statisticsService = statisticsService;
            _languageService = languageService;
        }

        public void LoadCommands()
        {
            _logger.Trace("CommandManager starting ...");

            var commandAttribute = typeof(Command);
            var commandInterface = typeof(ICommand);
            var queryInterface = typeof(IQueryCallback);
            var replyInterface = typeof(IReplyCallback);

            var assembly = Assembly.GetExecutingAssembly();

            foreach(var type in assembly.GetTypes())
            {
                try
                {
                    if(type.GetCustomAttribute(commandAttribute) is not Command attribute)
                    {
                        // Not a valid command
                        continue;
                    }

                    _logger.Trace($"Found command attribute on {type.Name}");

                    if(!commandInterface.IsAssignableFrom(type))
                    {
                        _logger.Warn($"Skipping {type.Name}: Not implementing required ICommand interface");
                        continue;
                    }

                    if(_commands.ContainsKey(attribute.Name))
                    {
                        _logger.Warn($"Skipping {type.Name}: Command '{attribute.Name}' already exists");
                        continue;
                    }

                    var commandName = attribute.Name;
                    if(ActivatorUtilities.CreateInstance(_serviceProvider, type) is not ICommand instance)
                    {
                        continue;
                    }

                    Commands.Add(attribute);

                    if(queryInterface.IsAssignableFrom(type))
                    {
                        _queryCommands.Add(commandName, (IQueryCallback)instance);
                    }

                    if(replyInterface.IsAssignableFrom(type))
                    {
                        var identifier = ((IReplyCallback)instance).UniqueIdentifier;
                        if(_replyCommands.ContainsKey(identifier))
                        {
                            _logger.Warn($"Skipping ForceReply implementation of {type.Name}: UniqueIdentifier '{identifier}' already exists");
                        }
                        else
                        {
                            _replyCommands.Add(identifier, (IReplyCallback)instance);
                        }
                    }

                    _commands.Add(commandName, instance);

                    _logger.Info($"Added Command '{commandName}'");
                }
                catch(Exception ex)
                {
                    _logger.Warn(ex, $"Failed to process type {type.Name}");
                }
            }

            _logger.Info($"CommandManager successfully loaded {_commands.Count} commands");
        }

        private static CommandParameters GetParameters(string commandLine)
        {
            var split = commandLine.Split(' ');
            if(commandLine.Contains('_'))
            {
                split = commandLine.Split('_');
            }

            var arguments = Array.Empty<string>();
            if(split.Length == 2)
            {
                // Only one argument supplied
                arguments = new string[] { split[1] };
            }
            else if(split.Length > 2)
            {
                arguments = split.Skip(1).ToArray();
            }
            return new CommandParameters()
            {
                // Remove leading '/' and normalize to lowercase
                CommandName = split[0].Substring(1, split[0].Length - 1).ToLower(),
                Arguments = arguments
            };
        }

        public async Task<bool> ProcessCommandMessage(TelegramBotClient client, Message message)
        {
            try
            {
                var text = message.Text;
                if(string.IsNullOrEmpty(text))
                {
                    return false;
                }

                var command = GetParameters(text);

                if(!_commands.ContainsKey(command.CommandName))
                {
                    // Command not found
                    await client.SendTextMessageAsync(message.Chat.Id, _languageService.GetTranslation("CommandNotFoundError"), replyToMessageId: message.MessageId);
                    return true;
                }

                var result = await _commands[command.CommandName].Execute(client, message, command.Arguments);
                if(!result)
                {
                    _logger.Warn($"Failed to execute command '{command}'");
                }
                else
                {
                    await _statisticsService.Increase(TrackedStatistics.CommandsProcessed);
                }
                return result;
            }
            catch(Exception ex)
            {
                _logger.Error(ex, "Failed to process message");
                await client.SendTextMessageAsync(message.Chat.Id, _languageService.GetTranslation("CommandProcessingError"), replyToMessageId: message.MessageId);
                return false;
            }
        }
    }
}
