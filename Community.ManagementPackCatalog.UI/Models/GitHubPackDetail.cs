// -----------------------------------------------------------------------
// <copyright file="GitHubPackDetail.cs">
// This is part of the Community Management Pack Catalog and licensed GPL v3; See https://github.com/mpcatalog/scom-community-catalog-ui/blob/master/LICENSE.
// </copyright>
// -----------------------------------------------------------------------

namespace Community.ManagementPackCatalog.UI.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.EnterpriseManagement.Configuration;
    using Newtonsoft.Json;

    /// <summary>
    /// The GitHubPackDetail class represents a serializable object with the details on a Community Management Pack
    /// </summary>
    public class GitHubPackDetail
    {
        private IList<string> managementPackTags;

        /// <summary>
        /// Initializes a new instance of the <see cref="GitHubPackDetail"/> class
        /// Empty constructor for JSON and the GitHubCatalogTranslator
        /// </summary>
        [JsonConstructor]
        public GitHubPackDetail()
        {
        }

        /// <summary>
        /// Gets or sets the ManagementPackSystemName
        /// The ManagementPackSystemName is the same as found in SCOM.
        /// This provides the key link between this item and the SCOM Management Group.
        /// </summary>
        [JsonRequired]
        [JsonProperty("ManagementPackSystemName")]
        public string ManagementPackSystemName { get; set; }

        /// <summary>
        /// Gets or sets the ManagementPackDisplayName
        /// ManagementPackDisplayName should match that of the Management Pack, though no code requires it.
        /// </summary>
        [JsonRequired]
        [JsonProperty("ManagementPackDisplayName")]
        public string ManagementPackDisplayName { get; set; }

        /// <summary>
        /// Gets or sets the URL of the Community Management Pack
        /// URL is provided by the Management Pack author and links to where the MP can be downloaded
        /// </summary>
        [JsonRequired]
        [JsonProperty("URL")]
        public string URL { get; set; }

        /// <summary>
        /// Gets or sets the Version Property
        /// The VersionString Property allows JSON.Net to de-serialize to the correct property without a cast.
        /// </summary>
        [JsonRequired]
        [JsonProperty("Version")]
        public string VersionString
        {
            get
            {
                return Version.ToString();
            }

            set
            {
                // Using the string intake as it will be more human readable and does not require VersionConverter
                // https://www.newtonsoft.com/json/help/html/T_Newtonsoft_Json_Converters_VersionConverter.htm
                Version = new Version(value);
            }
        }

        /// <summary>
        /// Gets or sets the Author
        /// The Author's name will be displayed so users can find other packs from that Author.
        /// </summary>
        [JsonRequired]
        [JsonProperty("Author")]
        public string Author { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the author is Commercial or not.
        /// This value will default to false unless set
        /// </summary>
        [JsonProperty("CommercialAuthor", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public bool CommercialAuthor { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this is a Free or a Paid Management Pack
        /// </summary>
        [JsonRequired]
        [JsonProperty("IsFree")]
        public bool IsFree { get; set; }

        /// <summary>
        /// Gets or sets the description for this management pack
        /// </summary>
        [JsonProperty("Description")]
        public string Description { get; set; }

        /// <summary>
        /// Gets a truncated version of the Description.
        /// </summary>
        [JsonIgnore]
        public string ShortDescription
        {
            get
            {
                if (Description != null && Description?.Length > 105)
                {
                    return Description?.Substring(0, 100) + "...";
                }
                else
                {
                    return Description;
                }
            }
        }

        /// <summary>
        /// Gets or sets the Tags for the Management Pack
        /// These tags are used for searching and filtering in the Discovery UI
        /// </summary>
        [JsonProperty("Tags")]
        public IList<string> Tags
        {
            get
            {
                // Tags are always used as lower case, ordered alphabetically.
                // Case conversion is performed on Get to prevent possible errors on Set with JSON.Net
                return managementPackTags?.Select(tag => tag.ToLower()).OrderBy(tag => tag).ToList();
            }

            set
            {
                managementPackTags = value;
            }
        }

        /// <summary>
        /// Gets the typed Version
        /// Version is a strongly typed representation of the Version String.  The cast occurs at de-serialization and will throw an error on invalid version.
        /// </summary>
        [JsonIgnore]
        public Version Version { get; private set; }

        /// <summary>
        /// Gets or sets the Installed Management Pack, if available.
        /// </summary>
        [JsonIgnore]
        public ManagementPack InstalledManagementPack { get; set; }

        /// <summary>
        /// Gets A string representation of the installed MP version
        /// </summary>
        [JsonIgnore]
        public string InstalledVersion
        {
            get
            {
                if (InstalledManagementPack == null)
                {
                    return "Not Installed";
                }
                else
                {
                    return InstalledManagementPack.Version.ToString();
                }
            }
        }

        /// <summary>
        /// Gets or sets the ReadMe.md markdown text found on GitHub.
        /// </summary>
        [JsonIgnore]
        public string ReadMeMarkdown { get; set; }
    }
}