// -----------------------------------------------------------------------
// <copyright file="InstalledCatalogPacks.cs">
// This is part of the Community Management Pack Catalog and licensed GPL v3; See https://github.com/mpcatalog/scom-community-catalog-ui/blob/master/LICENSE.
// </copyright>
// -----------------------------------------------------------------------

namespace Community.ManagementPackCatalog.UI
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.Design;
    using System.Globalization;
    using System.Windows.Forms;
    using Community.ManagementPackCatalog.UI.Models;
    using Microsoft.EnterpriseManagement.Configuration;
    using Microsoft.EnterpriseManagement.ConsoleFramework;
    using Microsoft.EnterpriseManagement.Mom.Internal.UI;
    using Microsoft.EnterpriseManagement.Mom.Internal.UI.Administration.MPInstall;
    using Microsoft.EnterpriseManagement.Mom.Internal.UI.Cache;
    using Microsoft.EnterpriseManagement.Mom.Internal.UI.Common;
    using Microsoft.EnterpriseManagement.Mom.Internal.UI.Controls;
    using Microsoft.EnterpriseManagement.Mom.Internal.UI.MPPages;
    using Microsoft.EnterpriseManagement.Mom.UI;

    using static Community.ManagementPackCatalog.UI.LogManager;

    /// <summary>
    /// The core class of the Community Management Pack Catalog UI
    /// The InstalledCatalogPacks inherits the GridViewBased used by multiple other SCOM views.
    /// </summary>
    internal class InstalledCatalogPacks : GridViewBase<GitHubPackDetail, DataConnectors.InstalledCatalogQuery>
    {
        /// <summary>
        /// Used to keep track of activities and progress
        /// </summary>
        private ShowProgressHelper progressIndicator;

        /// <summary>
        /// The MPSheet is used to display the properties page when someone double clicks on an MP
        /// </summary>
        private MPSheet propertiesSheet;

        /// <summary>
        /// Initializes a new instance of the <see cref="InstalledCatalogPacks"/> class
        /// </summary>
        /// <param name="container">Container in which to create the instance</param>
        public InstalledCatalogPacks(System.ComponentModel.IContainer container) : base(container, null)
        {
            Grid.MultiSelect = false;
            UseRowContextMenu = true;

            Log.WriteTrace(
               EventType.UIActivity,
               "Created new instance of InstalledCatalogPacks");
        }

        /// <summary>
        /// Overrides the base class to give this view a distinct name
        /// </summary>
        public override string ViewName
        {
            get
            {
                return "Community Management Pack Catalog";
            }
        }

        /// <summary>
        /// Add columns to the DataGrid and tie them to our data source using the GitHub translator
        /// </summary>
        protected override void AddColumns()
        {
            Field displayNameField = new Field("ManagementPackDisplayName", typeof(string), false, Field.SortInfos.Sort);
            DataGridViewColumn nameColumn = AddColumn("Name", displayNameField, true);
            nameColumn.Width = 400;

            Field availableVersionField = new Field("VersionString", typeof(string), false, Field.SortInfos.Sortable);
            AddColumn("Available Version", availableVersionField, true);

            Field installedVersionField = new Field("InstalledVersion", typeof(string), false, Field.SortInfos.Sortable);
            AddColumn("Installed Version", installedVersionField, true);

            Field versionStatusField = new Field("VersionStatus", typeof(string), false, Field.SortInfos.Sortable);
            AddColumn("Status", versionStatusField, true);

            Field authorField = new Field("Author", typeof(string), false, Field.SortInfos.Sortable);
            AddColumn("Author", authorField, true);

            Field urlField = new Field("URL", typeof(string), false, Field.SortInfos.Sortable);
            AddColumn("URL", urlField, true);

            Log.WriteTrace(
               EventType.UIActivity,
               "Declared and Instantiated GridView fields.");
        }

        /// <summary>
        /// Called by the base class in part of the build out
        /// this method adds the Tasks option to the right-hand pane in the SCOM console.
        /// </summary>
        protected override void AddActions()
        {
            // Add the Catalog Custom tasks to the Right-hand tasks pane.
            AddTaskItem(TaskCommands.ActionsTaskGroup, ManagementPackCatalogCommands.ViewURL, new EventHandler<CommandEventArgs>(ShowManagementPackSourceURL));
            AddTaskSeparatorItem("Catalog Actions", TaskCommands.ActionsTaskGroup);

            // Activity related to the Properties Task on the Right-hand side of the console
            AddTaskItem(TaskCommands.ActionsTaskGroup, ViewCommands.Properties);
            CommandID properties = ViewCommands.Properties;
            InstalledCatalogPacks installedCatalogPacks1 = this;
            InstalledCatalogPacks installedCatalogPacks2 = this;
            CommandHelpers.RegisterForNotification(properties, new EventHandler<CommandEventArgs>(installedCatalogPacks1.OnPropertiesCommand), new EventHandler<CommandStatusEventArgs>(installedCatalogPacks2.UpdatePropertiesCommandStatus));

            // Activity related to the Delete Task on the Right-hand side of the console
            AddTaskItem(TaskCommands.ActionsTaskGroup, MomViewCommands.DeleteSelectedViewItem);
            CommandHelpers.RegisterForNotification(StandardCommands.Delete, new EventHandler<CommandEventArgs>(OnDelete), new EventHandler<CommandStatusEventArgs>(OnStatusDelete));
            CommandHelpers.RegisterForNotification(MomViewCommands.DeleteSelectedViewItem, new EventHandler<CommandEventArgs>(OnDelete), new EventHandler<CommandStatusEventArgs>(OnStatusDelete));

            // This help isn't specific to the community packs.
            HelpKey = "Management Packs";
            base.AddActions();

            Log.WriteTrace(
               EventType.UIActivity,
               "Added Task Actions to the base instance.");
        }

        /// <summary>
        /// Additional context menu items we are not utilizing currently.
        /// </summary>
        /// <param name="contextMenu">context menu to add items too</param>
        protected override void AddContextMenu(ContextMenuHelper contextMenu)
        {
            // We are not using this Context Menu, the base class still calls this method.
        }

        /// <summary>
        /// Configures and adds items to the Right-Click contextual menu.
        /// </summary>
        /// <param name="contextMenu">menu to work with</param>
        /// <param name="data">GridView Data</param>
        protected override void AddRowContextMenu(ContextMenuHelper contextMenu, GitHubPackDetail data)
        {
            // Catalog Contextual Right-Click for the Catalog
            contextMenu.AddContextMenuItem(ManagementPackCatalogCommands.ViewURL, new EventHandler<CommandEventArgs>(ShowManagementPackSourceURL));

            // Contextual Right-Click Properties
            CommandID properties = ViewCommands.Properties;
            InstalledCatalogPacks installedCatalogPacks1 = this;
            InstalledCatalogPacks installedCatalogPacks2 = this;
            contextMenu.AddContextMenuSeparator();
            contextMenu.AddContextMenuItem(properties, new EventHandler<CommandEventArgs>(installedCatalogPacks1.OnPropertiesCommand), new EventHandler<CommandStatusEventArgs>(installedCatalogPacks2.UpdatePropertiesCommandStatus));

            // Contextual Right-Click Delete
            contextMenu.AddContextMenuSeparator();
            contextMenu.AddContextMenuItem(StandardCommands.Delete, new EventHandler<CommandEventArgs>(OnDelete), new EventHandler<CommandStatusEventArgs>(OnStatusDelete));

            Log.WriteTrace(
               EventType.UIActivity,
               "Context Menu items added to the base GridView.");
        }

        /// <summary>
        /// Uses the standard SCOM personalization options to store user preferences
        /// </summary>
        protected override void ApplyPersonalization()
        {
            if (ColumnCollection != null && ColumnCollection.Count > 0)
            {
                ColumnCollection.Apply(Grid);
                return;
            }

            if (Configuration != null)
            {
                ColumnCollection = ViewSupport.XmlToColumnInfoCollection(Configuration.Presentation);
                ColumnCollection.Apply(Grid);
            }
        }

        /// <summary>
        /// Handles a request for the management pack properties
        /// if the management pack is not installed the user is directed to the Website
        /// </summary>
        /// <param name="sender">Sending DataGrid</param>
        /// <param name="args">Command Arguments</param>
        protected override void OnPropertiesCommand(object sender, CommandEventArgs args)
        {
            base.OnPropertiesCommand(sender, args);
            Cursor cursor = Cursor;
            Cursor = Cursors.WaitCursor;
            GitHubPackDetail gitHubPack = GridSelectedItem as GitHubPackDetail;
            ManagementPack selectedMP = gitHubPack.InstalledManagementPack;

            if (selectedMP == null)
            {
                string notificationMessage = string.Format(
                    "{0} is not currently installed.\nManagement Pack Properties are only available on installed packs.",
                    gitHubPack.ManagementPackSystemName);

                DialogResult dialogResult = MessageBox.Show(notificationMessage, "This Management Pack is not currently installed.", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MPPropertiesData managementPackPropertiesData = new MPPropertiesData(selectedMP, ManagementGroup);
                propertiesSheet = new MPSheet(Container, managementPackPropertiesData);
                propertiesSheet.Show();
                Cursor = cursor;
            }
        }

        /// <summary>
        /// Updates the status of our available actions, for example if an item can be deleted or not.
        /// </summary>
        protected override void UpdateActionStatus()
        {
            base.UpdateActionStatus();
            UpdateCommandStatus(ViewCommands.Properties);
            UpdateCommandStatus(StandardCommands.Delete);
            UpdateCommandStatus(MomViewCommands.DeleteSelectedViewItem);
        }

        /// <summary>
        /// Can the Properties command be used on this object?
        /// </summary>
        /// <param name="sender">sending DataGrid</param>
        /// <param name="args">Command Status Arguments</param>
        protected override void UpdatePropertiesCommandStatus(object sender, CommandStatusEventArgs args)
        {
            args.CommandStatus.Visible = true;
            args.CommandStatus.Enabled = Grid.SelectedRows.Count == 1;
            if (args.CommandStatus.Enabled)
            {
                GridDataItem tag = (GridDataItem)Grid.SelectedRows[0].Cells[0].Tag;
                if (tag == null || tag.IsHeader)
                {
                    args.CommandStatus.Enabled = false;
                }
            }
        }

        /// <summary>
        /// Dispose method called on exit
        /// </summary>
        /// <param name="disposing">Indicates if we are currently disposing.</param>
        protected override void Dispose(bool disposing)
        {
            Log.WriteTrace(
               EventType.UIActivity,
               "Disposing the InstalledCatalogPacks");

            base.Dispose(disposing);
        }

        /// <summary>
        /// Starts the Progress of a tracked item.
        /// </summary>
        private void StartProgressForm()
        {
            EndProgressForm();
            progressIndicator = new ShowProgressHelper();
            progressIndicator.ShowOperation(null, "Please Wait, we're doing things", ShowProgressHelper.TimeOutConfig.TimeoutIn120Seconds);
        }

        /// <summary>
        /// Called when the Progress track operation is over
        /// </summary>
        private void EndProgressForm()
        {
            if (progressIndicator != null)
            {
                progressIndicator.CloseProgressDialog();
                progressIndicator.Dispose();
                progressIndicator = null;
            }
        }

        /// <summary>
        /// Called when a user requests to view the web site or URL for a community management pack
        /// </summary>
        /// <param name="sender">sending GridView item</param>
        /// <param name="e">Event Arguments</param>
        private void ShowManagementPackSourceURL(object sender, CommandEventArgs e)
        {
            if (Grid.SelectedRows == null || Grid.SelectedRows.Count <= 0)
            {
                MessageBox.Show("Please select a Management Pack from the Catalog.");
                return;
            }

            Cursor cursor = Cursor;
            Cursor = Cursors.WaitCursor;
            GitHubPackDetail gridSelectedItem = GridSelectedItem as GitHubPackDetail;

            string confirmMessageForUrl = string.Format("Would you like to proceed to the below URL?\n{0}", gridSelectedItem.URL);
            DialogResult dialogResult = MessageBox.Show(confirmMessageForUrl, "Open URL?", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
            if (dialogResult == DialogResult.Yes)
            {
                // User OK'd going to the URL, open it in the preferred browser.
                Log.WriteTrace(
                   EventType.ExternalDependency,
                   "Opening URL in new windows.",
                   gridSelectedItem.URL);

                System.Diagnostics.Process.Start(gridSelectedItem.URL);
            }
            else if (dialogResult == DialogResult.No)
            {
                // no action requested, we'll return back to the list
                return;
            }
        }

        /// <summary>
        /// Deletes the requested management pack if possible.
        /// The user is prompted to confirm the action.
        /// </summary>
        /// <param name="sender">GridView Data being acted upon</param>
        /// <param name="e">Event Arguments</param>
        private void OnDelete(object sender, CommandEventArgs e)
        {
            ManagementPack gridSelectedItem = (GridSelectedItem as GitHubPackDetail).InstalledManagementPack;
            ICollection<ManagementPack> dependentManagementPacks = SDKHelper.GetDependentManagementPacks(this, gridSelectedItem);
            if (dependentManagementPacks == null)
            {
                return;
            }

            if (dependentManagementPacks.Count > 0)
            {
                (new MPDeleteStatusDialog(dependentManagementPacks)).ShowDialog(this);
                return;
            }

            if (!UninstallConfirmed(gridSelectedItem))
            {
                return;
            }

            StartProgressForm();
            if (!SDKHelper.UnInstallManagementPack(this, ManagementGroup, gridSelectedItem).JobSucceeded)
            {
                MessageBoxOptions messageBoxOption = (MessageBoxOptions)0;
                if (RightToLeft == RightToLeft.Yes)
                {
                    messageBoxOption = MessageBoxOptions.RightAlign | MessageBoxOptions.RtlReading;
                }

                IApplicationInfo service = (IApplicationInfo)GetService(typeof(IApplicationInfo));
                MessageBox.Show(this, "The MP Delete Failed", service.ProductTitle, MessageBoxButtons.OK, MessageBoxIcon.Hand, MessageBoxDefaultButton.Button1, messageBoxOption);
            }

            ConsoleJobs.RunJob(this, (object sender2, ConsoleJobEventArgs e2) => UpdateCache(), new object[0]);
            EndProgressForm();
        }

        /// <summary>
        /// Determines if we should allow the Delete button or Grey it out.
        /// </summary>
        /// <param name="sender">Sending DataGrid Row</param>
        /// <param name="e">Command Arguments</param>
        private void OnStatusDelete(object sender, CommandStatusEventArgs e)
        {
            if (GetGridSelection().Count == 1)
            {
                // If there is only one item selected, we need to make sure that it is installed before we attempt to delete it.
                GitHubPackDetail selectedPack = GridSelectedItem as GitHubPackDetail;
                if (selectedPack.InstalledManagementPack == null)
                {
                    e.CommandStatus.Enabled = false;
                }
                else
                {
                    e.CommandStatus.Enabled = true;
                }

                return;
            }

            e.CommandStatus.Enabled = false;
        }

        /// <summary>
        /// Prior toe successful deletion of a management pack the user is prompted for confirmation.
        /// </summary>
        /// <param name="mp">Management Pack that is going to be deleted.</param>
        /// <returns>Verification Boolean if the pack can be removed.</returns>
        private bool UninstallConfirmed(ManagementPack mp)
        {
            MessageBoxOptions messageBoxOption = (MessageBoxOptions)0;
            if (RightToLeft == RightToLeft.Yes)
            {
                messageBoxOption = MessageBoxOptions.RightAlign | MessageBoxOptions.RtlReading;
            }

            CultureInfo invariantCulture = CultureInfo.InvariantCulture;
            string confirmMPDelete = "Are you sure you would like to delete this Community Pack?\nThe pack will be removed from your Management Group, but it will remain available in the catalog if you change your mind.";
            object[] managementPackDisplayName = new object[] { MPUtils.GetMPDisplayName(this, mp), mp.Version.ToString() };
            if (MessageBox.Show(this, string.Format(invariantCulture, confirmMPDelete, managementPackDisplayName), ((IApplicationInfo)GetService(typeof(IApplicationInfo))).ProductTitle, MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2, messageBoxOption) == DialogResult.No)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Modifies the Command Status
        /// </summary>
        /// <param name="commandID">Command to update</param>
        private void UpdateCommandStatus(CommandID commandID)
        {
            RegisteredCommand registeredCommand = CommandService.Find(commandID);
            if (registeredCommand != null)
            {
                registeredCommand.UpdateStatus(Grid, this);
            }
        }
    }
}