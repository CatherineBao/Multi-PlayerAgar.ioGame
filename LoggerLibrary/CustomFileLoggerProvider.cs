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
/// This project contains the CustomFileLoggerProvider for the 
/// CustomeFileLogger. This class acts as the wrapper class for 
/// the CustomeFileLogger object. 
/// </summary>
/// 

namespace LoggerLibrary
{
    /// <summary>
    /// A wrapper class for the custom file logger
    /// </summary>
    public class CustomFileLoggerProvider : ILoggerProvider
    {
        /// <summary>
        /// Instance variable for the logger
        /// </summary>
        private CustomFileLogger logger;

        /// <summary>
        /// Create the logger 
        /// </summary>
        /// <param name="categoryName">The level that the logger is recording at</param>
        /// <returns>Newly created logger</returns>
        public ILogger CreateLogger(string categoryName)
        {
            this.logger = new CustomFileLogger(categoryName);
            return logger;
        }

        ///<inheritdoc/>
        public void Dispose()
        {
        }
    }
}
