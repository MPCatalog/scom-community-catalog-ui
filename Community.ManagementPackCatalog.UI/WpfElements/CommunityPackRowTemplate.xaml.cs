// -----------------------------------------------------------------------
// <copyright file="CommunityPackRowTemplate.xaml.cs">
// This is part of the Community Management Pack Catalog and licensed GPL v3; See https://github.com/mpcatalog/scom-community-catalog-ui/blob/master/LICENSE.
// </copyright>
// -----------------------------------------------------------------------

namespace Community.ManagementPackCatalog.UI.WpfElements
{
    using System;
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Media.Imaging;
    using Community.ManagementPackCatalog.UI.DataConnectors;
    using Community.ManagementPackCatalog.UI.Models;
    using static Community.ManagementPackCatalog.UI.LogManager;

    /// <summary>
    /// Interaction logic for CommunityPackRowTemplate.xaml
    /// </summary>
    public partial class CommunityPackRowTemplate : UserControl
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CommunityPackRowTemplate"/> class
        /// </summary>
        /// <param name="communityPackToDisplay">What pack the UI Element should display.</param>
        public CommunityPackRowTemplate(GitHubPackDetail communityPackToDisplay)
        {
            InitializeComponent();
            this.CommunityPackDisplayedInTemplate = communityPackToDisplay;

            Log.WriteTrace(
                    EventType.UIActivity,
                    "CommunityPack Display Template Row Created",
                    CommunityPackDisplayedInTemplate.ManagementPackSystemName);

            DataContext = this;

            AddDetailsToPanel();
            IndicateCommunityPackInstalledStatus();
        }

        /// <summary>
        /// This event is raised when one of the Filter Tags on this Row is selected by a user.
        /// </summary>
        public event EventHandler<string> RowFilterTagSelected;

        /// <summary>
        /// Raised when a user removes one of the active filter tags from this row.
        /// </summary>
        public event EventHandler<string> RowFilterTagRemoved;

        /// <summary>
        /// Raised when the Author's name has been clicked on the row.
        /// </summary>
        public event EventHandler<string> RowAuthorSelected;

        /// <summary>
        /// When the Search text and query change this Event is raised to indicate the UI Element
        /// should check to see if the <see cref="CommunityPackDisplayedInTemplate"/> matches the query.
        /// </summary>
        internal event EventHandler<string> CommunityPackSearchUpdated;

        /// <summary>
        /// Gets or sets the GitHubPackDetail class instance that is represented by this UI Element
        /// </summary>
        public GitHubPackDetail CommunityPackDisplayedInTemplate { get; set; }

        /// <summary>
        /// When the search is updated this method starts the process to update the visibility of the UI Element
        /// </summary>
        /// <param name="sender">The object which triggered a SearchString update</param>
        /// <param name="searchString">The new string to search for</param>
        internal void SearchUpdated(object sender, string searchString)
        {
            if (CheckIfVisibleInThisSearch(searchString))
            {
                this.Visibility = Visibility.Visible;
            }
            else
            {
                this.Visibility = Visibility.Collapsed;
            }

            if (CommunityPackSearchUpdated != null)
            {
                CommunityPackSearchUpdated.Invoke(this, searchString);
            }
        }

        /// <summary>
        /// For each tag represented in the pack, create a matching tag in the UI.
        /// </summary>
        internal void PopulateTagsOnRow()
        {
            string[] tags = CommunityPackDisplayedInTemplate?.Tags?.ToArray();
            if (tags != null)
            {
                foreach (string tag in tags)
                {
                    Log.WriteTrace(
                          EventType.UIActivity,
                          "Creating tag on CommunityPack Display Template",
                          tag + " created for " + CommunityPackDisplayedInTemplate.ManagementPackSystemName);

                    GitHubPackTag newGitHubPackTag = new GitHubPackTag(tag);
                    newGitHubPackTag.FilterTagSelected += RowFilterTagSelected;
                    newGitHubPackTag.FilterTagRemoved += RowFilterTagRemoved;
                    this.CommunityPackSearchUpdated += newGitHubPackTag.HighlightIfTagExistsInSearch;
                    TagList.Children.Add(newGitHubPackTag);
                }
            }
        }

        /// <summary>
        /// Populates the Expanding section of the UI element with the MarkDown data from ReadMe.md
        /// </summary>
        private void AddDetailsToPanel()
        {
            if (CommunityPackDisplayedInTemplate.ReadMeMarkdown == null || string.IsNullOrWhiteSpace(CommunityPackDisplayedInTemplate.ReadMeMarkdown))
            {
                AdditionalDetailsExpander.Visibility = Visibility.Collapsed;
            }
            else
            {
                AdditionalDetailsExpandedData.MarkdownDisplay.DisplayMarkdown(CommunityPackDisplayedInTemplate.ReadMeMarkdown);
            }

            if (string.IsNullOrEmpty(CommunityPackDisplayedInTemplate.Description))
            {
                ManagementPackDescriptionLabel.Visibility = Visibility.Collapsed;
            }
            else
            {
                ManagementPackDescriptionLabel.Visibility = Visibility.Visible;
            }
        }

        /// <summary>
        /// When called this method updates the UI appearance of the pack if it is installed
        /// </summary>
        private void IndicateCommunityPackInstalledStatus()
        {
            if (ManagementGroupConnector.IsManagementPackInstalled(CommunityPackDisplayedInTemplate.ManagementPackSystemName))
            {
                // This management pack is installed
                ManagementPackDisplayNameLabel.Content = CommunityPackDisplayedInTemplate.ManagementPackDisplayName + " (Currently Installed)";
                this.IsEnabled = false;
                this.Opacity = .4;
            }
        }

        /// <summary>
        /// Determines the correct visibility of the element given the Search String
        /// </summary>
        /// <param name="searchString">Search text to compare against this Management Pack</param>
        /// <returns>A boolean indicating if this pack should be visible in the search results</returns>
        private bool CheckIfVisibleInThisSearch(string searchString)
        {
            bool searchResult = GitHubPackDataSearch.DoesPackMatchSearchString(CommunityPackDisplayedInTemplate, searchString);
            if (searchResult)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Raises the event indicating that a request was made to filter to the author.
        /// </summary>
        /// <param name="sender">The AuthorLinkButton</param>
        /// <param name="e">Event Arguments</param>
        private void AuthorLinkButton_Click(object sender, RoutedEventArgs e)
        {
            RowAuthorSelected?.Invoke(sender, CommunityPackDisplayedInTemplate.Author);
        }

        /// <summary>
        /// Raises the event indicating the a request was made to view the pack online
        /// </summary>
        /// <param name="sender">The ViewPackOnline button</param>
        /// <param name="e">Event Arguments</param>
        private void ViewPackOnlineClicked(object sender, RoutedEventArgs e)
        {
            Log.WriteTrace(
                              EventType.ExternalDependency,
                              "Navigating to the community pack Online.",
                              CommunityPackDisplayedInTemplate.URL);

            System.Diagnostics.Process.Start(CommunityPackDisplayedInTemplate.URL);
        }
    }
}