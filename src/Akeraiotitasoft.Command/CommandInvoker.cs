using Akeraiotitasoft.Command.Abstractions;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

#nullable enable

namespace Akeraiotitasoft.Command
{
    public class CommandInvoker<TReceiver, TCommandId> : ICommandInvoker<TReceiver, TCommandId>
        where TReceiver : class
    {
#pragma warning disable CS8714 // The type cannot be used as type parameter in the generic type or method. Nullability of type argument doesn't match 'notnull' constraint.
        private readonly Dictionary<TCommandId, ICommand<TReceiver, TCommandId>> _commands;
#pragma warning restore CS8714 // The type cannot be used as type parameter in the generic type or method. Nullability of type argument doesn't match 'notnull' constraint.
        private readonly ILogger<CommandInvoker<TReceiver, TCommandId>> _logger;

        protected virtual TCommandId? InvalidCommandId => default;

        public CommandInvoker(IEnumerable<ICommand<TReceiver, TCommandId>> commands, ILogger<CommandInvoker<TReceiver, TCommandId>> logger)
        {
            if (commands == null)
            {
                throw new ArgumentNullException(nameof(commands), "commands cannot be null");
            }
            _logger = logger ?? throw new ArgumentNullException(nameof(logger), "logger cannot be null");

            // search for duplicates
            List<TCommandId> duplicateCommandIds =
            (from command in commands
             group command by command.CommandId into commandNumbers
             where commandNumbers.Count() > 1
             select commandNumbers.Key).ToList();

            if (duplicateCommandIds.Count > 0)
            {
                throw new ArgumentException("The following CommandIds are duplicated: " + string.Join(",", duplicateCommandIds), nameof(commands));
            }

            // search for nulls
            bool nullCommandIdsExist =
            (from command in commands
             where command.CommandId == null
             select command
             ).Any();

            if (nullCommandIdsExist)
            {
                throw new ArgumentException("At least one command Id is null", nameof(commands));
            }

            if (InvalidCommandId != null)
            {
#pragma warning disable CS8602 // Dereference of a possibly null reference.
                if (!commands.Any(command => command.CommandId.Equals(InvalidCommandId)))
#pragma warning restore CS8602 // Dereference of a possibly null reference.
                {
                    throw new ArgumentException($"The commands parameter must contain a command that corresponds to the CommandId with the value {InvalidCommandId}", nameof(commands));
                }
            }

            try
            {
#pragma warning disable CS8714 // The type cannot be used as type parameter in the generic type or method. Nullability of type argument doesn't match 'notnull' constraint.
                _commands = new Dictionary<TCommandId, ICommand<TReceiver, TCommandId>>(commands.Select(command => new KeyValuePair<TCommandId, ICommand<TReceiver, TCommandId>>(command.CommandId, command)));
#pragma warning restore CS8714 // The type cannot be used as type parameter in the generic type or method. Nullability of type argument doesn't match 'notnull' constraint.
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in CommandInvoker constructor");
                throw;
            }
        }

        public void Execute(TReceiver reciever, TCommandId commandId)
        {
            try
            {
                _logger.LogInformation("CommandInvoker.Execute: trying CommandId " + commandId);
                if (_commands.ContainsKey(commandId))
                {
                    _logger.LogInformation("CommandInvoker.Execute: found option " + commandId);
                    _commands[commandId].Execute(reciever);
                    _logger.LogInformation($"CommandInvoker.Execute: option {commandId} completed execution");
                }
                else
                {
                    if (InvalidCommandId == null)
                    {
                        _logger.LogInformation("CommandInvoker.Execute: commandId not found");
                    }
                    else
                    {
                        _logger.LogInformation("CommandInvoker.Execute: commandId not found, executing invalid commandId");
                        _commands[InvalidCommandId].Execute(reciever);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Execute method");
                throw;
            }
        }

        /// <summary>
        /// Gets the commands as key value pairs of menu items
        /// </summary>
        /// <returns>An enumerable of key options and values of the menu text</returns>
        public IEnumerable<KeyValuePair<TCommandId, ICommand<TReceiver, TCommandId>>> GetCommandIds()
        {
            try
            {
                KeyValuePair<TCommandId, ICommand<TReceiver, TCommandId>>[] commands =
                    _commands
                    .Select(command => new KeyValuePair<TCommandId, ICommand<TReceiver, TCommandId>> (command.Key, command.Value))
                    .ToArray();
                Array.Sort(commands, (x, y) => Comparer<TCommandId>.Default.Compare(x.Key, y.Key));
                return commands;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetMenuOptions.");
                throw;
            }
        }

        /// <summary>
        /// Validates whether the selected commandId is valid
        /// </summary>
        /// <param name="option">The option number from the menu input</param>
        /// <returns>true to indicate that the option is valid, false otherwise</returns>
        public bool IsValidOption(TCommandId commandId)
        {
            if (commandId == null)
            {
                return false;
            }
            if (InvalidCommandId != null)
            {
                if (commandId.Equals(InvalidCommandId))
                {
                    return false;
                }
            }

            return _commands.ContainsKey(commandId);
        }
    }
}
