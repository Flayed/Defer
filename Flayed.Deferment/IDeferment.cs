using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Flayed.Deferment
{
    public interface IDeferment
    {
        IEnumerable<Task> Tasks { get; }

        void Defer(Action action, int delay);
        void Defer(Action action, int delay, CancellationToken cancellationToken);
        void Defer(Func<Task> func, int delay);
        void Defer(Func<Task> func, int delay, CancellationToken cancellationToken);
        void Run(Action action);
        void Run(Action action, CancellationToken cancellationToken);
        void Run(Func<Task> func);
        void Run(Func<Task> func, CancellationToken cancellationToken);
    }
}