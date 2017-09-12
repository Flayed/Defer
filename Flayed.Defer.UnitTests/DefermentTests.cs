using FluentAssertions;
using NUnit.Framework;
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
        public async Task Defer_RemovesFromTasklistWhenComplete()
        {
            Deferment deferment = new Deferment();

            deferment.Defer(() => { }, 10, CancellationToken.None);

            deferment.Tasks.Should().NotBeEmpty();

            await Task.WhenAll(deferment.Tasks.ToArray());

            deferment.Tasks.Should().BeEmpty();
        }

    }
}
