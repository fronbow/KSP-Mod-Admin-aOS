﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using HtmlAgilityPack;
using KSPModAdmin.Core.Controller;
using KSPModAdmin.Core.Model;
using KSPModAdmin.Core.Views;

namespace KSPModAdmin.Core.Utils.SiteHandler
{
    /// <summary>
    /// Handles the GetModInfo and Mod download for mods on GitHub.
    /// </summary>
    public class GitHubHandler : ISiteHandler
    {
        private const string NAME = "GitHub";
        private const string HOST = "github.com";
        private const string URL_0_1 = "https://github.com/{0}/{1}";
        private const string HOST2 = "raw.githubusercontent.com";

        /// <summary>
        /// Builds the url from the passed user and project name.
        /// </summary>
        /// <param name="userName">Name of the user from the GitHub repository.</param>
        /// <param name="projectName">Name of the project from the GitHub repository.</param>
        /// <returns>The build GitHub project URL or empty string.</returns>
        public static string GetProjectUrl(string userName, string projectName)
        {
            if (!string.IsNullOrEmpty(userName) && !string.IsNullOrEmpty(projectName))
                return string.Format(URL_0_1, userName, projectName);

            return string.Empty;
        }

        /// <summary>
        /// Gets the Name of the ISiteHandler.
        /// </summary>
        /// <returns>The Name of the ISiteHandler.</returns>
        public string Name { get { return NAME; } }

        /// <summary>
        /// Checks if the passed URL is a valid URL for GitHub.
        /// </summary>
        /// <param name="url">The URL to check.</param>
        /// <returns>True if the passed URL is a valid URL, otherwise false.</returns>
        public bool IsValidURL(string url)
        {
            return !string.IsNullOrEmpty(url) && (HOST.Equals(new Uri(url).Authority) || HOST2.Equals(new Uri(url).Authority));
        }

        /// <summary>
        /// Gets the content of the site of the passed URL and parses it for ModInfos.
        /// </summary>
        /// <param name="url">The URL of the site to parse the ModInfos from.</param>
        /// <returns>The ModInfos parsed from the site of the passed URL.</returns>
        public ModInfo GetModInfo(string url)
        {
            var parts = GetUrlParts(url);
            var modInfo = new ModInfo
            {
                SiteHandlerName = Name,
                ModURL = ReduceToPlainUrl(url),
                Name = parts[3],
                Author = parts[2]
            };
            ParseSite(ref modInfo);
            return modInfo;
        }

        /// <summary>
        /// Handles a mod add via URL.
        /// Validates the URL, gets ModInfos, downloads mod archive, adds it to the ModSelection and installs the mod if selected.
        /// </summary>
        /// <param name="url">The URL to the mod.</param>
        /// <param name="modName">The name for the mod.</param>
        /// <param name="install">Flag to determine if the mod should be installed after adding.</param>
        /// <param name="downloadProgressCallback">Callback function for download progress.</param>
        /// <returns>The root node of the added mod, or null.</returns>
        public ModNode HandleAdd(string url, string modName, bool install, DownloadProgressCallback downloadProgressCallback = null)
        {
            ModInfo modInfo = GetModInfo(url);
            if (modInfo == null)
                return null;

            if (!string.IsNullOrEmpty(modName))
                modInfo.Name = modName;

            ModNode newMod = null;
            if (DownloadMod(ref modInfo, downloadProgressCallback))
                newMod = ModSelectionController.HandleModAddViaModInfo(modInfo, install);

            return newMod;
        }

        /// <summary>
        /// Checks if updates are available for the passed mod.
        /// </summary>
        /// <param name="modInfo">The ModInfos of the mod to check for updates.</param>
        /// <param name="newModInfo">A reference to an empty ModInfo to write the updated ModInfos to.</param>
        /// <returns>True if there is an update, otherwise false.</returns>
        public bool CheckForUpdates(ModInfo modInfo, ref ModInfo newModInfo)
        {
            newModInfo = GetModInfo(modInfo.ModURL);
            if (string.IsNullOrEmpty(modInfo.Version) && !string.IsNullOrEmpty(newModInfo.Version))
                return true;
            else if (!string.IsNullOrEmpty(modInfo.Version) && !string.IsNullOrEmpty(newModInfo.Version))
                return (VersionComparer.CompareVersions(modInfo.Version, newModInfo.Version) == VersionComparer.Result.AisSmallerB);
            else if (string.IsNullOrEmpty(modInfo.CreationDate) && !string.IsNullOrEmpty(newModInfo.CreationDate))
                return true;
            else if (!string.IsNullOrEmpty(modInfo.CreationDate) && !string.IsNullOrEmpty(newModInfo.CreationDate))
                return modInfo.CreationDateAsDateTime < newModInfo.CreationDateAsDateTime;

            return false;
        }

