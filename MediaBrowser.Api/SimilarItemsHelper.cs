﻿using MediaBrowser.Controller.Dto;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Persistence;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Logging;
using MediaBrowser.Model.Querying;
using ServiceStack.ServiceHost;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MediaBrowser.Api
{
    /// <summary>
    /// Class BaseGetSimilarItems
    /// </summary>
    public class BaseGetSimilarItems : IReturn<ItemsResult>
    {
        /// <summary>
        /// Gets or sets the user id.
        /// </summary>
        /// <value>The user id.</value>
        [ApiMember(Name = "UserId", Description = "Optional. Filter by user id, and attach user data", IsRequired = false, DataType = "string", ParameterType = "query", Verb = "GET")]
        public Guid? UserId { get; set; }

        /// <summary>
        /// Gets or sets the id.
        /// </summary>
        /// <value>The id.</value>
        [ApiMember(Name = "Id", Description = "Item Id", IsRequired = true, DataType = "string", ParameterType = "path", Verb = "GET")]
        public string Id { get; set; }

        /// <summary>
        /// The maximum number of items to return
        /// </summary>
        /// <value>The limit.</value>
        [ApiMember(Name = "Limit", Description = "Optional. The maximum number of records to return", IsRequired = false, DataType = "int", ParameterType = "query", Verb = "GET")]
        public int? Limit { get; set; }

        /// <summary>
        /// Fields to return within the items, in addition to basic information
        /// </summary>
        /// <value>The fields.</value>
        [ApiMember(Name = "Fields", Description = "Optional. Specify additional fields of information to return in the output. This allows multiple, comma delimeted. Options: AudioInfo, Budget, Chapters, CriticRatingSummary, DateCreated, DisplayMediaType, EndDate, Genres, HomePageUrl, ItemCounts, IndexOptions, Locations, MediaStreams, Overview, OverviewHtml, ParentId, Path, People, ProviderIds, PrimaryImageAspectRatio, Revenue, SeriesInfo, SortName, Studios, Taglines, TrailerUrls, UserData", IsRequired = false, DataType = "string", ParameterType = "query", Verb = "GET", AllowMultiple = true)]
        public string Fields { get; set; }

        /// <summary>
        /// Gets the item fields.
        /// </summary>
        /// <returns>IEnumerable{ItemFields}.</returns>
        public IEnumerable<ItemFields> GetItemFields()
        {
            var val = Fields;

            if (string.IsNullOrEmpty(val))
            {
                return new ItemFields[] { };
            }

            return val.Split(',').Select(v => (ItemFields)Enum.Parse(typeof(ItemFields), v, true));
        }
    }

    /// <summary>
    /// Class SimilarItemsHelper
    /// </summary>
    public static class SimilarItemsHelper
    {
        /// <summary>
        /// Gets the similar items.
        /// </summary>
        /// <param name="userManager">The user manager.</param>
        /// <param name="itemRepository">The item repository.</param>
        /// <param name="libraryManager">The library manager.</param>
        /// <param name="userDataRepository">The user data repository.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="request">The request.</param>
        /// <param name="includeInSearch">The include in search.</param>
        /// <param name="getSimilarityScore">The get similarity score.</param>
        /// <returns>ItemsResult.</returns>
        internal static ItemsResult GetSimilarItems(IUserManager userManager, IItemRepository itemRepository, ILibraryManager libraryManager, IUserDataRepository userDataRepository, ILogger logger, BaseGetSimilarItems request, Func<BaseItem, bool> includeInSearch, Func<BaseItem, BaseItem, int> getSimilarityScore)
        {
            var user = request.UserId.HasValue ? userManager.GetUserById(request.UserId.Value) : null;

            var item = string.IsNullOrEmpty(request.Id) ?
                (request.UserId.HasValue ? user.RootFolder :
                (Folder)libraryManager.RootFolder) : DtoBuilder.GetItemByClientId(request.Id, userManager, libraryManager, request.UserId);

            var fields = request.GetItemFields().ToList();

            var dtoBuilder = new DtoBuilder(logger, libraryManager, userDataRepository, itemRepository);

            var inputItems = user == null
                                 ? libraryManager.RootFolder.RecursiveChildren
                                 : user.RootFolder.GetRecursiveChildren(user);

            var items = GetSimilaritems(item, inputItems, includeInSearch, getSimilarityScore).ToArray();

            var result = new ItemsResult
            {
                Items = items.Take(request.Limit ?? items.Length).Select(i => dtoBuilder.GetBaseItemDto(i, fields, user)).Select(t => t.Result).ToArray(),

                TotalRecordCount = items.Length
            };

            return result;
        }

        /// <summary>
        /// Gets the similaritems.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="inputItems">The input items.</param>
        /// <param name="includeInSearch">The include in search.</param>
        /// <param name="getSimilarityScore">The get similarity score.</param>
        /// <returns>IEnumerable{BaseItem}.</returns>
        private static IEnumerable<BaseItem> GetSimilaritems(BaseItem item, IEnumerable<BaseItem> inputItems, Func<BaseItem, bool> includeInSearch, Func<BaseItem, BaseItem, int> getSimilarityScore)
        {
            inputItems = inputItems.Where(includeInSearch);

            // Avoid implicitly captured closure
            var currentItem = item;

            return inputItems.Where(i => i.Id != currentItem.Id)
                .Select(i => new Tuple<BaseItem, int>(i, getSimilarityScore(item, i)))
                .Where(i => i.Item2 > 5)
                .OrderByDescending(i => i.Item2)
                .ThenByDescending(i => i.Item1.CriticRating ?? 0)
                .Select(i => i.Item1);
        }

        /// <summary>
        /// Gets the similiarity score.
        /// </summary>
        /// <param name="item1">The item1.</param>
        /// <param name="item2">The item2.</param>
        /// <returns>System.Int32.</returns>
        internal static int GetSimiliarityScore(BaseItem item1, BaseItem item2)
        {
            var points = 0;

            if (!string.IsNullOrEmpty(item1.OfficialRating) && string.Equals(item1.OfficialRating, item2.OfficialRating, StringComparison.OrdinalIgnoreCase))
            {
                points += 1;
            }

            // Find common genres
            points += item1.Genres.Where(i => item2.Genres.Contains(i, StringComparer.OrdinalIgnoreCase)).Sum(i => 5);

            // Find common tags
            points += item1.Tags.Where(i => item2.Tags.Contains(i, StringComparer.OrdinalIgnoreCase)).Sum(i => 5);

            // Find common studios
            points += item1.Studios.Where(i => item2.Studios.Contains(i, StringComparer.OrdinalIgnoreCase)).Sum(i => 3);

            var item2PeopleNames = item2.People.Select(i => i.Name).ToList();

            points += item1.People.Where(i => item2PeopleNames.Contains(i.Name, StringComparer.OrdinalIgnoreCase)).Sum(i =>
            {
                if (string.Equals(i.Type, PersonType.Director, StringComparison.OrdinalIgnoreCase))
                {
                    return 5;
                }
                if (string.Equals(i.Type, PersonType.Actor, StringComparison.OrdinalIgnoreCase))
                {
                    return 3;
                }
                if (string.Equals(i.Type, PersonType.Composer, StringComparison.OrdinalIgnoreCase))
                {
                    return 3;
                }
                if (string.Equals(i.Type, PersonType.GuestStar, StringComparison.OrdinalIgnoreCase))
                {
                    return 3;
                }
                if (string.Equals(i.Type, PersonType.Writer, StringComparison.OrdinalIgnoreCase))
                {
                    return 2;
                }

                return 1;
            });

            if (item1.ProductionYear.HasValue && item2.ProductionYear.HasValue)
            {
                var diff = Math.Abs(item1.ProductionYear.Value - item2.ProductionYear.Value);

                // Add if they came out within the same decade
                if (diff < 10)
                {
                    points += 2;
                }

                // And more if within five years
                if (diff < 5)
                {
                    points += 2;
                }
            }

            return points;
        }

    }
}
