// --------------------------------------------------------------------------
// <copyright file="TaskLoader.cs" company="Nexcel Solutions Vietnam">
// Copyright (coffee) Nexcel Solutions Vietnam. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------
// <history>
//   <change who="Long.Luu" date="2015.07.23">Created</change>
// </history>

namespace FD.Lib.TaskLoaders
{
    using Criteria.Task;
    using QueueManagement;
    using QueueManagement.SendingTasks;
    using Service.Implement;
    using Service.Interface;

    public class TaskLoader : ITaskLoader
    {
        private readonly ISendingMessage _sendingTask;
        private readonly ITaskService _taskService;

        public TaskLoader()
        {
            _taskService = new TaskService();
            _sendingTask = new AddingTask(new MessageQueueSender());
        }

        public TaskLoader(ITaskService taskService, IMessageQueueSender messageQueue)
        {
            _taskService = taskService;
            _sendingTask = new AddingTask(messageQueue);
        }

        public void LoadTasks()
        {
            var taskList = _taskService.LoadAllTasks(new TaskGetAllCriteria());

            foreach (var task in taskList)
            {
                if (task.IsActive)
                {
                    _sendingTask.Send(task.TaskId);
                }
            }
        }
    }
}