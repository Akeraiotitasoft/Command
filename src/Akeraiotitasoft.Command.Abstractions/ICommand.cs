using System;

namespace Akeraiotitasoft.Command.Abstractions
{
    public interface ICommand<TReciever, TCommandId>
        where TReciever : class
    {
        TCommandId CommandId { get; }
        void Execute(TReciever receiver);
    }
}
