// -----------------------------------------------------------------------
// <copyright file="MarkdownViewer.xaml.cs">
// This is part of the Community Management Pack Catalog and licensed GPL v3; See https://github.com/mpcatalog/scom-community-catalog-ui/blob/master/LICENSE.
// </copyright>
// -----------------------------------------------------------------------

namespace Community.ManagementPackCatalog.UI.WpfElements
{
    using System.Windows.Controls;
    using static Community.ManagementPackCatalog.UI.LogManager;

    /// <summary>
    /// Interaction logic for MarkdownViewer.xaml
    /// </summary>
    public partial class MarkdownViewer : UserControl
    {
        private string markdownText;

        /// <summary>
        /// Initializes a new instance of the <see cref="MarkdownViewer"/> class
        /// </summary>
        public MarkdownViewer()
        {
            InitializeComponent();
            MarkdownBrowserView.Navigating += MarkdownBrowserView_Navigating;
        }

        /// <summary>
        /// Clears out the control when there is no text to display.
        /// </summary>
        public void ClearMarkdown()
        {
            MarkdownBrowserView.NavigateToString("<br/>");
        }

        /// <summary>
        /// Render the provided markdown text in the window.
        /// </summary>
        /// <param name="markdownText">MarkDown text to display</param>
        public void DisplayMarkdown(string markdownText)
        {
            this.markdownText = markdownText;
            string htmlToRender = Markdig.Markdown.ToHtml(markdownText);
            htmlToRender = "<style>h1 { font-family: verdana;font-size: 130%;}h2 { font-family: verdana;font-size: 120%;}h3 { font-family: verdana;font-size: 110%;}h4 { font-family: verdana;font-size: 100%;}h5 { font-family: verdana;font-size: 100%;}h6 { font-family: verdana;font-size: 100%;}p { font-family: verdana;font-size: 85%;}li { font-family: verdana;font-size: 85%;}</style>" + htmlToRender;
            if (!string.IsNullOrWhiteSpace(htmlToRender))
            {
                MarkdownBrowserView.NavigateToString(htmlToRender);
            }
        }

        /// <summary>
        /// This method catches the navigation of the browser window an opens it externally.
        /// </summary>
        /// <param name="sender">The WebBrowser that triggered the event.</param>
        /// <param name="e">Event Arguments</param>
        private void MarkdownBrowserView_Navigating(object sender, System.Windows.Navigation.NavigatingCancelEventArgs e)
        {
            if (e.Uri != null)
            {
                string newDestinationUri = e.Uri.OriginalString;
                e.Cancel = true;
                Log.WriteTrace(
                     EventType.ExternalDependency,
                     "Redirecting URL from Markdown browser in new window.",
                     newDestinationUri);

                System.Diagnostics.Process.Start(newDestinationUri);
            }
        }
    }
}