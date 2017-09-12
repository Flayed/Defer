using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Flayed.Deferment
{
    public class Deferment
    {
        /// <summary>
        /// Collections of tasks created in Deferment
        /// </summary>
        private readonly ConcurrentDictionary<Guid, Task> _tasks = new ConcurrentDictionary<Guid, Task>();

        /// <summary>
        /// Collections of tasks created in Deferment
        /// </summary>
        public IEnumerable<Task> Tasks => _tasks.Values;

        public void Defer(Action action, int delay, CancellationToken cancellationToken)
        {
            if (action == null) return;
            Guid key = Guid.NewGuid();
            _tasks.TryAdd(key, Task.Run(async () =>
            {
                try
                {
                    if (delay > 0)
                    {
                        await Task.Delay(delay, cancellationToken);
                    }

                    action.Invoke();
                }
                catch (OperationCanceledException)
                {
                    return;
                }
                catch (Exception)
                {
                    throw;
                }
                finally
                {
                    RemoveTask(key);
                }
            }, cancellationToken));
        }

        private void RemoveTask(Guid key)
        {
            Task t;
            _tasks.TryRemove(key, out t);
        }
    }
}
