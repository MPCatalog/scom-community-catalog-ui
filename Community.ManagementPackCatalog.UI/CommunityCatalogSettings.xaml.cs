// -----------------------------------------------------------------------
// <copyright file="CommunityCatalogSettings.xaml.cs">
// This is part of the Community Management Pack Catalog and licensed GPL v3; See https://github.com/mpcatalog/scom-community-catalog-ui/blob/master/LICENSE.
// </copyright>
// -----------------------------------------------------------------------

namespace Community.ManagementPackCatalog.UI
{
    using System;
    using System.Collections.Generic;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Threading;
    using DataConnectors;
    using Microsoft.EnterpriseManagement.Common;
    using Microsoft.EnterpriseManagement.Configuration;
    using Microsoft.EnterpriseManagement.Monitoring;

    /// <summary>
    /// Interaction logic for CommunityCatalogSettings.xaml
    /// </summary>
    public partial class CommunityCatalogSettings : UserControl
    {
        private ConfigurationPackConnector configurationManagementPack;

        /// <summary>
        /// Initializes a new instance of the <see cref="CommunityCatalogSettings"/> class. 
        /// </summary>
        public CommunityCatalogSettings()
        {
            InitializeComponent();
            configurationManagementPack = new ConfigurationPackConnector();
            CheckCurrentConfiguration();
        }

        /// <summary>
        /// Pulls the current settings from the management pack and updates UI elements accordingly
        /// </summary>
        private void CheckCurrentConfiguration()
        {
            EnableAlertingCheckBox.IsChecked = configurationManagementPack.GetPackUpdateMonitorSetting();
            CheckForUpdatesNow.IsEnabled = (bool)EnableAlertingCheckBox.IsChecked;
        }

        /// <summary>
        /// Apply the currently selected settings to the Management Pack
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ApplySettings_Click(object sender, RoutedEventArgs e)
        {
            StatusLabel.Content = "Applying override settings, please wait.";
            this.UpdateLayout();
            configurationManagementPack.ModifyPackUpdateMonitorSetting((bool)EnableAlertingCheckBox.IsChecked);
            CheckCurrentConfiguration();
            StatusLabel.Content = "Override settings applied successfully.";
        }

        /// <summary>
        /// Check for 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CheckForUpdatesNow_Click(object sender, RoutedEventArgs e)
        {
            ResetStatusLabel.Content = string.Empty;
            ResetStatusLabel.Content = "Performing check for updated management packs, please wait.";
            MonitoringState packStatus = GetPackUpdateMonitorState();
            packStatus.Reset(15000);
            packStatus.Recalculate();
            ResetStatusLabel.Content = "Management pack update check has completed.";
        }

        /// <summary>
        /// Using the static ManagementGroupConnector this method pulls the monitoring state 
        /// for the Community Pack Update Monitor
        /// </summary>
        /// <returns>The <see cref="MonitoringState"/> object that represents the Community Pack Update Monitor.</returns>
        private MonitoringState GetPackUpdateMonitorState()
        {
            // Filter to the Root Management Server Emulator role, there is only one instance of this class so the criteria can be simple.
            ManagementPackClass rootManagementServerClass = ManagementGroupConnector.CurrentManagementGroup.EntityTypes.GetClass(Guid.Parse("1a9387f0-6fe5-5527-a2cb-73f2f6be6bc7"));
            MonitoringObjectGenericCriteria monitoringObjectCriteria = new MonitoringObjectGenericCriteria("FullName like 'Microsoft.SystemCenter%'");
            IObjectReader<MonitoringObject> monitoringObjects = ManagementGroupConnector.CurrentManagementGroup.EntityObjects.GetObjectReader<MonitoringObject>(monitoringObjectCriteria, rootManagementServerClass, ObjectQueryOptions.Default);
            MonitoringObject rootManagementServerInstance = monitoringObjects.GetData(0);

            // For the instance of the Root Management Server Emulator we need to cross that with the Monitor to get the exact monitoring state item
            ManagementPackMonitorCriteria monitorsCriteria = new ManagementPackMonitorCriteria("Name = 'Community.ManagementPackCatalog.PackStatusMonitor'");
            ManagementPackMonitor updateManagementPackMonitor = ManagementGroupConnector.CurrentManagementGroup.Monitoring.GetMonitors(monitorsCriteria)[0];

            // Creating a list of one item we pull the current monitoring state of the Update Status monitor
            var monitorList = new List<ManagementPackMonitor>();
            monitorList.Add(updateManagementPackMonitor);
            MonitoringState packStatus = rootManagementServerInstance.GetMonitoringStates((IEnumerable<ManagementPackMonitor>)monitorList)[0];

            return packStatus;
        }
    }
}
