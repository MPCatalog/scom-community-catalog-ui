// -----------------------------------------------------------------------
// <copyright file="GitHubCatalogTranslator.cs">
// This is part of the Community Management Pack Catalog and licensed GPL v3; See https://github.com/mpcatalog/scom-community-catalog-ui/blob/master/LICENSE.
// </copyright>
// -----------------------------------------------------------------------

namespace Community.ManagementPackCatalog.UI.DataConnectors
{
    using System;
    using Microsoft.EnterpriseManagement.Mom.Internal.UI.Cache;
    using static Community.ManagementPackCatalog.UI.LogManager;

    /// <summary>
    /// GitHubCatalogTranslator inherits from the PropertyTranslator class
    /// and is used to connect typed data items to a DataGrid via Named Fields.
    /// </summary>
    internal class GitHubCatalogTranslator : PropertyTranslator
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GitHubCatalogTranslator"/> class.
        /// </summary>
        public GitHubCatalogTranslator()
        {
        }

        /// <summary>
        /// Resolve a property value given an Object and tag
        /// </summary>
        /// <param name="tag">Identifier of the data to pull</param>
        /// <param name="dataItem">The Object from which to pull the data.</param>
        /// <returns>The value matched by the tag.</returns>
        public override object GetProperty(object tag, object dataItem)
        {
            string requestedPropery = tag as string;
            if (string.IsNullOrEmpty(requestedPropery))
            {
                Log.WriteError(
                    EventType.UIActivity,
                    "Received an Empty tag when requesting a property");
                throw new ArgumentException();
            }

            // Type our model
            Models.GitHubPackDetail gitHubPackDetails = dataItem as Models.GitHubPackDetail;

            string extractedPropertyString = string.Empty;
            switch (requestedPropery)
            {
                case "Author":
                    extractedPropertyString = gitHubPackDetails.Author;
                    break;

                case "ManagementPackDisplayName":
                    extractedPropertyString = gitHubPackDetails.ManagementPackDisplayName;
                    break;

                case "VersionStatus":
                    if (gitHubPackDetails.Version > gitHubPackDetails.InstalledManagementPack.Version)
                    {
                        extractedPropertyString = "Update available";
                    }
                    else
                    {
                        extractedPropertyString = "Installed";
                    }

                    break;

                case "VersionString":
                    extractedPropertyString = gitHubPackDetails.VersionString;
                    break;

                case "InstalledVersion":
                    extractedPropertyString = gitHubPackDetails.InstalledVersion;
                    break;

                case "URL":
                    extractedPropertyString = gitHubPackDetails.URL;
                    break;

                case "ManagementPackSystemName":
                    extractedPropertyString = gitHubPackDetails.ManagementPackSystemName;
                    break;

                default:
                    extractedPropertyString = "N/A";
                    break;
            }

            Log.WriteTrace(
                EventType.UIActivity,
                "Extracted Property from Detail Row",
                requestedPropery + " == " + extractedPropertyString);

            return extractedPropertyString;
        }

        /// <summary>
        /// unimplemented method exposed via the base class
        /// </summary>
        /// <param name="name">The parameter is not used.</param>
        /// <returns>The method is not used.</returns>
        public override object GetPropertyTag(string name)
        {
            // This is required by the PropertyTranslator base class, but we don't want to hit it.
            throw new InvalidOperationException();
        }
    }
}