// -----------------------------------------------------------------------
// <copyright file="ManagementGroupConnector.cs">
// This is part of the Community Management Pack Catalog and licensed GPL v3; See https://github.com/mpcatalog/scom-community-catalog-ui/blob/master/LICENSE.
// </copyright>
// -----------------------------------------------------------------------

namespace Community.ManagementPackCatalog.UI.DataConnectors
{
    using System;
    using System.Collections.Generic;
    using Microsoft.EnterpriseManagement;
    using Microsoft.EnterpriseManagement.Configuration;
    using Microsoft.Win32;
    using static LogManager;

    /// <summary>
    /// The <see cref="ManagementGroupConnector"/> establishes a SDK connection to the SCOM instance.
    /// This connection is used to check if packs are installed
    /// </summary>
    internal static class ManagementGroupConnector
    {
        /// <summary>
        /// <see cref="ManagementPack"/> objects in a Dictionary sorted by SystemName.  Used for lightweight lookup
        /// </summary>
        private static SortedDictionary<string, ManagementPack> managementPacks;

        /// <summary>
        /// Initializes static members of the <see cref="ManagementGroupConnector"/> class.
        /// </summary>
        static ManagementGroupConnector()
        {
            try
            {
                // On load we will attempt to connect to the management group that the active console is connected to.
                // if the connection fails an event can be logged and a manual connection will be needed.
                ConnectToDefaultManagementGroup();

                // If our default connection works, populate the management packs in the Dictionary.
                PopulateManagementPackDictionary();
            }
            catch (Exception ex)
            {
                Log.WriteWarning(
                   EventType.ExternalDependency,
                   "Unable to connect to Management Group",
                   "Unable to connect to the default management group and populate the dictionary on instantiation." + Environment.NewLine + ex.Message);
            }
        }

        /// <summary>
        /// Gets or sets Connection to the active managementGroup
        /// </summary>
        public static ManagementGroup CurrentManagementGroup { get; set; }

        /// <summary>
        /// Connect the <see cref="CurrentManagementGroup"/> to the specified management server.
        /// </summary>
        /// <param name="managementServerURI">URI of the management Server to connect to</param>
        public static void ConnectToManagementGroup(string managementServerURI)
        {
            if (string.IsNullOrEmpty(managementServerURI))
            {
                throw new ArgumentNullException("Need a Management Group URI for a connection.");
            }

            CurrentManagementGroup = new ManagementGroup(managementServerURI);
        }

        /// <summary>
        /// Uses the registry key from the console to connect to the default management group.
        /// This is the same group that would appear when you open the console.
        /// </summary>
        public static void ConnectToDefaultManagementGroup()
        {
            string defaultManagementGroupUri = Registry.GetValue("HKEY_CURRENT_USER\\Software\\Microsoft\\Microsoft Operations Manager\\3.0\\User Settings", "SDKServiceMachine", null).ToString();
            if (string.IsNullOrWhiteSpace(defaultManagementGroupUri))
            {
                throw new Exception("Unable to obtain the Default Management Group");
            }

            ConnectToManagementGroup(defaultManagementGroupUri);
        }

        /// <summary>
        /// Use <see cref="CurrentManagementGroup"/> to populate <see cref="managementPacks"/> using the Name (SystemName) as the key
        /// </summary>
        internal static void PopulateManagementPackDictionary()
        {
            try
            {
                managementPacks = new SortedDictionary<string, ManagementPack>();
                foreach (ManagementPack mp in CurrentManagementGroup.ManagementPacks.GetManagementPacks())
                {
                    managementPacks.Add(mp.Name, mp);
                }
            }
            catch (Exception)
            {
                Log.WriteError(
                   EventType.ExternalDependency,
                   "Unable to populate the management pack dictionary.",
                   CurrentManagementGroup?.Name);
                throw;
            }
        }

        /// <summary>
        /// Gets an instance of the <see cref="ManagementPack"/> class matching the name passed in.
        /// Returns NULL if the pack is not installed
        /// </summary>
        /// <param name="managementPackSystemName">SystemName of the management pack to retrieve</param>
        /// <returns>A <see cref="ManagementPack"/> object or NULL</returns>
        internal static ManagementPack GetManagementPackIfInstalled(string managementPackSystemName)
        {
            if (managementPacks == null)
            {
                return null;
            }
            else
            {
                return managementPacks.ContainsKey(managementPackSystemName) ? managementPacks[managementPackSystemName] : null;
            }
        }

        /// <summary>
        /// Checks to see if the Management Pack name passed in is installed.
        /// </summary>
        /// <param name="managementPackSystemName">SystemName of the management pack to retrieve</param>
        /// <returns>Boolean representing the Management Packs installation status</returns>
        internal static bool IsManagementPackInstalled(string managementPackSystemName)
        {
            if (managementPacks == null)
            {
                return false;
            }
            else
            {
                return managementPacks.ContainsKey(managementPackSystemName);
            }
        }
    }
}