        /// <summary>
        /// Downloads the mod.
        /// </summary>
        /// <param name="modInfo">The infos of the mod. Must have at least ModURL and LocalPath</param>
        /// <param name="downloadProgressCallback">Callback function for download progress.</param>
        /// <returns>True if the mod was downloaded.</returns>
        public bool DownloadMod(ref ModInfo modInfo, DownloadProgressCallback downloadProgressCallback = null)
        {
            if (modInfo == null)
                return false;

            var downloadInfos = GetDownloadInfo(modInfo);
            DownloadInfo selected = null;

            if (downloadInfos.Count > 1)
            {
                // create new selection form if more than one download option found
                var dlg = new frmSelectDownload(downloadInfos);
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    selected = dlg.SelectedLink;
                }
            }
            else if (downloadInfos.Count == 1)
            {
                selected = downloadInfos.First();
            }
            else
            {
                string msg = string.Format(Messages.MSG_NO_BINARY_DOWNLOAD_FOUND_AT_0, modInfo.SiteHandlerName);
                MessageBox.Show(msg, Messages.MSG_TITLE_ERROR);
                Messenger.AddDebug(msg);
                return false;
            }

            if (selected != null)
            {
                string downloadUrl = selected.DownloadURL;
                modInfo.LocalPath = Path.Combine(OptionsController.DownloadPath, selected.Filename);
                Www.DownloadFile(downloadUrl, modInfo.LocalPath, downloadProgressCallback);
            }

            return File.Exists(modInfo.LocalPath);
        }

        /// <summary>
        /// Takes a site url and parses the site for mod info
        /// </summary>
        /// <param name="modInfo">The modInfo to add data to</param>
        public void ParseSite(ref ModInfo modInfo)
        {
            var htmlDoc = new HtmlWeb().Load(GetPathToReleases(modInfo.ModURL));
            htmlDoc.OptionFixNestedTags = true;

            // get the node that contains all releases (indepandend of label or tags view)
            // Easy way to get XPATH search: use chrome, inspect element, highlight the needed data, right-click and copy XPATH
            var timeLineNode = htmlDoc.DocumentNode.SelectSingleNode("//*[@id=\"js-repo-pjax-container\"]/div[2]/div[1]/div[2]");
            var lableNode = timeLineNode.SelectSingleNode("//*[@class=\"release label-latest\"]");
            var tagsNode = timeLineNode.SelectSingleNode("//*[@class=\"release-timeline-tags\"]");

            if (lableNode == null && tagsNode == null)
            {
                Messenger.AddError("Error! Can't parse GitHib version or creation date!");
                return;
            }

            // get data from label or tag view.
            HtmlNode versionNode = null;
            HtmlNode updateNode = null;
            if (lableNode != null)
            {
                versionNode = lableNode.SelectSingleNode("//*[@class='release label-latest']/div/ul/li/a/span");
                updateNode = lableNode.SelectSingleNode("//*[@class='release label-latest']/div[2]/div/p/relative-time");
            }
            else if (tagsNode != null)
            {
                versionNode = tagsNode.SelectSingleNode("//*/li[1]/div/div/h3/a/span");
                updateNode = tagsNode.SelectSingleNode("//*/li/span/relative-time");
            }

            if (versionNode == null || updateNode == null)
                Messenger.AddError("Error! Can't parse GitHib version or creation date!");

            if (versionNode != null)
                modInfo.Version = Regex.Replace(versionNode.InnerText, @"[A-z]", string.Empty);
            if (updateNode != null)
                modInfo.ChangeDateAsDateTime = DateTime.Parse(updateNode.Attributes["datetime"].Value);
        }

        /// <summary>
        /// Takes a GitHub url and sets it to the shortest path to the project
        /// </summary>
        /// <param name="url">GitHub project url</param>
        /// <returns>Shortest GitHub project url</returns>
        public string ReduceToPlainUrl(string url)
        {
            var parts = GetUrlParts(url);
            if (parts[1].Equals(HOST2))
            {
                return parts[0] + "://www.github.com/" + parts[2] + "/" + parts[3];
            }
            return parts[0] + "://" + parts[1] + "/" + parts[2] + "/" + parts[3];
        }

