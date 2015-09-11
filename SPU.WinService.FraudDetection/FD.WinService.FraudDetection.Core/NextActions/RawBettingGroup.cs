using System.Collections.Generic;

namespace FD.WinService.FraudDetection.Core.NextActions
{
    using Lib.DTO;

    public class RawBettingGroup
    {
        public List<CustomerBettingGroupDetail> Customers { get; set; }

        public List<int> RelatedBettingGroups { get; set; }

        public RawBettingGroup()
        {
            RelatedBettingGroups = new List<int>();
        }
    }
}