using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Quartz;
using SchedulingApp;
using Xunit;

namespace StatelessTests
{
    public class UnitTest1
    {
        [Fact]
        public async Task Test1()
        {

            var scheduler = Substitute.For<IScheduler>();
            var p = new HostedProgram(scheduler, NullLogger<HostedProgram>.Instance);
            await p.StartAsync(CancellationToken.None);

            scheduler.ReceivedCalls().Where(c => c.GetMethodInfo().Name == nameof(IScheduler.Start)).Should().HaveCount(1);
        }
    }
}
