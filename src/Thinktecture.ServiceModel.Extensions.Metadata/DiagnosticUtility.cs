// ---------------------------------------------------------------------------------
// File:            DignosticUtility.cs
// Description:     
//
// Author:          Buddhike de Silva
// Date Created:    11th May 2008
// ---------------------------------------------------------------------------------

using System;
using System.Collections;
using System.Threading;
using System.Diagnostics;
using Thinktecture.ServiceModel.Extensions.Metadata.Properties;
using System.Runtime.InteropServices;
using System.Text;
using System.Globalization;

namespace Thinktecture.ServiceModel.Extensions.Metadata
{
    /// <summary>
    /// 
    /// </summary>
    internal static class DiagnosticUtility
    {
        #region Private Classes

        /// <summary>
        /// This class defines the constants used in DiagnosticUtility class.
        /// </summary>
        static class DiagnosticUtilityConstants
        {
            #region Constant Fields

            public const string TraceSourceName = "Thinktecture.ServiceModel.Extensions.Metadata";

            public const int MetadataServiceError = 1;
            public const int MetadataServicePlumbingError = 2;

            #endregion
        }

        #endregion        

        #region Static Fields

        private static TraceSource traceSource;
        private static object syncRoot = new object();

        #endregion

        #region Public Static Methods

        /// <summary>
        /// This method traverses a given exception tree and returns true
        /// if it contains any of the fatal runtime exceptions.
        /// </summary>
        /// <param name="exception">
        /// Root exception of the exception tree.
        /// </param>
        /// <returns>
        /// Boolean value indicating whether the exception tree contains a fatal
        /// exception or not.
        /// </returns>
        public static bool IsFatal(Exception exception)
        {
            Debug.Assert(exception != null, string.Format(CultureInfo.CurrentCulture, Resources.DebugParameterIsNull, "exception"));
            while (exception != null)
            {                
                Type currentExceptionType = exception.GetType();

                if (typeof(ThreadAbortException) == currentExceptionType ||
                    typeof(AppDomainUnloadedException) == currentExceptionType ||
                    typeof(OutOfMemoryException) == currentExceptionType ||
                    typeof(InsufficientMemoryException) == currentExceptionType ||
                    typeof(StackOverflowException) == currentExceptionType ||
                    typeof(AccessViolationException) == currentExceptionType ||
                    typeof(SEHException) == currentExceptionType)
                {
                    return true;
                }

                exception = exception.InnerException;
            }
            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="exception"></param>
        public static void LogMetadataServiceException(Exception exception)
        {
            LogExceptionCore(exception, DiagnosticUtilityConstants.MetadataServiceError);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="exception"></param>
        public static void LogMetadataServicePlumbingException(Exception exception)
        {
            LogExceptionCore(exception, DiagnosticUtilityConstants.MetadataServicePlumbingError);
        }

        public static void ShutDownTraceSourceGracefully()
        {
            if (traceSource != null)
            {
                traceSource.Flush();
                traceSource.Close();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public static Exception ThrowInvalidOperationExceptionFromService(string message)
        {
            InvalidOperationException invalidOperationException = new InvalidOperationException(message);
            LogMetadataServiceException(invalidOperationException);
            return invalidOperationException;
        }

        #endregion

        #region Private Static Methods

        /// <summary>
        /// Formats and logs the details of a given Exception. In addition to the exception details,
        /// this method also logs the current process name, process id, managed thread id of the thread
        /// that exception was throw in and the name of the identity of the current principal.
        /// </summary>
        /// <param name="exception">Exception to be logged.</param>
        /// <param name="eventId">An event id used to categorize the exceptions in the target log.</param>
        private static void LogExceptionCore(Exception exception, int eventId)
        {
            Debug.Assert(exception != null, string.Format(CultureInfo.CurrentCulture, Resources.DebugParameterIsNull, "exception"));
            if (exception != null)
            {
                StringBuilder logStringBuilder = new StringBuilder();
                Process currentProcess = Process.GetCurrentProcess();

                while (exception != null)
                {
                    logStringBuilder.AppendLine(string.Format(CultureInfo.CurrentCulture, "Exception: {0}", exception.GetType().ToString()));
                    logStringBuilder.AppendLine(string.Format(CultureInfo.CurrentCulture, "Details: {0}", exception.Message));
                    logStringBuilder.AppendLine(string.Format(CultureInfo.CurrentCulture, "Process: {0}", currentProcess.ProcessName));
                    logStringBuilder.AppendLine(string.Format(CultureInfo.CurrentCulture, "Process Id: {0}", Process.GetCurrentProcess().Id));
                    logStringBuilder.AppendLine(string.Format(CultureInfo.CurrentCulture, "Managed Thread Id: {0}", Thread.CurrentThread.ManagedThreadId));
                    logStringBuilder.AppendLine(string.Format(CultureInfo.CurrentCulture, "Current Principal: {0}", Thread.CurrentPrincipal.Identity.Name));
                    logStringBuilder.AppendLine("Stack Trace:");                    
                    logStringBuilder.AppendLine(exception.StackTrace);
                    logStringBuilder.AppendLine();

                    exception = exception.InnerException;
                }

                TraceSource currentTraceSource = CreateOrGetTraceSource();                
                currentTraceSource.TraceEvent(TraceEventType.Error, eventId, logStringBuilder.ToString());
                currentTraceSource.Flush();
                logStringBuilder = null;
            }
        }        

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private static TraceSource CreateOrGetTraceSource()
        {
            if (traceSource == null)
            {
                lock (syncRoot)
                {
                    if (traceSource == null)
                    {
                        Thread.MemoryBarrier();
                        traceSource = new TraceSource(DiagnosticUtilityConstants.TraceSourceName, SourceLevels.Error);
                    }
                }
            }

            return DiagnosticUtility.traceSource;
        }

        #endregion
    }
}
