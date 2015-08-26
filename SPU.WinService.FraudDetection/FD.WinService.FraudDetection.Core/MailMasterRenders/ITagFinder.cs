// --------------------------------------------------------------------------
// <copyright file="ITagFinder.cs" company="Nexcel Solutions Vietnam">
// Copyright (coffee) Nexcel Solutions Vietnam. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------
// <history>
//   <change who="Long.Luu" date="2015.07.23">Created</change>
// </history>

namespace FD.WinService.FraudDetection.Core.MailMasterRenders
{
    using System.Collections.Generic;

    public interface ITagFinder
    {
        string Tag { get; set; }

        IList<int> GetTagPositions(string content);
    }
}