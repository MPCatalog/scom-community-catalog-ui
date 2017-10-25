// -----------------------------------------------------------------------
// <copyright file="GitHubPackTag.xaml.cs">
// This is part of the Community Management Pack Catalog and licensed GPL v3; See https://github.com/mpcatalog/scom-community-catalog-ui/blob/master/LICENSE.
// </copyright>
// -----------------------------------------------------------------------

namespace Community.ManagementPackCatalog.UI.WpfElements
{
    using System;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Media;

    /// <summary>
    /// Interaction logic for GitHubPackTag.xaml
    /// </summary>
    public partial class GitHubPackTag : UserControl
    {
        /// <summary>
        /// Is the pack currently active in the search filter.
        /// </summary>
        private bool isSelected;

        /// <summary>
        /// Initializes a new instance of the <see cref="GitHubPackTag"/> class
        /// An instance represents a single tag to display in the UI
        /// </summary>
        /// <param name="tagName">Text if the Tag</param>
        /// <param name="startWithRemovalX">Parameter has been depreciated</param>
        public GitHubPackTag(string tagName, bool startWithRemovalX = false)
        {
            InitializeComponent();
            TagName = tagName;
            FilterToTag.Content = TagName;
        }

        /// <summary>
        /// Raised when the User requests this tag removed from search.
        /// </summary>
        public event EventHandler<string> FilterTagRemoved;

        /// <summary>
        /// Raised when the user requests this tag added to search.
        /// </summary>
        public event EventHandler<string> FilterTagSelected;

        /// <summary>
        /// Gets or sets the text of the tag represented by the UI element
        /// </summary>
        public string TagName { get; set; }

        /// <summary>
        /// Processes the active search text to determine if the tag is active in search.
        /// </summary>
        /// <param name="sender">The object that triggered a search update</param>
        /// <param name="searchText">Currently active search text.</param>
        internal void HighlightIfTagExistsInSearch(object sender, string searchText)
        {
            if (searchText.ToLower().Contains(TagName.ToLower()))
            {
                // This Tag is in the Search!
                ContentBorder.Background = SystemColors.HighlightBrush;
                isSelected = true;
            }
            else
            {
                // This Tag is not in Search
                ContentBorder.Background = SystemColors.ControlDarkBrush;
                isSelected = false;
            }
        }

        /// <summary>
        /// Removal of tag from search clicked
        /// </summary>
        /// <param name="sender">The <see cref="GitHubPackTag"/> removed.</param>
        /// <param name="e">Event Arguments</param>
        private void RemoveTag_Click(object sender, RoutedEventArgs e)
        {
            // Only attempt to raise the event if there is something listening
            if (FilterTagRemoved != null)
            {
                FilterTagRemoved.Invoke(this, TagName);
            }
        }

        /// <summary>
        /// Body of the tag has been clicked.  This will add the tag, and if currently selected will remove it.
        /// </summary>
        /// <param name="sender">The <see cref="GitHubPackTag"/> added.</param>
        /// <param name="e">Event Arguments</param>
        private void FilterToTag_Click(object sender, RoutedEventArgs e)
        {
            // This tag is selected, remove it from the list to act like a toggle
            if (!isSelected)
            {
                // Only attempt to raise the event if there is something listening
                if (FilterTagSelected != null)
                {
                    FilterTagSelected.Invoke(this, TagName);
                }
            }
            else
            {
                if (FilterTagRemoved != null)
                {
                    FilterTagRemoved.Invoke(this, TagName);
                }
            }
        }
    }
}