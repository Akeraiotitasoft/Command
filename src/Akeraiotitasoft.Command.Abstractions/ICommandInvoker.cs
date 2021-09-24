using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Akeraiotitasoft.Command.Abstractions
{
    public interface ICommandInvoker<TReceiver, TCommandId>
        where TReceiver : class
    {
        void Execute(TReceiver reciever, TCommandId commandId);

        IEnumerable<KeyValuePair<TCommandId, ICommand<TReceiver, TCommandId>>> GetCommandIds();

        bool IsValidOption(TCommandId commandId);
    }
}
