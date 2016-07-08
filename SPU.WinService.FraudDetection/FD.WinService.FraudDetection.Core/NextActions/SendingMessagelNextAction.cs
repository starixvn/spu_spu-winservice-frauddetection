// --------------------------------------------------------------------------
// <copyright file="SendingMessagelNextAction.cs" company="Nexcel Solutions Vietnam">
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

    public class SendingMessagelNextAction : INextAction
    {
        private readonly NotificationService _notificationService;

        public readonly int _messageId;

        public SendingMessagelNextAction(int messageMasterId)
        {
            _messageId = messageMasterId;
            _notificationService = new NotificationService();
        }

        public IList<SuspiciousInfo> SuspicionList { get; set; }

        public IList<DataCapture> DataCaptureList { get; set; }

        public void TakeAction()
        {

            return;
            //if (SuspicionList.Count <= 0)
            //{
            //    return;
            //}

            //var messageMaster = new MessageMasterService().GetMessageById(new MessageMasterGetByIdCriteria
            //{
            //    MessageId = _messageId
            //});
        }
    }
}