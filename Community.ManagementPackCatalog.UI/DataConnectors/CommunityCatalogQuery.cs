// -----------------------------------------------------------------------
// <copyright file="CommunityCatalogQuery.cs">
// This is part of the Community Management Pack Catalog and licensed GPL v3; See https://github.com/mpcatalog/scom-community-catalog-ui/blob/master/LICENSE.
// </copyright>
// -----------------------------------------------------------------------

namespace Community.ManagementPackCatalog.UI.DataConnectors
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Community.ManagementPackCatalog.UI.Models;
    using Microsoft.EnterpriseManagement.Mom.Internal.UI.Cache;

    /// <summary>
    /// The CommunityCatalogQuery class is an extension of the Cache Query,
    /// this class ties directly to a MOM UI DataGrid and provides the data via cache or direct load.
    /// </summary>
    internal class CommunityCatalogQuery : Query<GitHubPackDetail>, ISerialization
    {
        /// <summary>
        /// the Connection to the GitHub Repository
        /// </summary>
        private GitHubRepository communityManagementPackIndex;

        /// <summary>
        /// Initializes a new instance of the <see cref="CommunityCatalogQuery"/> class.
        /// </summary>
        public CommunityCatalogQuery()
        {
            PropertyTranslator = new GitHubCatalogTranslator();
            Grouped = true;
        }

        /// <summary>
        /// Deserialize takes a byte array that represents a key to a DataGrid item.
        /// the Key then pulls the full object and returns it to the DataGrid base.
        /// </summary>
        /// <param name="bytes">An Array of bytes representing a GitHub Pack Key</param>
        /// <returns>A strongly typed object matching the key.</returns>
        public object Deserialize(byte[] bytes)
        {
            string managementPackSystemName = System.Text.Encoding.UTF8.GetString(bytes, 0, bytes.Length);
            CheckAndFillGitHubIndex();

            return communityManagementPackIndex.ManagementPacks[managementPackSystemName];
        }

        /// <summary>
        /// Matching the Deserialize method this is used to serialize the Key of an object
        /// into a byte array.  The array can later be deserialized by the matching method.
        /// </summary>
        /// <param name="data">Inbound Object to serialize the key of.</param>
        /// <returns>a byte array representing the input object's key</returns>
        public byte[] Serialize(object data)
        {
            GitHubPackDetail typedData = (GitHubPackDetail)data;
            byte[] buffer = System.Text.Encoding.UTF8.GetBytes(typedData.ManagementPackSystemName);

            return buffer;
        }

        /// <summary>
        /// Retrieves a collection of items to display on the DataGrid.
        /// </summary>
        /// <param name="criteria">This Parameter is not used.</param>
        /// <returns>A Collection of Community Packs</returns>
        protected override ICollection<GitHubPackDetail> DoQuery(string criteria)
        {
            if (ManagementGroup == null)
            {
                return null;
            }

            CheckAndFillGitHubIndex();

            // Return only those packs in the index that DO NOT have an InstalledManagementPack
            return communityManagementPackIndex.ManagementPacks
                .Where(mp => mp.Value.InstalledManagementPack == null)
                .Select(imp => imp.Value).ToList();
        }

        /// <summary>
        /// Pull the Management Pack ID from a GitHubPackDetail
        /// </summary>
        /// <param name="data">A GitHubPackDetail object from the UI</param>
        /// <returns>The ID of the Matching Management Pack</returns>
        protected override Guid GetId(GitHubPackDetail data)
        {
            if (data.InstalledManagementPack != null)
            {
                return data.InstalledManagementPack.Id;
            }
            else
            {
                return Guid.Empty;
            }
        }

        /// <summary>
        /// Confirm that the <see cref="communityManagementPackIndex"/> variable is properly populated.
        /// If the value is null the data will be re-fetched from GitHub
        /// </summary>
        private void CheckAndFillGitHubIndex()
        {
            if (communityManagementPackIndex != null)
            {
                // We've got data and we can proceeded
                return;
            }
            else
            {
                // Loop through our GitHub packs an populate a Installed Pack (if applicable)
                communityManagementPackIndex = new Models.GitHubRepository();
                Task.Run(() => communityManagementPackIndex.PopulateDataFromRepository("InstalledPacks")).Wait();

                var managementPackInventory = ManagementGroup.ManagementPacks.GetManagementPacks();
                foreach (GitHubPackDetail communityPack in communityManagementPackIndex.ManagementPacks.Values)
                {
                    communityPack.InstalledManagementPack = managementPackInventory.Where(mp => mp.Name == communityPack.ManagementPackSystemName).FirstOrDefault();

                    if (communityPack.InstalledManagementPack != null)
                    {
                        LogManager.Log.WriteTrace(
                            LogManager.EventType.ExternalDependency,
                            "Found Matching Management Pack",
                            communityPack.ManagementPackSystemName);
                    }
                }
            }
        }
    }
}