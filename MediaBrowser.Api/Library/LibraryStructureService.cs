﻿using System.Threading;
using MediaBrowser.Controller;
using MediaBrowser.Controller.Library;
using MediaBrowser.Model.Entities;
using ServiceStack.ServiceHost;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MediaBrowser.Api.Library
{
    /// <summary>
    /// Class GetDefaultVirtualFolders
    /// </summary>
    [Route("/Library/VirtualFolders", "GET")]
    [Route("/Users/{UserId}/VirtualFolders", "GET")]
    public class GetVirtualFolders : IReturn<List<VirtualFolderInfo>>
    {
        /// <summary>
        /// Gets or sets the user id.
        /// </summary>
        /// <value>The user id.</value>
        public string UserId { get; set; }
    }

    [Route("/Library/VirtualFolders/{Name}", "POST")]
    [Route("/Users/{UserId}/VirtualFolders/{Name}", "POST")]
    public class AddVirtualFolder : IReturnVoid
    {
        /// <summary>
        /// Gets or sets the user id.
        /// </summary>
        /// <value>The user id.</value>
        public string UserId { get; set; }
        
        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>The name.</value>
        public string Name { get; set; }
    }

    [Route("/Library/VirtualFolders/{Name}", "DELETE")]
    [Route("/Users/{UserId}/VirtualFolders/{Name}", "DELETE")]
    public class RemoveVirtualFolder : IReturnVoid
    {
        /// <summary>
        /// Gets or sets the user id.
        /// </summary>
        /// <value>The user id.</value>
        public string UserId { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>The name.</value>
        public string Name { get; set; }
    }

    [Route("/Library/VirtualFolders/{Name}/Name", "POST")]
    [Route("/Users/{UserId}/VirtualFolders/{Name}/Name", "POST")]
    public class RenameVirtualFolder : IReturnVoid
    {
        /// <summary>
        /// Gets or sets the user id.
        /// </summary>
        /// <value>The user id.</value>
        public string UserId { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>The name.</value>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>The name.</value>
        public string NewName { get; set; }
    }

    [Route("/Library/VirtualFolders/{Name}/Paths", "POST")]
    [Route("/Users/{UserId}/VirtualFolders/{Name}/Paths", "POST")]
    public class AddMediaPath : IReturnVoid
    {
        /// <summary>
        /// Gets or sets the user id.
        /// </summary>
        /// <value>The user id.</value>
        public string UserId { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>The name.</value>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>The name.</value>
        public string Path { get; set; }
    }
    
    [Route("/Library/VirtualFolders/{Name}/Paths", "DELETE")]
    [Route("/Users/{UserId}/VirtualFolders/{Name}/Paths", "DELETE")]
    public class RemoveMediaPath : IReturnVoid
    {
        /// <summary>
        /// Gets or sets the user id.
        /// </summary>
        /// <value>The user id.</value>
        public string UserId { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>The name.</value>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>The name.</value>
        public string Path { get; set; }
    }

    /// <summary>
    /// Class LibraryStructureService
    /// </summary>
    public class LibraryStructureService : BaseApiService
    {
        /// <summary>
        /// The _app paths
        /// </summary>
        private readonly IServerApplicationPaths _appPaths;

        /// <summary>
        /// The _user manager
        /// </summary>
        private readonly IUserManager _userManager;

        /// <summary>
        /// The _library manager
        /// </summary>
        private readonly ILibraryManager _libraryManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="LibraryStructureService"/> class.
        /// </summary>
        /// <param name="appPaths">The app paths.</param>
        /// <param name="userManager">The user manager.</param>
        /// <param name="libraryManager">The library manager.</param>
        /// <exception cref="System.ArgumentNullException">appPaths</exception>
        public LibraryStructureService(IServerApplicationPaths appPaths, IUserManager userManager, ILibraryManager libraryManager)
        {
            if (appPaths == null)
            {
                throw new ArgumentNullException("appPaths");
            }

            _userManager = userManager;
            _appPaths = appPaths;
            _libraryManager = libraryManager;
        }

        /// <summary>
        /// Gets the specified request.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns>System.Object.</returns>
        public object Get(GetVirtualFolders request)
        {
            if (string.IsNullOrEmpty(request.UserId))
            {
                var result = _libraryManager.GetDefaultVirtualFolders().OrderBy(i => i.Name).ToList();

                return ToOptimizedResult(result);
            }
            else
            {
                var user = _userManager.GetUserById(new Guid(request.UserId));

                var result = _libraryManager.GetVirtualFolders(user).OrderBy(i => i.Name).ToList();

                return ToOptimizedResult(result);
            }
        }

        /// <summary>
        /// Posts the specified request.
        /// </summary>
        /// <param name="request">The request.</param>
        public void Post(AddVirtualFolder request)
        {
            if (string.IsNullOrEmpty(request.UserId))
            {
                LibraryHelpers.AddVirtualFolder(request.Name, null, _appPaths);
            }
            else
            {
                var user = _userManager.GetUserById(new Guid(request.UserId));

                LibraryHelpers.AddVirtualFolder(request.Name, user, _appPaths);
            }

            _libraryManager.ValidateMediaLibrary(new Progress<double>(), CancellationToken.None);
        }

        /// <summary>
        /// Posts the specified request.
        /// </summary>
        /// <param name="request">The request.</param>
        public void Post(RenameVirtualFolder request)
        {
            if (string.IsNullOrEmpty(request.UserId))
            {
                LibraryHelpers.RenameVirtualFolder(request.Name, request.NewName, null, _appPaths);
            }
            else
            {
                var user = _userManager.GetUserById(new Guid(request.UserId));

                LibraryHelpers.RenameVirtualFolder(request.Name, request.NewName, user, _appPaths);
            }

            _libraryManager.ValidateMediaLibrary(new Progress<double>(), CancellationToken.None);
        }
        
        /// <summary>
        /// Deletes the specified request.
        /// </summary>
        /// <param name="request">The request.</param>
        public void Delete(RemoveVirtualFolder request)
        {
            if (string.IsNullOrEmpty(request.UserId))
            {
                LibraryHelpers.RemoveVirtualFolder(request.Name, null, _appPaths);
            }
            else
            {
                var user = _userManager.GetUserById(new Guid(request.UserId));

                LibraryHelpers.RemoveVirtualFolder(request.Name, user, _appPaths);
            }

            _libraryManager.ValidateMediaLibrary(new Progress<double>(), CancellationToken.None);
        }

        /// <summary>
        /// Posts the specified request.
        /// </summary>
        /// <param name="request">The request.</param>
        public void Post(AddMediaPath request)
        {
            if (string.IsNullOrEmpty(request.UserId))
            {
                LibraryHelpers.AddMediaPath(request.Name, request.Path, null, _appPaths);
            }
            else
            {
                var user = _userManager.GetUserById(new Guid(request.UserId));

                LibraryHelpers.AddMediaPath(request.Name, request.Path, user, _appPaths);
            }

            _libraryManager.ValidateMediaLibrary(new Progress<double>(), CancellationToken.None);
        }

        /// <summary>
        /// Deletes the specified request.
        /// </summary>
        /// <param name="request">The request.</param>
        public void Delete(RemoveMediaPath request)
        {
            if (string.IsNullOrEmpty(request.UserId))
            {
                LibraryHelpers.RemoveMediaPath(request.Name, request.Path, null, _appPaths);
            }
            else
            {
                var user = _userManager.GetUserById(new Guid(request.UserId));

                LibraryHelpers.RemoveMediaPath(request.Name, request.Path, user, _appPaths);
            }

            _libraryManager.ValidateMediaLibrary(new Progress<double>(), CancellationToken.None);
        }
    }
}
