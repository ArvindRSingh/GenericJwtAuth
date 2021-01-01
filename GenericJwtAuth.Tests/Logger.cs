using AzureTableIdentity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using System;

namespace GenericJwtAuth.Tests
{
    public class Logger : ILogger<UserManager<AzureTableUser>>
    {
        public IDisposable BeginScope<TState>(TState state)
        {
            return new ConsoleLoggerProvider(new Options).CreateLogger("alkfjsd").BeginScope(state);
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            throw new NotImplementedException();
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            throw new NotImplementedException();
        }
    }
}