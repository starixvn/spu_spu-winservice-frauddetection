namespace FD.WinService.FraudDetection.Core.NextActions
{
    using System;
    using System.Collections.Generic;
    using Fanex.Logging;
    using Lib.Core.Utilities;
    using Lib.Criteria.ScannedMatch;
    using Lib.Service.Implement;
    using Lib.Service.Interface;

    public class SavingScannedMatch
    {
        protected const int ChunkSize = 50;

        private readonly IList<string> _matchIdList;

        private readonly int _taskId;

        public SavingScannedMatch(IList<string> matchIdList, int taskId)
        {
            _matchIdList = matchIdList;
            _taskId = taskId;

            ScannedMatchesService = new ScannedMatchesService();
        }

        public IScannedMatchesService ScannedMatchesService { get; set; }

        public void TakeAction()
        {
            InsertScannedMatch();
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
    }
}