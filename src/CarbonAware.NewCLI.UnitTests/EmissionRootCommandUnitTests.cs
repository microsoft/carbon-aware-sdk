using CarbonAware.Aggregators.CarbonAware;
using CarbonAware.NewCLI.CommandKeywords.Emissions;
using Moq;
using System.CommandLine;
using System.CommandLine.Parsing;

namespace CarbonAware.NewCLI.UnitTests
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
            String[] args = {"emissions", "list", "--locations", "eastus"};
            
            var exitCode = command.Invoke(args);
            Assert.That(exitCode, Is.EqualTo(0));

            _aggregator.Verify(a => a.GetEmissionsDataAsync(It.IsAny<Dictionary<string, object>>()), Times.Once);
        }

        [Test]
        public void NotProviding_RequiredArguments_ResultsInParseErrror()
        {
            var command = new RootCommand();
        
            EmissionsRootCommand.AddEmissionsCommands(ref command, _aggregator.Object);
            String[] args = { "emissions", "list", "--startTime", "02-02-2022" };
           
            var exitCode = command.Invoke(args);
            Assert.That(exitCode, Is.EqualTo(1));
        }

        [TestCase("invalid", "02-02-2022") ]
        [TestCase("02-02-2022", "invalid")]
        public void ProvidingInvalidDate_ResultsInParseErrror(string? startTime = null, string? toTime = null)
        {
            var command = new RootCommand();

            EmissionsRootCommand.AddEmissionsCommands(ref command, _aggregator.Object);
            String[] args = { "emissions", "list", "--locations", "eastus", "--startTime", startTime!, "--toTime", toTime! };

            var exitCode = command.Invoke(args);
            Assert.That(exitCode, Is.EqualTo(1));
        }
    }
}


