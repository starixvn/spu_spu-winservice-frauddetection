// --------------------------------------------------------------------------
// <copyright file="PlaceHolderFinder.cs" company="Nexcel Solutions Vietnam">
// Copyright (coffee) Nexcel Solutions Vietnam. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------
// <history>
//   <change who="Long.Luu" date="2015.07.23">Created</change>
// </history>

namespace FD.WinService.FraudDetection.Core.MailMasterRenders
{
    using System.Collections.Generic;

    public class PlaceHolderFinder : IPlaceHolderFinder
    {
        public PlaceHolderFinder()
        {
            OpenTagFinder = new TagFinder
            {
                Tag = "&lt;placeholder&gt;"
            };

            CloseTagFinder = new TagFinder
            {
                Tag = "&lt;/placeholder&gt;"
            };
        }

        public ITagFinder OpenTagFinder { get; set; }

        public ITagFinder CloseTagFinder { get; set; }

        public IList<PlaceholderPosition> GetPlaceHolderPositions(string content)
        {
            var locations = new List<PlaceholderPosition>();
            var openPositions = OpenTagFinder.GetTagPositions(content);
            var closePositions = CloseTagFinder.GetTagPositions(content);

            if (openPositions.Count == 0 || closePositions.Count == 0)
            {
                return locations;
            }

            for (var i = 0; i < openPositions.Count && i < closePositions.Count; i++)
            {
                locations.Add(new PlaceholderPosition
                {
                    StartPlaceholder = openPositions[i],
                    EndPlaceholder = closePositions[i]
                });
            }

            return locations;
        }
    }
}