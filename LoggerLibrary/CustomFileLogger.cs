using Microsoft.Extensions.Logging;
/// <summary>
/// Author:    Catherine Bao
/// Partner:   Jessie Taubert
/// Date:      April 1, 2024
/// Course:    CS 3500, University of Utah, School of Computing
/// Copyright: CS 3500 and Catherine Bao and Jessie Taubert - This work 
///            may not be copied for use in Academic Coursework.
///
/// I, Catherine Bao and Jessie Taubert, certify that I wrote this 
/// code from scratch and did not copy it in part or whole from 
/// another source.  All references used in the completion of the 
/// assignments are cited in my README file.
///
/// File Contents
/// This project contains the CustomFileLogger which logs the 
/// timestamp, catagoryName, and the logger message to a file. 
/// </summary>
/// 
namespace LoggerLibrary
{
    /// <summary>
    /// Actual logger object that appends text to a file
    /// </summary>
    public class CustomFileLogger : ILogger
    {
        /// <summary>
        /// The file that the logger logs to
        /// </summary>
        string _FileName;

        /// <summary>
        /// Adding the event to the file using the logger 
        /// </summary>
        /// <param name="categoryName">The level that the event is logged at</param>
        public CustomFileLogger(string categoryName)
        {
            _FileName = Environment.GetFolderPath(
                Environment.SpecialFolder.ApplicationData)
                + Path.DirectorySeparatorChar
                + $"ChatLog-{categoryName}.log";
        }

        /// <summary>
        /// Creates another level of nesting so the event in the scope can be tagged with information on where they came from. 
        /// </summary>
        /// <typeparam name="TState">The state of the event</typeparam>
        /// <param name="state">Identifier for the state</param>
        /// <returns>Ends the logical operation</returns>
        /// <exception cref="NotImplementedException">Not actually implemented for this assignment</exception>
        public IDisposable? BeginScope<TState>(TState state) where TState : notnull
        {
            throw new NotImplementedException();
        }

        ///<inheritdoc/>
        public bool IsEnabled(LogLevel logLevel)
        {
            throw new NotImplementedException();
        }

        ///<inheritdoc/>
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            lock (this)
            {
                File.AppendAllText(_FileName, $"{DateTime.Now} ({System.Threading.Thread.CurrentThread.ManagedThreadId}) - {logLevel.ToString()} - {formatter(state, exception)} {Environment.NewLine}");
            }
        }
    }
}
