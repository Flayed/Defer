using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Flayed.Deferment
{
    public class Deferment : IDeferment
    {
        /// <summary>
        /// Collections of tasks created in Deferment
        /// </summary>
        private readonly ConcurrentDictionary<Guid, Task> _tasks = new ConcurrentDictionary<Guid, Task>();

        /// <summary>
        /// Collections of tasks created in Deferment
        /// </summary>
        public IEnumerable<Task> Tasks => _tasks.Values;

        /// <summary>
        /// Defers the provided action until the supplied delay has elapsed, monitoring the cancellation token.
        /// </summary>
        /// <param name="action">The action to invoke after the supplied delay has elapsed.</param>
        /// <param name="delay">The number of miliseconds to delay before invoking the action.</param>
        /// <param name="cancellationToken">The cancellation token to observe.</param>
        public void Defer(Action action, int delay, CancellationToken cancellationToken) => Defer(action, null, delay, cancellationToken);

        /// <summary>
        /// Defers the provided action until the supplied delay has elapsed.
        /// </summary>
        /// <param name="action">The action to invoke after the supplied delay has elapsed.</param>
        /// <param name="delay">The number of miliseconds to delay before invoking the action.</param>
        public void Defer(Action action, int delay) => Defer(action, null, delay, CancellationToken.None);

        /// <summary>
        /// Defers the provided async func until the supplied delay has elapsed, monitoring the cancellation token.
        /// </summary>
        /// <param name="func">The async func to invoke after the delay has elapsed.</param>
        /// <param name="delay">The number of miliseconds to delay before invoking the async func.</param>
        /// <param name="cancellationToken">The cancellation token to observe.</param>
        public void Defer(Func<Task> func, int delay, CancellationToken cancellationToken) => Defer(null, func, delay, cancellationToken);

        /// <summary>
        /// Defers the provided async func until the supplied delay has elapsed.
        /// </summary>
        /// <param name="func">The async func to invoke after the delay has elapsed.</param>
        /// <param name="delay">The number of miliseconds to delay before invoking the async func.</param>
        public void Defer(Func<Task> func, int delay) => Defer(null, func, delay, CancellationToken.None);

        /// <summary>
        /// Runs an action on a separate thread.
        /// </summary>
        /// <param name="action">The action to invoke.</param>
        public void Run(Action action) => Defer(action, null, 0, CancellationToken.None);

        /// <summary>
        /// Runs an action on a separate thread while observing the provided cancellation token.
        /// </summary>
        /// <param name="action">The action to invoke.</param>
        /// <param name="cancellationToken">The cancellation token to observe.</param>
        public void Run(Action action, CancellationToken cancellationToken) => Defer(action, null, 0, cancellationToken);

        /// <summary>
        /// Runs an async func on a separate thread.
        /// </summary>
        /// <param name="func">The async func to invoke.</param>
        public void Run(Func<Task> func) => Defer(null, func, 0, CancellationToken.None);

        /// <summary>
        /// Runs an async func on a separate thread while observing the provided cancellation token.
        /// </summary>
        /// <param name="func">The async func to invoke.</param>
        /// <param name="cancellationToken">The cancellation token to observe.</param>
        public void Run(Func<Task> func, CancellationToken cancellationToken) => Defer(null, func, 0, cancellationToken);

        /// <summary>
        /// Defers the provided action until the supplied delay has elapsed, monitoring the cancellation token.
        /// </summary>
        /// <param name="action">The action to invoke after the supplied delay has elapsed.</param>
        /// <param name="func">The async func to invoke after the supplied delay has elapsed.</param>
        /// <param name="delay">The number of miliseconds to delay before invoking the action.</param>
        /// <param name="cancellationToken">The cancellation token to observe.</param>
        private void Defer(Action action, Func<Task> func, int delay, CancellationToken cancellationToken)
        {
            if (func == null && action == null) return;
            if (cancellationToken.IsCancellationRequested) return;
            Guid key = Guid.NewGuid();
            _tasks.TryAdd(key, Task.Run(async () =>
            {
                try
                {
                    if (delay > 0)
                    {
                        await Task.Delay(delay, cancellationToken).ConfigureAwait(false);
                    }

                    if (cancellationToken.IsCancellationRequested)
                        return;

                    if (action != null) action.Invoke();
                    if (func != null) await func.Invoke().ConfigureAwait(false);
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

        /// <summary>
        /// Removes the task with the provided key from the task list
        /// </summary>
        /// <param name="key">The key of the task to remove from the task list</param>
        private void RemoveTask(Guid key)
        {
            Task t;
            _tasks.TryRemove(key, out t);
        }
    }
}
