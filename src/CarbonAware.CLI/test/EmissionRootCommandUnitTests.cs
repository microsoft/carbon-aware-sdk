using System.CommandLine;
using System.CommandLine.Parsing;

using CarbonAware.Aggregators.CarbonAware;
using CarbonAware.CLI.CommandKeywords.Emissions;

using Moq;

namespace CarbonAware.CLI.UnitTests
{
    class EmissionCommandUnitTests
    {
        Mock<ICarbonAwareAggregator>? _aggregator;

        [SetUp]
        public void Setup()
        {
            _aggregator = new Mock<ICarbonAwareAggregator>();
        }

        [Test]
        public void AddEmissionsCommand_CreatesCorrectSubCommandAndOptions()
        {
            var command = new RootCommand();
            if (_aggregator != null)
            {
                EmissionsRootCommand.AddEmissionsCommands(ref command, _aggregator.Object);
            }
            else { Assert.NotNull(_aggregator); }

            var subCommands = command.Subcommands.AsEnumerable();
            Assert.That(subCommands.First().Name, Is.EqualTo("emissions"));

            // Find the emissions observed keyword command
            var observedCommand = subCommands.First().Subcommands.Where(command => command.Name == "observed").First();
            
            // We only added the emissions commands so there should be exactly one keyword off the root command
            Assert.That(command.Subcommands.Count, Is.EqualTo(1));

            var startTimeOption = observedCommand.Options.Where(option => option.Name == "startTime");
            var endTimeOption = observedCommand.Options.Where(option => option.Name == "endTime");

            // Both options should yield exactly one instance of each when searching the tree.
            Assert.That(startTimeOption.Count, Is.EqualTo(1));
            Assert.That(endTimeOption.Count, Is.EqualTo(1));
        }

        [Test]
        public void Providing_RequiredArguments_SuccessfullyCallsAggregator()
        {
            var command = new RootCommand();
            if (_aggregator != null)
            {
                EmissionsRootCommand.AddEmissionsCommands(ref command, _aggregator.Object);
                String[] args = {"emissions", "observed", "eastus"};
                
                var exitCode = command.Invoke(args);
                Assert.AreEqual(exitCode, 0);
                
                _aggregator.Verify(a => a.GetEmissionsDataAsync(It.IsAny<Dictionary<string, object>>()), Times.Once);
            }
            else 
            {
                Assert.NotNull(_aggregator);
            }

        }

    }
}

