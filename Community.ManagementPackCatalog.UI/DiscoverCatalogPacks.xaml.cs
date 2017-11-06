// -----------------------------------------------------------------------
// <copyright file="DiscoverCatalogPacks.xaml.cs">
// This is part of the Community Management Pack Catalog and licensed GPL v3; See https://github.com/mpcatalog/scom-community-catalog-ui/blob/master/LICENSE.
// </copyright>
// -----------------------------------------------------------------------

namespace Community.ManagementPackCatalog.UI
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Controls;
    using Community.ManagementPackCatalog.UI.Models;
    using Community.ManagementPackCatalog.UI.WpfElements;
    using static Community.ManagementPackCatalog.UI.LogManager;

    /// <summary>
    /// Interaction logic for DiscoverCatalogPacks.xaml file
    /// </summary>
    public partial class DiscoverCatalogPacks : UserControl
    {
        /// <summary>
        /// The communityManagementPackIndex that is displayed within the UI
        /// </summary>
        private GitHubRepository communityManagementPackIndex;

        /// <summary>
        /// Initializes a new instance of the <see cref="DiscoverCatalogPacks"/> class.
        /// This is the base UI element that SCOM will call.
        /// </summary>
        public DiscoverCatalogPacks()
        {
            InitializeComponent();
            SearchString = string.Empty;
            DataContext = this;
            PopulateCommunityManagementPackListFromGitHub(communityManagementPackIndex?.ManagementPacks.Values.ToArray());
            PopulateSuggestedTagsFromGitHub();
        }

        /// <summary>
        /// Event is raised whenever a change to the search criteria is made.
        /// </summary>
        protected event EventHandler<string> ManagementPackSearchChanged;

        /// <summary>
        /// Gets or sets the active search criteria of the Discovery View
        /// </summary>
        public string SearchString { get; set; }

        /// <summary>
        /// Gets or sets a list of tags to display in the top of the search pane as recommendations.
        /// </summary>
        public List<string> SuggestedTagsList { get; set; }

        /// <summary>
        /// Checks to see if all of the packs in the list are collapsed, if they are inform the user.
        /// </summary>
        private void VerifyIfAllPacksAreCollapsed()
        {
            // SearchTextBox.Text = SearchString;
            var visiblePackCount = CommunityPackList.Items.OfType<CommunityPackRowTemplate>().Where(cprt => cprt.Visibility == Visibility.Visible).Count();

            // If there are no visible packs, politely inform the user of such.
            if (visiblePackCount == 0)
            {
                CommunityPackList.Visibility = Visibility.Collapsed;
                NoResultsSearchMessage.Visibility = Visibility.Visible;
            }
            else
            {
                CommunityPackList.Visibility = Visibility.Visible;
                NoResultsSearchMessage.Visibility = Visibility.Collapsed;
            }
        }

        /// <summary>
        /// Called when a user selects a tag on one of the <see cref="CommunityPackRowTemplate"/>
        /// </summary>
        /// <param name="sender">The clicked <see cref="CommunityPackRowTemplate"/></param>
        /// <param name="tagSelected">Value of the tag clicked.</param>
        private void FilterTagSelectedByUser(object sender, string tagSelected)
        {
            // Clear the search text
            SearchTextBox.Text = string.Empty;

            if (string.IsNullOrWhiteSpace(SearchString))
            {
                SearchString += tagSelected;
            }
            else
            {
                SearchString += "," + tagSelected;
            }

            // Remove any double commas left from the Tag manipulations
            SearchString = SearchString.Replace(",,", ",");

            ManagementPackSearchChanged?.Invoke(this, SearchString);
            VerifyIfAllPacksAreCollapsed();
        }

        /// <summary>
        /// Based on a <see cref="GitHubPackDetail"/> array, this method builds out the displayed list of packs.
        /// </summary>
        /// <param name="packsToDisplay">An optional array of <see cref="GitHubPackDetail"/>.  Providing NULL will fetch the list from GitHub</param>
        private void PopulateCommunityManagementPackListFromGitHub(GitHubPackDetail[] packsToDisplay)
        {
            if (packsToDisplay == null || communityManagementPackIndex == null)
            {
                communityManagementPackIndex = new Models.GitHubRepository();
                Task.Run(() => communityManagementPackIndex.PopulateDataFromRepository("DiscoverPacks")).Wait();
                packsToDisplay = communityManagementPackIndex.ManagementPacks?.Values.ToArray();
            }

            // Empty the list prior to re-populating
            CommunityPackList.Items.Clear();

            // If there are no packs in the list, discovery will be blank.
            if (packsToDisplay == null)
            {
                return;
            }

            foreach (GitHubPackDetail communityPack in packsToDisplay.OrderBy(mp => mp.ManagementPackDisplayName))
            {
                CommunityPackRowTemplate packListTemplateDisplay = new CommunityPackRowTemplate(communityPack);

                // Before Adding the Tags, we pass the Tag Selection Event handles through
                packListTemplateDisplay.RowFilterTagSelected += FilterTagSelectedByUser;
                packListTemplateDisplay.RowFilterTagRemoved += FilterTagRemovedByUser;
                packListTemplateDisplay.RowAuthorSelected += FilterAuthorSelected;

                // Now populate the tags, they will connect to the above handles.
                packListTemplateDisplay.PopulateTagsOnRow();
                this.ManagementPackSearchChanged += packListTemplateDisplay.SearchUpdated;
                CommunityPackList.Items.Add(packListTemplateDisplay);
            }
        }

        /// <summary>
        /// One of the <see cref="CommunityPackRowTemplate"/> instances has raised an event
        /// indicating the search should be filtered to an Author.
        /// </summary>
        /// <param name="sender">The Sending Object</param>
        /// <param name="selectedAuthor">Author to filter to</param>
        private void FilterAuthorSelected(object sender, string selectedAuthor)
        {
            // When an author is selected the search is cleared and set to only the author.
            SearchString = selectedAuthor;
            SearchTextBox.Text = selectedAuthor;
            ManagementPackSearchChanged?.Invoke(sender, selectedAuthor);
            VerifyIfAllPacksAreCollapsed();
        }

        /// <summary>
        /// Using the list of GitHub recommended tags, this method creates instances
        /// of the <see cref="GitHubPackTag"/> class for display.
        /// </summary>
        private void PopulateSuggestedTagsFromGitHub()
        {
            var sortedTags = communityManagementPackIndex.RecommendedSearchTags?.Select(tag => tag.ToLower()).OrderBy(tag => tag).ToList();
            foreach (string tag in sortedTags)
            {
                Log.WriteTrace(
                            EventType.UIActivity,
                            "Creating tag on Recommended List",
                            tag);

                GitHubPackTag newGitHubPackTag = new GitHubPackTag(tag);
                newGitHubPackTag.FilterTagSelected += FilterTagSelectedByUser;
                newGitHubPackTag.FilterTagRemoved += FilterTagRemovedByUser;
                this.ManagementPackSearchChanged += newGitHubPackTag.HighlightIfTagExistsInSearch;
                RecommendedTags.Children.Add(newGitHubPackTag);
            }
        }

        /// <summary>
        /// One of the <see cref="CommunityPackRowTemplate"/> instances has raised an event
        /// indicating removal of a Search Tag.
        /// </summary>
        /// <param name="sender">The sending Object</param>
        /// <param name="removedTag">Tag to remove from search</param>
        private void FilterTagRemovedByUser(object sender, string removedTag)
        {
            // Remove the requested tag from the search string
            SearchString = SearchString.Replace(removedTag, string.Empty);

            // Remove any double commas left from the Tag removal
            SearchString = SearchString.Replace(",,", ",");

            // Clean up leading or trailing commas
            SearchString = SearchString.Trim(',');

            ManagementPackSearchChanged?.Invoke(this, SearchString);
            VerifyIfAllPacksAreCollapsed();
        }

        /// <summary>
        /// Triggered by the change of the text in the search box, this starts the process of updating the management packs in the list.
        /// </summary>
        /// <param name="sender">The sending TextBox</param>
        /// <param name="e"><see cref="TextChangedEventArgs"/> from the event</param>
        private void SearchText_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Text Box is Selected, Update the SearchString
            SearchString = SearchTextBox.Text;
            ManagementPackSearchChanged?.Invoke(this, SearchString);
            VerifyIfAllPacksAreCollapsed();
        }

        /// <summary>
        /// Clear the search box and start fresh
        /// </summary>
        /// <param name="sender">The Object requesting the SearchString be reset.</param>
        /// <param name="e"><see cref="RoutedEventArgs"/> from the event</param>
        private void ResetSearchState_Click(object sender, RoutedEventArgs e)
        {
            SearchString = string.Empty;
            SearchTextBox.Text = string.Empty;

            // After reseting the string, notify any attached handlers
            ManagementPackSearchChanged?.Invoke(this, SearchString);
            VerifyIfAllPacksAreCollapsed();
        }

        /// <summary>
        /// Modifies the list of packs when the user selects Paid, Free, or All Packs.
        /// </summary>
        /// <param name="sender">Tab selecting the packs to display.</param>
        /// <param name="e"><see cref="RoutedEventArgs"/> from the event</param>
        private void PaidOrFreeFilterSelected(object sender, RoutedEventArgs e)
        {
            // Reset Search and then Change the Population of the List
            SearchString = string.Empty;
            SearchTextBox.Text = string.Empty;

            switch ((sender as TabItem).Name)
            {
                case "AllPacks":
                    PopulateCommunityManagementPackListFromGitHub(communityManagementPackIndex.ManagementPacks.Values.ToArray());
                    break;

                case "FreePacks":
                    PopulateCommunityManagementPackListFromGitHub(communityManagementPackIndex.ManagementPacks.Values.Where(pack => pack.IsFree).ToArray());
                    break;

                case "PaidPacks":
                    PopulateCommunityManagementPackListFromGitHub(communityManagementPackIndex.ManagementPacks.Values.Where(pack => !pack.IsFree).ToArray());
                    break;
            }

            // Update our search and display after the catalog change
            ManagementPackSearchChanged?.Invoke(this, SearchString);
            VerifyIfAllPacksAreCollapsed();
        }

        /// <summary>
        /// Directs the user to the Additional Information URL
        /// </summary>
        /// <param name="sender">The clicked link</param>
        /// <param name="e"><see cref="RoutedEventArgs"/> from the event</param>
        private void AdditionalInformationLinkClicked(object sender, RoutedEventArgs e)
        {
            string informationURL = "http://mpcatalog.net/TermsAndConditions";

            Log.WriteTrace(
                   EventType.ExternalDependency,
                   "Navigating to the More Information URL.",
                   informationURL);

            System.Diagnostics.Process.Start(informationURL);
        }
    }
}