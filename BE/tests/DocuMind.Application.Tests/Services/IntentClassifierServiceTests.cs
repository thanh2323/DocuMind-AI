using Xunit;
using Moq;
using DocuMind.Application.Services.IntentClassifier;
using DocuMind.Core.Interfaces.ILLM;
using Microsoft.Extensions.Logging;
using DocuMind.Core.Enum;
using System.Threading.Tasks;
using System.Threading;
using System;

namespace DocuMind.Application.Tests.Services
{
    public class IntentClassifierServiceTests
    {
        private readonly Mock<ILlmService> _mockLlmService;
        private readonly Mock<ILogger<IntentClassifierService>> _mockLogger;
        private readonly IntentClassifierService _classifierService;

        public IntentClassifierServiceTests()
        {
            _mockLlmService = new Mock<ILlmService>();
            _mockLogger = new Mock<ILogger<IntentClassifierService>>();
            _classifierService = new IntentClassifierService(_mockLlmService.Object, _mockLogger.Object);
        }

        [Theory]
        [InlineData("QA", IntentType.QA)]
        [InlineData("SUMMARY", IntentType.SUMMARY)]
        [InlineData("EXPLANATION", IntentType.EXPLANATION)]
        [InlineData("qa", IntentType.QA)] // Case insensitive check
        [InlineData(" Summary ", IntentType.SUMMARY)] // Whitespace check
        public async Task ClassifyIntent_ValidResponses_ReturnsCorrectEnum(string llmResponse, IntentType expectedIntent)
        {
            // ARRANGE
            _mockLlmService.Setup(x => x.AskAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(llmResponse);

            // ACT
            var result = await _classifierService.ClassifyIntentAsync("Dummy Question");

            // ASSERT
            Assert.Equal(expectedIntent, result);
        }

        [Fact]
        public async Task ClassifyIntent_InvalidResponse_DefaultsToQA()
        {
            // ARRANGE
            _mockLlmService.Setup(x => x.AskAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync("I don't know");

            // ACT
            var result = await _classifierService.ClassifyIntentAsync("Dummy Question");

            // ASSERT
            Assert.Equal(IntentType.QA, result); // Default behavior
        }

        [Fact]
        public async Task ClassifyIntent_LlmException_DefaultsToQA()
        {
            // ARRANGE
            _mockLlmService.Setup(x => x.AskAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("LLM Error"));

            // ACT
            var result = await _classifierService.ClassifyIntentAsync("Dummy Question");

            // ASSERT
            Assert.Equal(IntentType.QA, result);
        }
    }
}
