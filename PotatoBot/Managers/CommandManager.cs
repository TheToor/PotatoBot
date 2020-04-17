using PotatoBot.Modals.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace PotatoBot.Managers
{
    internal class CommandManager
    {
        private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        private Dictionary<string, ICommand> _commands = new Dictionary<string, ICommand>();
        private Dictionary<string, IQueryCallback> _queryCommands = new Dictionary<string, IQueryCallback>();
        private Dictionary<string, IReplyCallback> _replyCommands = new Dictionary<string, IReplyCallback>();

        internal CommandManager()
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
                    var attribute = (Command)type.GetCustomAttribute(commandAttribute);
                    if (attribute == null)
                    {
                        // Not a valid command
                        continue;
                    }

                    _logger.Trace($"Found command attribute on {type.Name}");

                    if (!commandInterface.IsAssignableFrom(type))
                    {
                        _logger.Warn($"Skipping {type.Name}: Not implementing required ICommand interface");
                        continue;
                    }

                    if (_commands.ContainsKey(attribute.Name))
                    {
                        _logger.Warn($"Skipping {type.Name}: Command '{attribute.Name}' already exists");
                        continue;
                    }

                    var commandName = attribute.Name;
                    var instance = (ICommand)Activator.CreateInstance(type);

                    if (queryInterface.IsAssignableFrom(type))
                    {
                        _queryCommands.Add(commandName, (IQueryCallback)instance);
                    }

                    if (replyInterface.IsAssignableFrom(type))
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

        public async Task<bool> ProcessMessage(TelegramBotClient client, Message message)
        {
            try
            {
                var text = message.Text;
                if(string.IsNullOrEmpty(text))
                {
                    return false;
                }

                var split = text.Split(' ');
                var command = split[0];
                // Remove leading '/' and normalize to lowercase
                command = command.Substring(1, command.Length - 1).ToLower();

                var arguments = new string[0];
                if(split.Length == 2)
                {
                    // Only one argument supplied
                    arguments = new string[] { split[1] };
                }
                else if(split.Length > 2)
                {
                    arguments = split.Skip(1).ToArray();
                }

                if(!_commands.ContainsKey(command))
                {
                    // Command not found
                    return true;
                }

                var result = await _commands[command].Execute(client, message, arguments);
                if(!result)
                {
                    _logger.Warn($"Failed to execute command '{command}'");
                }
                return result;
            }
            catch(Exception ex)
            {
                _logger.Error(ex, "Failed to process message");
                return false;
            }
        }
    }
}
