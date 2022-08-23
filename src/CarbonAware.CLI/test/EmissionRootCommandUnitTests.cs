using System.CommandLine;
using System.CommandLine.Parsing;

using CarbonAware.Aggregators.CarbonAware;
using CarbonAware.CLI.CommandKeywords.Emissions;

using Moq;

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

            // Find the emissions observed keyword command
            var observedCommand = subCommands.Where(command => command.Name == "observed").First();

            // We only added the emissions commands so there should be exactly one keyword off the root command
            Assert.That(command.Subcommands.Count, Is.EqualTo(1));

            // Options are startTime and endTime for the observed keyword
            Assert.That(observedCommand.Options.Count, Is.EqualTo(2));
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

