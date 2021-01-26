// -----------------------------------------------------------------------
// <copyright file="GitHubIndex.cs">
// This is part of the Community Management Pack Catalog and licensed GPL v3; See https://github.com/mpcatalog/scom-community-catalog-ui/blob/master/LICENSE.
// </copyright>
// -----------------------------------------------------------------------

namespace Community.ManagementPackCatalog.UI.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Threading.Tasks;
    using Newtonsoft.Json;
    using static LogManager;

    /// <summary>
    /// The GitHubIndex class manages and fetches the data from the JSON files hosted on the GitHub Repository.
    /// </summary>
    public class GitHubRepository
    {
        /// <summary>
        /// This constructor initialises the class and sets TLS version to 1.2
        /// </summary>
        public GitHubRepository ()
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
        }

        /// <summary>
        /// The working set of Management Pack Details can only be modified from inside the class, the Public Property is GET only.
        /// </summary>
        private Dictionary<string, GitHubPackDetail> managementPackDetailedEntries;

        /// <summary>
        /// This is the one configuration item that indicates where the Community MP data lives.
        /// Everything else is build off of this base.
        /// </summary>
        private string gitHubRepoBase;

        /// <summary>
        /// The HttpClient class can be re-used across threads and calls, this will be the instance we use for the Index and Detail data.
        /// </summary>
        private HttpClient gitHubHttpClient;

        /// <summary>
        /// Gets the Management Pack Detailed information.
        /// The ManagementPacks property is GET only and returns only those packs that are enabled.
        /// </summary>>
        public Dictionary<string, GitHubPackDetail> ManagementPacks
        {
            get
            {
                return managementPackDetailedEntries;
            }
        }

        /// <summary>
        /// Represents the tags to suggest on the UI
        /// </summary>
        public string[] RecommendedSearchTags { get; private set; }

        /// <summary>
        /// Asynchronously performs the population of the Repository from the index.
        /// </summary>
        /// <returns>a Threading Task for the status of Populating the Index</returns>
        public async Task PopulateDataFromRepository(string callingModuleName)
        {
            try
            {
                // Populate class fields with the values in the resources file
                await PopulateClassFieldsFromResources(callingModuleName);
                string managementPackIndexUrl = gitHubRepoBase + "Index.json";
                string recommendedTagsJsonFile = gitHubRepoBase + "RecommendedSearchTags.json";

                // we'll use a single client for the data pull, but no reason for it to stay open once we have what we need.
                using (gitHubHttpClient = new HttpClient())
                {
                    // 20 seconds should do for these small JSON Docs
                    gitHubHttpClient.Timeout = new TimeSpan(0, 0, 20);
                    gitHubHttpClient.DefaultRequestHeaders.Clear();
                    gitHubHttpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("plain/text"));
                    gitHubHttpClient.DefaultRequestHeaders.Add("User-Agent", "Community");

                    // Fetch the Management Pack list, then we'll iterate through it and get the pack details.
                    List<GitHubIndexEntry> indexResults = await GetManagementPackIndexUsingHttp(managementPackIndexUrl);
                    managementPackDetailedEntries = await GetManagementPackDetailsUsingHttp(indexResults);
                    RecommendedSearchTags = await GetRecommendedSearchTags(recommendedTagsJsonFile);
                }
            }
            catch (Exception ex)
            {
                Log.WriteError(
                        EventType.ExternalDependency,
                        "Failed to create the GitHubIndex Object",
                        ex.Message,
                        501);
                throw;
            }
        }

        /// <summary>
        /// Populates the RecommendedSearchTags property with data returned from the provided URL
        /// </summary>
        /// <param name="recommendedTagsJsonFile">Location of the file containing the properties</param>
        /// <returns></returns>
        private async Task<string[]> GetRecommendedSearchTags(string recommendedTagsJsonFile)
        {
            try
            {
                var recommendedStrings = await gitHubHttpClient.GetStringAsync(recommendedTagsJsonFile);
                dynamic results = JsonConvert.DeserializeObject(recommendedStrings);
                return results.RecommendedTags.ToObject<string[]>();
            }
            catch (Exception ex)
            {
                // Non fatal, return an empty list of tags.
                Log.WriteWarning(
                       EventType.ExternalDependency,
                       "Failed to fetch Recommended Tags Object",
                       ex.Message,
                       601);

                return (new List<string>()).ToArray();
            }
        }

        /// <summary>
        /// Populates the Resource Settings from their storage location.
        /// </summary>
        private async Task PopulateClassFieldsFromResources(string refererName)
        {
            // Need to choose a resource method, AppConfig isn't an option for SCOM
            gitHubRepoBase = "";
            string catalogRedirectUrl = "http://www.mpcatalog.net/CatalogRepo";

            // Don't Follow our redirects when found
            HttpClientHandler httpClientHandler = new HttpClientHandler();
            httpClientHandler.AllowAutoRedirect = false;

            // Using the catalogRedirectUrl we pull the actual destination of the index.
            using (gitHubHttpClient = new HttpClient(httpClientHandler))
            {
                gitHubHttpClient.DefaultRequestHeaders.Referrer = new Uri("HTTP://" + refererName);

                // Pull the base URL from our redirect
                var fetchResults = await gitHubHttpClient.GetAsync(catalogRedirectUrl);
                if(fetchResults.StatusCode == System.Net.HttpStatusCode.Moved || fetchResults.StatusCode == System.Net.HttpStatusCode.Redirect)
                {
                    gitHubRepoBase = fetchResults.Headers.Location.ToString();
                }
                else
                {
                    throw new Exception("Unable to resolve the index location from " + catalogRedirectUrl);
                }

                // Test to see if this is a second redirect, we follow a max of two redirects
                fetchResults = null;
                fetchResults = await gitHubHttpClient.GetAsync(gitHubRepoBase);
                if (fetchResults.StatusCode == System.Net.HttpStatusCode.Moved || fetchResults.StatusCode == System.Net.HttpStatusCode.Redirect)
                {
                    gitHubRepoBase = fetchResults.Headers.Location.ToString();
                }
            }
        }

        /// <summary>
        /// Using the list from the Index this method pulls the details of each individual pack.
        /// </summary>
        /// <param name="managementPackIndex">The list of GitHub entries to pull details from.</param>
        /// <returns>A dictionary of Management Pack Details keyed by their SystemName</returns>
        private async Task<Dictionary<string, GitHubPackDetail>> GetManagementPackDetailsUsingHttp(List<GitHubIndexEntry> managementPackIndex)
        {
            if (managementPackIndex == null)
            {
                throw new InvalidOperationException("Management Pack Details cannot be processed until the Index is fetched.");
            }

            Dictionary<string, GitHubPackDetail> fetchedPackDetails = new Dictionary<string, GitHubPackDetail>();
            List<Task<GitHubPackDetail>> detailFetchTasks = new List<Task<GitHubPackDetail>>();

            foreach (GitHubIndexEntry managementPackEntry in managementPackIndex.Where(mp => mp.Active == true))
            {
                string detailsJsonFileLocation = gitHubRepoBase + managementPackEntry.ManagementPackSystemName + "/details.json";
                string readMeFileLocation = gitHubRepoBase + managementPackEntry.ManagementPackSystemName + "/ReadMe.md";
                detailFetchTasks.Add(FetchPackDetails(detailsJsonFileLocation, readMeFileLocation));
            }

            // When all of the fetches have completed, create a dictionary and return it to the caller.
            GitHubPackDetail[] packsList = await Task.WhenAll(detailFetchTasks.ToArray());
            return packsList
                .Where(pack => pack != null)
                .ToDictionary(pack => pack.ManagementPackSystemName, pack => pack);
        }

        /// <summary>
        /// Fetches the details of a single JSON Management Pack File.
        /// </summary>
        /// <param name="detailsJsonFileLocation">The URL of the Management File to pull</param>
        /// <param name="readMeFileLocation">The URL of the ReadMe markdown file</param>
        /// <returns>A Treading Task with a CLR object representing the Pack fetched via HTTP.</returns>
        private async Task<GitHubPackDetail> FetchPackDetails(string detailsJsonFileLocation, string readMeFileLocation)
        {
            // Each of these tasks will include a Try/Catch, if a detail file fails we'd like to know which one
            try
            {
                Log.WriteTrace(
                EventType.ExternalDependency,
                "Fetching Management Pack Details",
                detailsJsonFileLocation);

                string detailsString = await gitHubHttpClient.GetStringAsync(detailsJsonFileLocation);

                // We'll attempt to deserialize before logging success
                GitHubPackDetail deserializedPackData = JsonConvert.DeserializeObject<GitHubPackDetail>(detailsString);

                Log.WriteTrace(
                    EventType.ExternalDependency,
                    "Successfully Fetched Management Pack Details",
                    detailsString);

                try
                {
                    deserializedPackData.ReadMeMarkdown = await gitHubHttpClient.GetStringAsync(readMeFileLocation);
                }
                catch (Exception)
                {
                    // If we cannot fetch the ReadMe we will make sure the value is null and continue.
                    deserializedPackData.ReadMeMarkdown = null;
                }

                return deserializedPackData;
            }
            catch (Exception ex)
            {
                Log.WriteWarning(
                    EventType.ExternalDependency,
                    "Failed at Fetching Management Pack Details at: " + detailsJsonFileLocation,
                    ex.Message);

                // If we fail to get one pack, should we throw an exception or hide it?
                // throw new Exception("Failed when retrieving Management Pack Details",ex);
                return null;
            }
        }

        /// <summary>
        /// Asynchronously pulls the list of entries via HTTP Get
        /// </summary>
        /// <param name="managementPackIndexUrl">URL Location of the Management Pack List</param>
        /// <returns>A Threading Task representing the progress and status of the Pack List Fetch</returns>
        private async Task<List<GitHubIndexEntry>> GetManagementPackIndexUsingHttp(string managementPackIndexUrl)
        {
            Log.WriteTrace(
                    EventType.ExternalDependency,
                    "Fetching Management Pack Index",
                    managementPackIndexUrl);

            List<GitHubIndexEntry> indexEntries;
            HttpResponseMessage packListResponse = await gitHubHttpClient.GetAsync(managementPackIndexUrl);
            indexEntries = JsonConvert.DeserializeObject<List<GitHubIndexEntry>>(await packListResponse.Content.ReadAsStringAsync());

            Log.WriteTrace(
                    EventType.ExternalDependency,
                    "Successfully Fetched Management Pack Index",
                    "A list of " + indexEntries.Count.ToString() + " Management Pack Entries was successfully fetched from the URL.");

            return indexEntries;
        }

        /// <summary>
        /// This private nested class should only be used within the GitHubIndex parent class.
        /// Any interaction with the data that exists in the GitHubIndex should be performed through properties of the parent.
        /// https://docs.microsoft.com/en-us/dotnet/standard/design-guidelines/nested-types
        /// </summary>
        private class GitHubIndexEntry
        {
            /// <summary>
            /// Defaulting to true, this is our simple Active boolean.
            /// </summary>
            private bool isMPActive = true;

            /// <summary>
            /// Gets or sets the ManagementPackSystemName  property
            /// The MP's SystemName will be used to locate the details.json file, and as a key to the MP
            /// </summary>
            [JsonProperty("ManagementPackSystemName")]
            public string ManagementPackSystemName { get; set; }

            /// <summary>
            /// Gets or sets a value indicating whether the Index Entry is Active
            /// Simple boolean to control if something should be hidden from the UI
            /// </summary>
            [JsonProperty("Active")]
            public bool Active
            {
                get { return isMPActive; }
                set { isMPActive = value; }
            }
        }
    }
}