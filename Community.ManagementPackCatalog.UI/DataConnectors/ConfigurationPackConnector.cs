// -----------------------------------------------------------------------
// <copyright file="ConfigurationPackConnector.cs">
// This is part of the Community Management Pack Catalog and licensed GPL v3; See https://github.com/mpcatalog/scom-community-catalog-ui/blob/master/LICENSE.
// </copyright>
// -----------------------------------------------------------------------

namespace Community.ManagementPackCatalog.UI.DataConnectors
{
    using System;
    using System.IO;
    using System.Reflection;
    using System.Windows.Forms;
    using Microsoft.EnterpriseManagement;
    using Microsoft.EnterpriseManagement.Configuration;
    using static Community.ManagementPackCatalog.UI.LogManager;

    /// <summary>
    /// This class handles connections and modification of the Configuration Pack for the Community Management Pack Catalog
    /// </summary>
    internal class ConfigurationPackConnector
    {
        /// <summary>
        /// Once populated, represents the ManagementPack object for the Override Pack
        /// </summary>
        private ManagementPack configurationOverridePack;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigurationPackConnector"/> class.
        /// Connects to the provided management server or URI
        /// </summary>
        public ConfigurationPackConnector()
        {
            Log.WriteTrace(
                EventType.UIActivity,
                "Connecting to configuration override Management Pack");

            try
            {
                ManagementGroup managementGroupConnection = ManagementGroupConnector.CurrentManagementGroup;
                ManagementPackCriteria packCriteria = new ManagementPackCriteria("Name = 'Community.ManagementPackCatalog.Configuration'");
                var packResults = managementGroupConnection.ManagementPacks.GetManagementPacks(packCriteria);
                if (packResults.Count == 1)
                {
                    // Good news, we should only see one
                    configurationOverridePack = packResults[0];
                }
                else if (packResults.Count == 0)
                {
                    // install the shell unsealed pack
                    try
                    {
                        DialogResult installPack = System.Windows.Forms.MessageBox.Show(
                            "A small unsealed MP is required to configure the alerting, can we create it now?" 
                            + Environment.NewLine + "Community.ManagementPackCatalog.Configuration will be created for alert configuration overrides."
                            , "Create Override Pack?"
                            , System.Windows.Forms.MessageBoxButtons.YesNo
                            , System.Windows.Forms.MessageBoxIcon.Information);

                        if (installPack == DialogResult.Yes)
                        {
                            InstallConfigurationPack();
                        }
                        else
                        {
                            throw new Exception("The Settings pane cannot be used without the unsealed Configuration management pack.");
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.WriteError(
                            EventType.UIActivity,
                            "Unable to install management pack.",
                            ex.Message);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.WriteError(
                        EventType.ExternalDependency,
                        "Unable to connect to Management Group and Get Override Pack",
                        ex.Message);
            }
        }

        /// <summary>
        /// Modify the enabled setting on the Community Pack Update Monitor to match the boolean passed in.
        /// </summary>
        /// <param name="enableAlerting">Status to set the monitor to</param>
        public void ModifyPackUpdateMonitorSetting(bool enableAlerting)
        {
            string overrideName = "Community.ManagementPackCatalog.Configuration.EnableUpdateMonitor";

            Log.WriteTrace(
               EventType.ExternalDependency,
               "Connecting to Override to Modify",
               overrideName);

            try
            {
                ManagementPackMonitorPropertyOverride monitorOverrideToWorkWith = (ManagementPackMonitorPropertyOverride)configurationOverridePack.GetOverride(overrideName);
                ManagementPackMonitorPropertyOverride remadeOverride = new ManagementPackMonitorPropertyOverride(configurationOverridePack, overrideName);
                remadeOverride.Context = monitorOverrideToWorkWith.Context;
                remadeOverride.Description = monitorOverrideToWorkWith.Description;
                remadeOverride.Enforced = false;
                remadeOverride.DisplayName = monitorOverrideToWorkWith.DisplayName ?? "not used";
                remadeOverride.Monitor = monitorOverrideToWorkWith.Monitor;
                remadeOverride.Property = ManagementPackMonitorProperty.Enabled;
                remadeOverride.Value = enableAlerting.ToString();
                SaveManagementPack();
            }
            catch (Exception ex)
            {
                Log.WriteError(
                        EventType.ExternalDependency,
                        "Unable to connect to update monitoring alert override.",
                        ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Get the status of the Community Pack Update Monitor
        /// </summary>
        /// <returns>a boolean representing the state of the Monitor</returns>
        public bool GetPackUpdateMonitorSetting()
        {
            ManagementPackOverride packStatusMonitorOverride = configurationOverridePack.GetOverride("Community.ManagementPackCatalog.Configuration.EnableUpdateMonitor");
            return bool.Parse(packStatusMonitorOverride.Value);
        }

        /// <summary>
        /// After prompting the user for approval, this method installs the Configuration override management pack.
        /// </summary>
        private void InstallConfigurationPack()
        {
            string tempFileName = Path.GetTempPath() + "Community.ManagementPackCatalog.Configuration.xml";
            using (var resource = Assembly.GetExecutingAssembly().GetManifestResourceStream("Community.ManagementPackCatalog.UI.Community.ManagementPackCatalog.Configuration.xml"))
            {
                using (var file = new FileStream(tempFileName, FileMode.Create, FileAccess.Write))
                {
                    resource.CopyTo(file);
                }
            }

            configurationOverridePack = new ManagementPack(tempFileName);
            ManagementGroupConnector.CurrentManagementGroup.ManagementPacks.ImportManagementPack(configurationOverridePack);

            // Cleanup our temp stuff if we can
            if (tempFileName != null)
            {
                try
                {
                    File.Delete(tempFileName);
                }
                catch
                {
                    // best effort
                }

                tempFileName = null;
            }
        }

        /// <summary>
        /// Save the management pack to the active connection.
        /// </summary>
        private void SaveManagementPack()
        {
            UpLevelVersion();

            try
            {
                configurationOverridePack.Verify();
                configurationOverridePack.AcceptChanges();
                Log.WriteTrace(
                      EventType.ExternalDependency,
                      "Successfully saved the updated management pack",
                      configurationOverridePack.Version.ToString());
            }
            catch (Exception ex)
            {
                Log.WriteError(
                       EventType.ExternalDependency,
                       "Unable to save the updated management pack",
                       ex.Message);
                throw;
            }
        }

        /// <summary>
        /// When making changes to the management pack we increase the revision by one version
        /// </summary>
        private void UpLevelVersion()
        {
            System.Version currentVersion = configurationOverridePack.Version;
            System.Version newVersion = default(System.Version);
            newVersion = new System.Version(currentVersion.Major, currentVersion.Minor, currentVersion.Build, currentVersion.Revision + 1);
            configurationOverridePack.Version = newVersion;
        }
    }
}
