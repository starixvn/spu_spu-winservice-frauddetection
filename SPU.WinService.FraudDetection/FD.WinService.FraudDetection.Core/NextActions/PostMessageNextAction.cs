// --------------------------------------------------------------------------
// <copyright file="PostMessageNextAction.cs" company="Nexcel Solutions Vietnam">
// Copyright (coffee) Nexcel Solutions Vietnam. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------
// <history>
//   <change who="Aaron" date="2016.07.08">Created</change>
// </history>

namespace FD.WinService.FraudDetection.Core.NextActions
{
    using FD.Lib.Criteria.MessageMaster;
    using FD.Lib.Criteria.UserMesssage;
    using Lib.DTO;
    using Lib.Service.Implement;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Linq;
    using System.Text;

    public class PostMessageNextAction : INextAction
    {
        private readonly NotificationService _notificationService;

        private int _messageId;

        private Task _task;

        public PostMessageNextAction(int messageId, Task task)
        {
            _messageId = messageId;
            _task = task;
            _notificationService = new NotificationService();
        }

        public IList<SuspiciousInfo> SuspiciousList { get; set; }

        public IList<DataCapture> DataCaptureList { get; set; }

        public void TakeAction()
        {
            if (SuspiciousList.Count <= 0 && DataCaptureList.Count <= 0)
            {
                return;
            }

            var matchIdList = GetMatchIdList();
            var userIdList = GetUserIdList();

            if (userIdList.Count <= 0)
            {
                return;
            }

            var criteria = new NotificationInsertUserMesssageCriteria
            {
                CreatorId = 0,
                UserMesssageDoc = CreateXmlParamForInsertingUserMessage(userIdList, matchIdList)
            };

            _notificationService.InsertUserMessage(criteria);
        }

        private List<string> GetUserIdList()
        {
            var messageMaster = new MessageMasterService().GetMessageById(new MessageMasterGetByIdCriteria
            {
                MessageId = _messageId
            });

            var userList = string.IsNullOrEmpty(messageMaster.Recipient) ? 
                new List<string>() : 
                messageMaster.Recipient.Split(';').Where(u => !string.IsNullOrEmpty(u)).ToList();

            var listUsersOnline = _notificationService.GetUsersOnline(new NotificationGetUserOnlineCriteria
            {
                ExpiredTime = int.Parse(ConfigurationManager.AppSettings["ExpiredTime"]),
                UserIdList = messageMaster.OnlineRecipient
            });

            foreach (var userId in listUsersOnline)
            {
                userList.Add(userId.UserId.ToString());
            }

            return userList.Distinct().ToList();
        }

        private List<int> GetMatchIdList()
        {
            var matchIdList = new List<int>();

            if (SuspiciousList.Count > 0)
            {
                matchIdList = SuspiciousList.Select(s => s.MatchId).Distinct().ToList();
            }
            else if (DataCaptureList.Count > 0)
            {
                matchIdList = DataCaptureList.Select(s => s.MatchId).Distinct().ToList();
            }

            return matchIdList;
        }

        private string CreateXmlParamForInsertingUserMessage(List<string> userIdList, List<int> matchIdList)
        {
            var builder = new StringBuilder();
            builder.Append("<Root>");

            foreach (var userId in userIdList)
            {
                foreach (var matchId in matchIdList)
                {
                    builder.Append("<Row>");
                    builder.Append(GetElement("UserId", userId));
                    builder.Append(GetElement("MessageId", _messageId));
                    builder.Append(GetElement("TaskId", _task.TaskId));
                    builder.Append(GetElement("MatchId", matchId));
                    builder.Append(GetElement("CaseId", _task.CaseId));
                    builder.Append("</Row>");
                }
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