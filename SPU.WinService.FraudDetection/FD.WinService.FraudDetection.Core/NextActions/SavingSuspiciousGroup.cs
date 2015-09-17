namespace FD.WinService.FraudDetection.Core.NextActions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Fanex.Logging;
    using Lib.Core.Utilities;
    using Lib.Criteria.CustomerBettingGroup;
    using Lib.Criteria.CustomerBettingGroupDetails;
    using Lib.Criteria.ScannedMatch;
    using Lib.Criteria.SuspiciousInfo;
    using Lib.Criteria.SuspiciousStatistic;
    using Lib.DTO;
    using Lib.Service.Implement;
    using Lib.Service.Interface;

    public class SavingSuspiciousGroup
    {
        protected const int ChunkSize = 50;

        private readonly IList<string> _matchIdList;

        private readonly int _taskId;

        private List<SuspiciousGroupInfo> _suspiciousGroupInfoList;

        public SavingSuspiciousGroup(List<SuspiciousGroupInfo> suspiciousInfoList, IList<string> matchIdList, int taskId)
        {
            _suspiciousGroupInfoList = suspiciousInfoList;
            _matchIdList = matchIdList;
            _taskId = taskId;

            ScannedMatchesService = new ScannedMatchesService();
            SuspiciousGroupInfoService = new SuspiciousGroupInfoService();
            SuspiciousCustomerService = new SuspiciousCustomerService();
            CustomerService = new CustomerService();
            CustomerBettingGroupService = new CustomerBettingGroupService();
            CustomerBettingGroupDetailsService = new CustomerBettingGroupDetailsService();
        }

        public IScannedMatchesService ScannedMatchesService { get; set; }

        public ISuspiciousGroupInfoService SuspiciousGroupInfoService { get; set; }

        public ISuspiciousCustomerService SuspiciousCustomerService { get; set; }

        public ICustomerService CustomerService { get; set; }

        public ICustomerBettingGroupService CustomerBettingGroupService { get; set; }

        public ICustomerBettingGroupDetailsService CustomerBettingGroupDetailsService { get; set; }

        public List<SuspiciousGroupInfo> AssignGroupList
        {
            get { return _suspiciousGroupInfoList; }
            set { _suspiciousGroupInfoList = AssignGroupList; }
        }

        public void TakeAction()
        {
            if (_suspiciousGroupInfoList.Count > 0)
            {
                InsertSuspiciousCustomers();
                InsertSuspiciousGroupInfo();
                CreateAndMergeBettingGroup();
            }

            InsertScannedMatch();
        }

        private IList<RawBettingGroup> rawBettingGroupList;

        private void ConvertIntoRawBettingGroup()
        {
            rawBettingGroupList = new List<RawBettingGroup>();
            var groups = _suspiciousGroupInfoList.GroupBy(t => new { t.GroupingOption, t.MatchId });

            foreach (var group in groups)
            {
                rawBettingGroupList.Add(new RawBettingGroup
                {
                    Customers = group.Select(t => new CustomerBettingGroupDetail
                    {
                        CustId = t.CustId,
                        CurrencyId = t.Currency,
                        CurrencyName = t.CurrencyName
                    }).ToList()
                });
            }
        }

        private void CreateAndMergeBettingGroup()
        {
            ConvertIntoRawBettingGroup();

            CreateAndMergeBettingNewGroupsOnTheFly();
        }

        private void CreateAndMergeBettingNewGroupsOnTheFly()
        {
            foreach (var group in rawBettingGroupList)
            {
                FindRelationBetweenOneNewGroupWithOldBettingGroups(group);

                if (group.RelatedBettingGroups.Any())
                {
                    MergeOneNewGroupIntoOldBettingGroups(group);
                }
                else
                {
                    CreateOneNewBettingGroup(group);
                }
            }
        }

        private void FindRelationBetweenOneNewGroupWithOldBettingGroups(RawBettingGroup group)
        {
            var chunks = group.Customers.Chunk(ChunkSize);
            var relatedGroups = new List<int>();

            foreach (var chunk in chunks)
            {
                var oldGroups = CustomerBettingGroupDetailsService.GetRelatedBettingGroups(new RelatedBettingGroupGetCriteria
                {
                    CustIds = string.Join(",", chunk.Select(t => t.CustId))
                });

                relatedGroups.AddRange(oldGroups);
            }

            group.RelatedBettingGroups.AddRange(relatedGroups.Distinct().OrderBy(t => t));
        }

        private void MergeOneNewGroupIntoOldBettingGroups(RawBettingGroup group)
        {
            var selectedGroup = group.RelatedBettingGroups.First();

            CustomerBettingGroupDetailsService.MergeCustomerBettingGroups(new CustomerBettingGroupsMergeCriteria
            {
                RelatedGroupIds = string.Join(",", group.RelatedBettingGroups)
            });

            var chunks = group.Customers.Chunk(ChunkSize);

            foreach (var chunk in chunks)
            {
                CustomerBettingGroupDetailsService.InsertCustomersIntoBettingGroup(new CustomersIntoBettingGroupInsertCriteria
                {
                    GroupId = selectedGroup,
                    XmlDoc = CreateXmlParamForInsertingCustomerBettingGroups(chunk)
                });
            }
        }

        private void CreateOneNewBettingGroup(RawBettingGroup group)
        {
            var criteria = new BettingGroupAutomaticallyCreateCriteria();
            var result = CustomerBettingGroupService.CreateBettingGroupAutomatically(criteria);

            if (!result.IsSuccessful)
            {
                return;
            }

            var chunks = group.Customers.Chunk(ChunkSize);

            foreach (var chunk in chunks)
            {
                CustomerBettingGroupDetailsService.InsertCustomersIntoBettingGroup(new CustomersIntoBettingGroupInsertCriteria
                {
                    GroupId = criteria.NewGroupId,
                    XmlDoc = CreateXmlParamForInsertingCustomerBettingGroups(chunk)
                });
            }
        }

        private void InsertScannedMatch()
        {
            var matchChunks = _matchIdList.Chunk(ChunkSize);

            foreach (var matchChunk in matchChunks)
            {
                var criteria = new MatchInsertCriteria
                {
                    MatchIdList = string.Join(",", matchChunk),
                    TaskId = _taskId,
                    ScannedDate = DateTime.Now
                };

                var result = ScannedMatchesService.InsertScannedMatch(criteria);

                if (result.IsSuccessful == false)
                {
                    Logger.Log.Error("Error UpdateScannedMatch: " + result.Message);
                }
            }
        }

        private void InsertSuspiciousGroupInfo()
        {
            var suspiciousChunks = _suspiciousGroupInfoList.Chunk(ChunkSize);

            foreach (var suspiciousChunk in suspiciousChunks)
            {
                var criteria = new SuspiciousGroupInfoInsertCriteria
                {
                    SuspiciousDoc = CreateXmlParamForInsertingSuspiciousInfo(suspiciousChunk)
                };

                var result = SuspiciousGroupInfoService.InsertSuspiciousGroupInfo(criteria);

                if (result.IsSuccessful == false)
                {
                    Logger.Log.Error("Error InsertSuspiciousInfo: " + result.Message);
                }
            }
        }

        private void InsertSuspiciousCustomers()
        {
            var newSuspiciousCustomers = FindNewSuspiciousCustomers();

            if (newSuspiciousCustomers.Count == 0)
            {
                return;
            }

            var newSuspiciousCustomerChunks = newSuspiciousCustomers.Chunk(ChunkSize);

            foreach (var newSuspiciousCustomer in newSuspiciousCustomerChunks)
            {
                InsertNewSuspiciousCustomers(newSuspiciousCustomer);
            }
        }

        private void InsertNewSuspiciousCustomers(IEnumerable<int> suspiciousCustomers)
        {
            var customers = CustomerService.GetCustormersByCustIds(new CustomerSelectCriteria()
            {
                ListCustIds = string.Join(",", suspiciousCustomers)
            });

            SuspiciousCustomerService.InsertSuspiciousCustomer(new SuspiciousCustomerInsertCriteria()
            {
                SuspiciousDoc = CreateXmlParamForInsertingSuspiciousCustomers(customers)
            });
        }

        private IList<int> FindNewSuspiciousCustomers()
        {
            var custIds = _suspiciousGroupInfoList.Select(t => t.CustId).Distinct().ToList();

            var custIdChunks = custIds.Chunk(ChunkSize);
            var newSuspiciousCustomers = new List<int>();

            foreach (var custIdChunk in custIdChunks)
            {
                var custStr = string.Join(",", custIdChunk);

                var temp = SuspiciousCustomerService.GetNewSuspiciousCustomers(new NewSuspiciousCustomersGetCriteria
                {
                    SuspiciousCustomers = custStr
                });

                newSuspiciousCustomers.AddRange(temp);
            }

            return newSuspiciousCustomers;
        }

        private string CreateXmlParamForInsertingSuspiciousCustomers(IEnumerable<Customer> suspiciousCustomers)
        {
            var builder = new StringBuilder();
            builder.Append("<Root>");

            foreach (var suspiciousCustomer in suspiciousCustomers)
            {
                builder.Append("<Row>");
                builder.Append(GetElement("CustId", suspiciousCustomer.CustId));
                builder.Append(GetElement("CustName", suspiciousCustomer.CustName));
                builder.Append("</Row>");
            }

            builder.Append("</Root>");

            return builder.ToString();
        }

        private string CreateXmlParamForInsertingCustomerBettingGroups(IEnumerable<CustomerBettingGroupDetail> details)
        {
            var builder = new StringBuilder();
            builder.Append("<Root>");

            foreach (var suspiciousCustomer in details)
            {
                builder.Append("<Row>");
                builder.Append(GetElement("CustId", suspiciousCustomer.CustId));
                builder.Append(GetElement("CurrencyId", suspiciousCustomer.CurrencyId));
                builder.Append(GetElement("CurrencyName", suspiciousCustomer.CurrencyName));
                builder.Append("</Row>");
            }

            builder.Append("</Root>");

            return builder.ToString();
        }

        private string CreateXmlParamForInsertingSuspiciousInfo(IEnumerable<SuspiciousGroupInfo> suspiciousInfos)
        {
            var builder = new StringBuilder();
            builder.Append("<Root>");

            foreach (var suspiciousInfo in suspiciousInfos)
            {
                builder.Append("<Row>");
                builder.Append(GetElement("CustId", suspiciousInfo.CustId));
                builder.Append(GetElement("MatchId", suspiciousInfo.MatchId));
                builder.Append(GetElement("TransId", suspiciousInfo.TransId));
                builder.Append(GetElement("TaskId", suspiciousInfo.TaskId));
                builder.Append(GetElement("CaseId", suspiciousInfo.CaseId));
                builder.Append(GetElement("GroupingOption", suspiciousInfo.GroupingOption));
                builder.Append("</Row>");
            }

            builder.Append("</Root>");

            return builder.ToString();
        }

        private string GetElement(string tagName, object value)
        {
            return string.Format("<{0}>{1}</{0}>", tagName, value);
        }
    }
}