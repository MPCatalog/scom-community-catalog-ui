// -----------------------------------------------------------------------
// <copyright file="GitHubPackDataSearch.cs">
// This is part of the Community Management Pack Catalog and licensed GPL v3; See https://github.com/mpcatalog/scom-community-catalog-ui/blob/master/LICENSE.
// </copyright>
// -----------------------------------------------------------------------

namespace Community.ManagementPackCatalog.UI.DataConnectors
{
    using System.Collections.Generic;
    using System.Linq;
    using Community.ManagementPackCatalog.UI.Models;
    using static Community.ManagementPackCatalog.UI.LogManager;

    /// <summary>
    /// This Static class is used by the Community Management Pack UI Elements to
    /// determine if the Management Pack Represented by the UI should be displayed.
    /// </summary>
    internal static class GitHubPackDataSearch
    {
        /// <summary>
        /// Checks against Tags, Display Name, System Name, and Author.
        /// Searches are not case sensitive.
        /// </summary>
        /// <param name="packToSearch">The <see cref="GitHubPackDetail"/> class instance to search against.</param>
        /// <param name="searchText">The search string to verify.</param>
        /// <returns>Boolean representing if the pack is a match for the search</returns>
        public static bool DoesPackMatchSearchString(GitHubPackDetail packToSearch, string searchText)
        {
            return MatchesAuthor(searchText, packToSearch) ||
                MatchesDisplayOrSystemName(searchText, packToSearch) ||
                FoundExactTagMatchForAllTags(searchText, packToSearch);
        }

        /// <summary>
        /// Checks for the presence of a single Tag in a List or tags.
        /// </summary>
        /// <param name="tagTextLower">Lowercase Text to be searched for</param>
        /// <param name="tagList">The List to search against</param>
        /// <param name="packSystemName">What packs is currently being tested.</param>
        /// <returns>A boolean representing if an EXACT match was found in the Tag List</returns>
        private static bool CheckForSingleTagInTagList(string tagTextLower, IList<string> tagList, string packSystemName)
        {
            if (tagList == null)
            {
                // No list was provided, there cannot be a match
                return false;
            }

            int matchingTagCount = tagList.Where(tag =>
                                           tag.ToLower() == tagTextLower).Count();

            if (matchingTagCount > 0)
            {
                Log.WriteTrace(
                    EventType.UIActivity,
                    "Tag matched for GitHub Management Pack",
                    tagTextLower + " matched for pack " + packSystemName);
                return true;
            }
            else
            {
                Log.WriteTrace(
                    EventType.UIActivity,
                    "No tag matches for GitHub Management Pack",
                    tagTextLower + " did not match on pack " + packSystemName);
                return false;
            }
        }

        /// <summary>
        /// Searches for a match of ALL searched tags.
        /// </summary>
        /// <param name="searchText">The text we are looking</param>
        /// <param name="packToMatch">The GitHubPackDetail object to search.</param>
        /// <returns>A boolean representing if ALL of the tags searched returned an EXACT match</returns>
        private static bool FoundExactTagMatchForAllTags(string searchText, GitHubPackDetail packToMatch)
        {
            searchText = searchText.ToLower();

            // If there is a comma and multiple tags we must match all of them
            if (searchText.Contains(","))
            {
                foreach (string searchTag in searchText.Split(','))
                {
                    if (!CheckForSingleTagInTagList(searchTag.Trim(), packToMatch.Tags, packToMatch.ManagementPackSystemName))
                    {
                        // If any one tag is matching from the list of tags this isn't a match
                        return false;
                    }
                }

                Log.WriteTrace(
                    EventType.UIActivity,
                    "Matched all tags on Search String",
                    searchText);

                // Every tag in the search string was found in the Tags on the Pack
                return true;
            }
            else
            {
                // A Single tag was in the search, perform one check.
                return CheckForSingleTagInTagList(searchText, packToMatch.Tags, packToMatch.ManagementPackSystemName);
            }
        }

        /// <summary>
        /// Checks to see if the search text matches the Author's name
        /// </summary>
        /// <param name="searchText">The text we are looking</param>
        /// <param name="packToMatch">The GitHubPackDetail object to search.</param>
        /// <returns>Boolean representing if the Author was a match for the search.</returns>
        private static bool MatchesAuthor(string searchText, GitHubPackDetail packToMatch)
        {
            searchText = searchText.ToLower();
            return packToMatch.Author.ToLower().Contains(searchText);
        }

        /// <summary>
        /// Checks to see if the search text matches the Display or System name of the pack.
        /// </summary>
        /// <param name="searchText">The text we are looking</param>
        /// <param name="packToMatch">The GitHubPackDetail object to search.</param>
        /// <returns>Boolean representing if the Display or System name was a match for the search.</returns>
        private static bool MatchesDisplayOrSystemName(string searchText, GitHubPackDetail packToMatch)
        {
            searchText = searchText.ToLower();
            return packToMatch.ManagementPackDisplayName.ToLower().Contains(searchText) ||
                    packToMatch.ManagementPackSystemName.ToLower().Contains(searchText);
        }
    }
}