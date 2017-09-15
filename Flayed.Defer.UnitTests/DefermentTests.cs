using FluentAssertions;
using NUnit.Framework;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Flayed.Deferment.UnitTests
{
    [TestFixture]
    public class DefermentTests
    {
        [Test]
        public void Defer_AddsToTaskList()
        {
            Deferment deferment = new Deferment();

            deferment.Defer(() => { }, 5000, CancellationToken.None);

            deferment.Tasks.Should().NotBeEmpty();
        }

        [Test]
        public void Defer_NoAction_DoesNotAddToTaskList()
        {
            Deferment deferment = new Deferment();

            deferment.Defer((Action)null, 5000, CancellationToken.None);

            deferment.Tasks.Should().BeEmpty();
        }

        [Test]
        public void Defer_NoFunc_DoesNotAddToTaskList()
        {
            Deferment deferment = new Deferment();

            deferment.Defer((Func<Task>)null, 5000, CancellationToken.None);

            deferment.Tasks.Should().BeEmpty();
        }

        [Test]
        public async Task Defer_RunsAction()
        {
            bool invokedAction = false;
            Deferment deferment = new Deferment();

            deferment.Defer(() => { invokedAction = true; }, 0, CancellationToken.None);

            await Task.WhenAll(deferment.Tasks.ToArray());

            invokedAction.Should().BeTrue();
        }

        [Test]
        public async Task Defer_RunsFunc()
        {
            bool invokedFunc = false;
            Deferment deferment = new Deferment();

            deferment.Defer(async () => { await Task.Delay(0); invokedFunc = true; }, 0);

            await Task.WhenAll(deferment.Tasks.ToArray());

            invokedFunc.Should().BeTrue();
        }

        [Test]
        public async Task Defers_WaitsBeforeInvokingAction()
        {
            int delay = 10;
            Stopwatch sw = Stopwatch.StartNew();
            Deferment deferment = new Deferment();

            deferment.Defer(() => { sw.Stop(); }, delay, CancellationToken.None);

            await Task.WhenAll(deferment.Tasks.ToArray());

            sw.ElapsedMilliseconds.Should().BeGreaterOrEqualTo(delay);
        }

        [Test]
        public async Task Defer_RemovesFromTasklistWhenComplete()
        {
            Deferment deferment = new Deferment();

            deferment.Defer(() => { }, 10, CancellationToken.None);

            deferment.Tasks.Should().NotBeEmpty();

            await Task.WhenAll(deferment.Tasks.ToArray());

            deferment.Tasks.Should().BeEmpty();
        }

        [Test]
        public void Defer_Cancellation_DoesNotThrow()
        {
            Deferment deferment = new Deferment();

            CancellationTokenSource cts = new CancellationTokenSource(1);

            try
            {
                deferment.Defer(() => { }, 10, cts.Token);
            }
            catch (Exception ex)
            {
                Assert.Fail($"{ex.GetType()} should not have been thrown");
            }
        }

        [Test]
        public async Task Defer_CancellationTokenIsCancelled_DoesNotInvokeAction()
        {
            bool invokedAction = false;
            Deferment deferment = new Deferment();

            CancellationTokenSource cts = new CancellationTokenSource();
            cts.Cancel();

            deferment.Defer(() => { invokedAction = true; }, 0, cts.Token);

            await Task.WhenAll(deferment.Tasks.ToArray());

            invokedAction.Should().BeFalse();
        }

        [Test]
        public async Task Defer_CancellationDuringDelay_DoesNotInvokeAction()
        {
            bool invokedAction = false;
            Deferment deferment = new Deferment();

            CancellationTokenSource cts = new CancellationTokenSource(5);

            deferment.Defer(() => { invokedAction = true; }, 10, cts.Token);

            await Task.WhenAll(deferment.Tasks.ToArray());

            invokedAction.Should().BeFalse();
        }
    }
}