        /// <summary>
        /// Splits a url into it's segment parts
        /// </summary>
        /// <param name="modUrl">A url to split</param>
        /// <exception cref="ArgumentException">ArgumentException("GitHub URL must point to a repository.")</exception>
        /// <returns>An array of the url segments
        /// 1: Scheme
        /// 2: Authority
        /// 4+: Additional path</returns>
        private static List<string> GetUrlParts(string modUrl)
        {
            // Split the url into parts
            var parts = new List<string>
            {
                new Uri(modUrl).Scheme, 
                new Uri(modUrl).Authority
            };

            parts.AddRange(new Uri(modUrl).Segments);

            for (int index = 0; index < parts.Count; index++)
            {
                parts[index] = parts[index].Trim(new char[] { '/' });
            }

            // Remove empty parts from the list
            parts = parts.Where(s => !string.IsNullOrWhiteSpace(s)).Distinct().ToList();

            // TODO Error message should go wherever strings are going.
            if (parts.Count < 4)
                throw new System.ArgumentException("GitHub URL must point to a repository.");

            return parts;
        }

        /// <summary>
        /// Gets a URL to a repository's releases
        /// </summary>
        /// <param name="modUrl">The URL to resolve</param>
        /// <returns>The resolved URL</returns>
        private static string GetPathToReleases(string modUrl)
        {
            var url = modUrl;
            if (modUrl.Contains("releases")) return url;

            var parts = GetUrlParts(modUrl);
            url = parts[0] + "://" + parts[1] + "/" + parts[2] + "/" + parts[3] + "/releases";

            return url;
        }

        /// <summary>
        /// Creates a list of DownloadInfos from a GitHub release
        /// </summary>
        /// <param name="modInfo">The mod to generate the list from</param>
        /// <returns>A list of one or more DownloadInfos for the most recent release of the selected repository</returns>
        private static List<DownloadInfo> GetDownloadInfo(ModInfo modInfo)
        {
            var htmlDoc = new HtmlWeb().Load(GetPathToReleases(modInfo.ModURL));
            htmlDoc.OptionFixNestedTags = true;

            var releases = new List<DownloadInfo>();

            // get the node that contains all releases (indepandend of label or tags view)
            // Easy way to get XPATH search: use chrome, inspect element, highlight the needed data, right-click and copy XPATH
            var timeLineNode = htmlDoc.DocumentNode.SelectSingleNode("//*[@id=\"js-repo-pjax-container\"]/div[2]/div[1]/div[2]");
            var lableNode = timeLineNode.SelectSingleNode("//*[@class=\"release label-latest\"]");
            var tagsNode = timeLineNode.SelectSingleNode("//*[@class=\"release-timeline-tags\"]");

            if (lableNode == null && tagsNode == null)
            {
                Messenger.AddError("Error! Can't parse GitHib for binaries!");
                return releases;
            }

            // get data from label or tag view.
            HtmlNodeCollection releaseNodes = null;
            if (lableNode != null)
            {
                // try find last release (select all link nodes of the download section within the class 'release label-latest')
                releaseNodes = lableNode.SelectNodes("//*[@class='release label-latest']/div[2]/ul/li[1]/a");

                // try find other releases (select all link nodes of the download section within the classes 'release label-latest')
                if (releaseNodes == null)
                    releaseNodes = lableNode.SelectNodes("//*[@class='release label-']/div[2]/ul/li[1]/a");
            }
            else if (tagsNode != null)
            {
                // try find last release (select all link nodes of the download section within the class 'release label-latest')
                releaseNodes = tagsNode.SelectNodes("//*[@class='release-timeline-tags']/li/div/div/ul/li[2]/a");
            }

            if (releaseNodes != null)
            {
                // iterate over all link nodes and get only urls with 'releases' in it.
                foreach (var s in releaseNodes)
                {
                    var url = "https://github.com" + s.Attributes["href"].Value;

                    if (!url.Contains("releases") && !url.Contains("archive")) continue;

                    var dInfo = new DownloadInfo
                    {
                        DownloadURL = url,
                        Filename = GetUrlParts(url).Last(),
                        Name = Path.GetFileNameWithoutExtension(GetUrlParts(url).Last())
                    };

                    releases.Add(dInfo);
                }
            }

            return releases;
        }
    }
}
