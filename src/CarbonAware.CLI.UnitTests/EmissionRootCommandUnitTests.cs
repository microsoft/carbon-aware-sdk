using CarbonAware.Aggregators.CarbonAware;
using CarbonAware.CLI.CommandKeywords.Emissions;
using Moq;
using System.CommandLine;
using System.CommandLine.Parsing;

namespace CarbonAware.CLI.UnitTests
{
    class EmissionCommandUnitTests
    {
        Mock<ICarbonAwareAggregator> _aggregator;

        [SetUp]
        public void Setup()
        {
            _aggregator = new Mock<ICarbonAwareAggregator>();
        }

        [Test]
        public void AddEmissionsCommand_CreatesCorrectSubCommandAndOptions()
        {
            var command = new RootCommand();
            EmissionsRootCommand.AddEmissionsCommands(ref command, _aggregator.Object);

            var subCommands =  command.Subcommands.AsEnumerable();
            var listCommand = subCommands.First().Subcommands.First();

            Assert.That(command.Subcommands.Count, Is.EqualTo(1));
            Assert.That(listCommand.Options.Count, Is.EqualTo(4));
        }

        [Test]
        public void Providing_RequiredArguments_SuccessfullyCallsAggregator()
        {
            var command = new RootCommand();
            EmissionsRootCommand.AddEmissionsCommands(ref command, _aggregator.Object);
            String[] args = {"emissions", "observed", "eastus"};
            
            var exitCode = command.Invoke(args);
            
            _aggregator.Verify(a => a.GetEmissionsDataAsync(It.IsAny<Dictionary<string, object>>()), Times.Once);
        }

    }
}

