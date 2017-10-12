namespace FD.WinService.FraudDetection.Core.NextActions
{
    using FD.Lib.Criteria.Group;
    using FD.Lib.DTO;
    using Lib.Service.Implement;
    using Lib.Service.Interface;
    using System.Collections.Generic;

    public class SyncDataNextAction : INextAction
    {
        private int _groupId;

        private int _caseId;

        private ICustomerGroupService _customerGroupService;

        private IList<string> _associatedAccounts;

        public SyncDataNextAction(int groupId, int caseId, List<string> associatedAccounts)
        {
            _caseId = caseId;
            _groupId = groupId;
            _customerGroupService = new CustomerGroupService();
            _associatedAccounts = associatedAccounts;
        }

        public IList<SuspiciousInfo> SuspiciousList { get; set; }

        public IList<DataCapture> DataCaptureList { get; set; }

        public void TakeAction()
        {
            _customerGroupService.InsertAssociatedAccount(new AssociatedAccountInsertCriteria() 
            {
                CaseId = _caseId,
                GroupId = _groupId,
                CustNameList = string.Join(",", _associatedAccounts)
            });
        }
    }
}