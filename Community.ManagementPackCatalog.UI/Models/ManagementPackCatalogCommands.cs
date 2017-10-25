// -----------------------------------------------------------------------
// <copyright file="ManagementPackCatalogCommands.cs">
// This is part of the Community Management Pack Catalog and licensed GPL v3; See https://github.com/mpcatalog/scom-community-catalog-ui/blob/master/LICENSE.
// </copyright>
// -----------------------------------------------------------------------

namespace Community.ManagementPackCatalog.UI.Models
{
    using System;
    using System.ComponentModel.Design;
    using System.Drawing;
    using Microsoft.EnterpriseManagement.ConsoleFramework;
    using static Community.ManagementPackCatalog.UI.LogManager;

    /// <summary>
    /// This class is used to create the custom commands utilized by the Management Pack Catalog
    /// </summary>
    public class ManagementPackCatalogCommands
    {
        /// <summary>
        /// Gets the GUID that represents the View URL command.
        /// </summary>
        public static readonly Guid ViewUrlGuid;

        /// <summary>
        /// Gets the CommandID for the ViewURL operation.
        /// </summary>
        public static readonly CommandID ViewURL;

        /// <summary>
        /// Initializes static members of the <see cref="ManagementPackCatalogCommands"/> class
        /// Generate the Static Management Pack Control buttons.
        /// </summary>
        static ManagementPackCatalogCommands()
        {
            // Picked some random GUIDs to represent our commands, the chance that the have already been used is pretty low
            ViewUrlGuid = new Guid("{5E827AF2-CB76-47F9-A560-AFB2986C4F5C}");

            // Assigned the Text and GUIDs to our commands
            ViewURL = CreateCommand(0, ViewUrlGuid, "View Management Pack Online");
        }

        /// <summary>
        /// Gets CommandService used to create new UI commands
        /// </summary>
        internal static ICommandService CommandService
        {
            get
            {
                return (ICommandService)FrameworkServices.GetService(typeof(ICommandService));
            }
        }

        /// <summary>
        /// Create A CommandID object for the specified parameters
        /// </summary>
        /// <param name="id">ID of the command</param>
        /// <param name="commandGuid">GUID of the command to create</param>
        /// <param name="commandText">Text for the Command</param>
        /// <returns>A CommandID object for the UI</returns>
        private static CommandID CreateCommand(int id, Guid commandGuid, string commandText)
        {
            CommandID commandID = new CommandID(commandGuid, id);
            Command command = new Command(commandID)
            {
                Text = commandText
            };
            if (CommandService.TryAdd(command))
            {
                Log.WriteTrace(
                    EventType.UIActivity,
                    "Successfully created a UI Command without Image",
                    commandID.ToString() + " == " + commandText);
                return commandID;
            }

            return null;
        }

        /// <summary>
        /// Create A CommandID object for the specified parameters
        /// </summary>
        /// <param name="id">ID of the command</param>
        /// <param name="commandGuid">GUID of the command to create</param>
        /// <param name="commandText">Text for the Command</param>
        /// <param name="commandImage">The Image to add to the Command</param>
        /// <returns>A CommandID object for the UI</returns>
        private static CommandID CreateCommand(int id, Guid commandGuid, string commandText, Image commandImage)
        {
            CommandID commandID = new CommandID(commandGuid, id);
            Command command = new Command(commandID)
            {
                Text = commandText,
                Image = commandImage
            };
            if (CommandService.TryAdd(command))
            {
                Log.WriteTrace(
                    EventType.UIActivity,
                    "Successfully created a UI Command with Image",
                    commandID.ToString() + " == " + commandText);

                return commandID;
            }

            return null;
        }
    }
}