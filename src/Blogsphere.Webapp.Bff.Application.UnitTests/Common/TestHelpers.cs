using AutoMapper;
using Blogsphere.Webapp.Bff.Application.Mappers;
using Blogsphere.Webapp.Bff.Domain.Models.Core;
using Moq;

namespace Blogsphere.Webapp.Bff.Application.UnitTests.Common
{
    public static class TestHelpers
    {
        private static readonly Lazy<IMapper> _mapperLazy = new(() =>
            new MapperConfiguration(cfg => cfg.AddProfile(new EntityToDtoMapper()))
                .CreateMapper());

        public static IMapper Mapper => _mapperLazy.Value;

        public static Mock<ILogger> CreateLoggerMock()
        {
            var logger = new Mock<ILogger>();

            // The codebase uses Serilog's contextual logging heavily (logger.ForContext(...).ForContext(...)).
            // Ensure chained calls don't return null in unit tests.
            logger
                .Setup(l => l.ForContext(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()))
                .Returns(logger.Object);
            logger
                .Setup(l => l.ForContext(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<bool>()))
                .Returns(logger.Object);

            return logger;
        }

        public static ILogger CreateLogger() => CreateLoggerMock().Object;

        public static RequestInformation CreateRequestInformation(string correlationId = "test-correlation-id")
        {
            return new RequestInformation
            {
                CorreationId = correlationId,
                CurrentUser = null
            };
        }
    }
}
