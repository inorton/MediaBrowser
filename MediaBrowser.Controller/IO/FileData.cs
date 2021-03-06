﻿using MediaBrowser.Controller.Library;
using MediaBrowser.Model.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MediaBrowser.Controller.IO
{
    /// <summary>
    /// Provides low level File access that is much faster than the File/Directory api's
    /// </summary>
    public static class FileData
    {
        /// <summary>
        /// Gets the filtered file system entries.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="searchPattern">The search pattern.</param>
        /// <param name="flattenFolderDepth">The flatten folder depth.</param>
        /// <param name="resolveShortcuts">if set to <c>true</c> [resolve shortcuts].</param>
        /// <param name="args">The args.</param>
        /// <returns>Dictionary{System.StringFileSystemInfo}.</returns>
        /// <exception cref="System.ArgumentNullException">path</exception>
        public static Dictionary<string, FileSystemInfo> GetFilteredFileSystemEntries(string path, ILogger logger, string searchPattern = "*", int flattenFolderDepth = 0, bool resolveShortcuts = true, ItemResolveArgs args = null)
        {
            if (string.IsNullOrEmpty(path))
            {
                throw new ArgumentNullException("path");
            }

            var entries = new DirectoryInfo(path).EnumerateFileSystemInfos(searchPattern, SearchOption.TopDirectoryOnly);

            if (!resolveShortcuts && flattenFolderDepth == 0)
            {
                return entries.ToDictionary(i => i.FullName, StringComparer.OrdinalIgnoreCase);
            }

            var dict = new Dictionary<string, FileSystemInfo>(StringComparer.OrdinalIgnoreCase);

            foreach (var entry in entries)
            {
                var isDirectory = (entry.Attributes & FileAttributes.Directory) == FileAttributes.Directory;

                var fullName = entry.FullName;

                if (resolveShortcuts && FileSystem.IsShortcut(fullName))
                {
                    var newPath = FileSystem.ResolveShortcut(fullName);

                    if (string.IsNullOrWhiteSpace(newPath))
                    {
                        //invalid shortcut - could be old or target could just be unavailable
                        logger.Warn("Encountered invalid shortcut: " + fullName);
                        continue;
                    }

                    // Don't check if it exists here because that could return false for network shares.
                    var data = new DirectoryInfo(newPath);

                    // add to our physical locations
                    if (args != null)
                    {
                        args.AddAdditionalLocation(newPath);
                    }

                    dict[newPath] = data;
                }
                else if (flattenFolderDepth > 0 && isDirectory)
                {
                    foreach (var child in GetFilteredFileSystemEntries(fullName, logger, flattenFolderDepth: flattenFolderDepth - 1, resolveShortcuts: resolveShortcuts))
                    {
                        dict[child.Key] = child.Value;
                    }
                }
                else
                {
                    dict[fullName] = entry;
                }
            }

            return dict;
        }

    }

}
