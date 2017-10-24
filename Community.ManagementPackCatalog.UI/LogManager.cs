// -----------------------------------------------------------------------
// <copyright file="LogManager.cs">
// This is part of the Community Management Pack Catalog and licensed GPL v3; See https://github.com/mpcatalog/scom-community-catalog-ui/blob/master/LICENSE.
// </copyright>
// -----------------------------------------------------------------------
namespace Community.ManagementPackCatalog.UI
{
    using System;
    using System.Diagnostics;
    using System.Diagnostics.Tracing;

    /// <summary>
    /// This Class uses ETW and logs against the ETW Event source Community.ManagementPackCatalog
    /// </summary>
    [EventSource(Name = "Community.ManagementPackCatalog")]
    public class LogManager : EventSource
    {
        /// <summary>
        /// The Singleton log manager instance that can be used to easily and quickly log.
        /// </summary>
        private static readonly LogManager ActiveLogManager = new LogManager();

        /// <summary>
        /// When writing to the Windows Event Log, this is the source that events will be generated as
        /// </summary>
        private readonly string eventLogSource = "Community MP Catalog";

        /// <summary>
        /// When writing to the Windows Event Log, this is the Log that the events will be sent to.
        /// </summary>
        private readonly string eventLogName = "Operations Manager";

        /// <summary>
        /// The EventTypes Enumeration gives loose classification to how the trace impacts the application.
        /// </summary>
        public enum EventType : int
        {
            /// <summary>
            /// An External call that leaves the local client process and expects a return.
            /// </summary>
            ExternalDependency = 0,

            /// <summary>
            /// Accessing a resource local to the machine running this code.
            /// Use ExternalDependency if the item is on another server/computer.
            /// </summary>
            ResourceActivity = 1,

            /// <summary>
            /// The User made a request to the application which needs to be handled.
            /// </summary>
            UIActivity = 2,

            /// <summary>
            /// This indicates a type that you cannot classify.  Extending the Enumeration may be required if this is frequently used.
            /// </summary>
            Unknown = 3
        }

        /// <summary>
        /// Gets A Static instance of this class to raise events against.
        /// </summary>
        public static LogManager Log
        {
            get
            {
                return ActiveLogManager;
            }
        }

        /// <summary>
        /// Creates a Generic ETW Trace Event that can be used to track anything.
        /// </summary>
        /// <param name="eventType">What type of Trace is this Event</param>
        /// <param name="traceAction">What short phrase describes this Action?</param>
        /// <param name="traceDescription">A More verbose option for Describing the event.</param>
        /// <param name="traceID">A Representative ID to track this Trace Later</param>
        [Event(1000, Level = EventLevel.Verbose, Message = "Trace Event")]
        public void WriteTrace(
            EventType eventType,
            string traceAction,
            string traceDescription = "",
            int traceID = 0)
        {
            WriteEvent(1000, eventType, traceAction, traceDescription, traceID);
        }

        /// <summary>
        /// Create a ETW Warning Event,
        /// </summary>
        /// <param name="eventType">What Type of Warning Event is this</param>
        /// <param name="warningAction">Short phrase that Describes this Action.</param>
        /// <param name="warningDescription">A full description of the Warning Event</param>
        /// <param name="warningId">A Representative ID to track this Warning later.</param>
        [Event(1100, Level = EventLevel.Warning, Message = "Warning Event")]
        public void WriteWarning(
           EventType eventType,
           string warningAction,
           string warningDescription = "",
           int warningId = 0)
        {
            WriteEvent(1100, eventType, warningAction, warningDescription, warningId);

            // As this is an Warning level event, we will also be placing it in the event log
            // We also perform a quick check and create our event log source if needed.
            if (CheckForEventLogAccessibility())
            {
                EventLog.WriteEntry(eventLogSource, warningDescription, EventLogEntryType.Warning, warningId);
            }
        }

        /// <summary>
        /// Create an ETW Error Event
        /// </summary>
        /// <param name="eventType">What Type of Error Event is this</param>
        /// <param name="errorAction">Short Phrase defining the Action this Error Represents</param>
        /// <param name="errorDesctiption">A full description of the Error Event.</param>
        /// <param name="errorID">A Representative ID to track this Error Later</param>
        [Event(1200, Level = EventLevel.Error, Message = "Error Event")]
        public void WriteError(
           EventType eventType,
           string errorAction,
           string errorDesctiption = "",
           int errorID = 0)
        {
            WriteEvent(1200, eventType, errorAction, errorDesctiption, errorID);

            // As this is an Error level event, we will also be placing it in the event log
            // We also perform a quick check and create our event log source if needed.
            if (CheckForEventLogAccessibility())
            {
                EventLog.WriteEntry(eventLogSource, errorDesctiption, EventLogEntryType.Error, errorID);
                System.Windows.Forms.MessageBox.Show(errorDesctiption, errorAction);
            }
            else
            {
                // fall back to a message box
                System.Windows.Forms.MessageBox.Show(errorDesctiption, errorAction);
            }
        }

        /// <summary>
        /// Performs a check to see if the LogManager will be able to write to the windows event log
        /// </summary>
        /// <returns>A boolean representing Log Accessibility</returns>
        private bool CheckForEventLogAccessibility()
        {
            try
            {
                if (!EventLog.SourceExists(eventLogSource))
                {
                    EventLog.CreateEventSource(eventLogSource, eventLogName);
                }

                return true;
            }
            catch (System.Security.SecurityException)
            {
                // We didn't have access to all the logs, and the Source Check could not complete
                // Typically the access exception is not for the Operations Manager log, but
                // Log Source creation cannot occur when this permission is not available
                return false;
            }
            catch (Exception)
            {
                // Not expecting this exception, throw to the calling code
                throw;
            }
        }
    }
}