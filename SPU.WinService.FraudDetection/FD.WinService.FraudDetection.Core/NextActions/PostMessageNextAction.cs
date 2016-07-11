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
    using System.Collections.Generic;
    using Lib.BusinessManager.ReportTemplateRenders;
    using Lib.DTO;
    using Lib.Service.Implement;
    using Lib.Service.Interface;
    using WinService.FraudDetection.Core.MailMasterRenders;
    using WinService.FraudDetection.Core.MailSenders;
    using FD.Lib.Criteria.MessageMaster;
    using FD.Lib.Criteria.UserMesssage;

    public class PostMessageNextAction : INextAction
    {
        private readonly NotificationService _notificationService;

        public readonly int _messageId;

        private readonly Task _task;

        public PostMessageNextAction(int messageMasterId, Task task)
        {
            _messageId = messageMasterId;
            _task = task;
            _notificationService = new NotificationService();
        }

        public IList<SuspiciousInfo> SuspicionList { get; set; }

        public IList<DataCapture> DataCaptureList { get; set; }

        public void TakeAction()
        {
            if (SuspicionList.Count <= 0)
            {
                return;
            }

            //var messageMaster = new MessageMasterService().GetMessageById(new MessageMasterGetByIdCriteria
            //{
            //    MessageId = _messageId
            //});

            //var listUsersOnline = _notificationService.GetUsersOnline(new NotificationGetUserOnlineCriteria 
            //{
            //    ExpriredTime = 15*60,
            //    UserIdList = messageMaster.OnlineRecipient
            //});

            //foreach(var userId in listUsersOnline)
            //{

            //}
        }
    }
}