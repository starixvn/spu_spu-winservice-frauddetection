namespace FD.Lib.TaskExecutors
{
    using BusinessManager.TaskExecutors;
    using Core.Utilities;
    using DTO;
    using FD.Lib.Service.Implement;
    using Newtonsoft.Json;
    using Service.Interface;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Linq;
    using WinService.FraudDetection.Core.NextActions;

    public class SyncDataWithFPSTaskExecutor : TaskExecutor
    {
        private IList<Customer> _customers;

        private List<string> _associatedAccounts;

        private IHttpWebService _webService;

        private string FPS_ASSOCIATION = ConfigurationManager.AppSettings["FPS_ASSOCIATION"];

        public SyncDataWithFPSTaskExecutor(Task task)
            : base(task)
        {
            _customers = new List<Customer>();
            _associatedAccounts = new List<string>();
            _webService = new HttpWebService();
        }

        public SyncDataWithFPSTaskExecutor(Task task, ITaskService taskService)
            : base(task, taskService)
        {
            _customers = new List<Customer>();
            _associatedAccounts = new List<string>();
            _webService = new HttpWebService();
        }

        protected override void ClearOldResult()
        {
            _customers.Clear();
            _associatedAccounts.Clear();
        }

        protected override void ExecutePrecondition()
        {
            if (Task.HasPrecondition)
            {
                var preConditionQuery = Task.TaskPreconditions;
                _customers = TaskService.ExecutePreconditionForSyncDataWithFPS(preConditionQuery).ToList();
            }
        }

        protected override void ExecuteRule()
        {
            if (Task.HasRule)
            {
                if (_customers.Any())
                {
                    var chunks = _customers.Chunk(ChunkSize);
                    foreach (var chunk in chunks)
                    {
                        var data = new { usernames = string.Join(",", chunk.Select(t => t.CustName)) };
                        var result = _webService.PostJson(FPS_ASSOCIATION, JsonConvert.SerializeObject(data)).Replace("\"", "");

                        if (!string.IsNullOrEmpty(result) && result.Length > 1)
                            _associatedAccounts.AddRange(result.Split(','));
                    }

                    foreach (var customer in _customers)
                    {
                        _associatedAccounts.RemoveAll(t => t.ToLower() == customer.CustName.ToLower());
                    }

                    _associatedAccounts = _associatedAccounts.Distinct().ToList();
                }
            }
        }

        protected override void TakeNextAction()
        {
            if (Task.HasNextAction)
            {
                if (_associatedAccounts.Count == 0)
                {
                    return;
                }

                foreach (var item in NextActionDictionary)
                {
                    var actionType = Utility.ParseEnum<NextActionType>(item.Key.ToString());
                    if (actionType == NextActionType.AssigningGroup)
                    {
                        actionType = NextActionType.SyncDataWithFPS;
                    }

                    var nextAction = NextActionFactory.GetNextAction(actionType, item.Value, Task, _associatedAccounts);
                    nextAction.TakeAction();
                }
            }
        }
    }
}