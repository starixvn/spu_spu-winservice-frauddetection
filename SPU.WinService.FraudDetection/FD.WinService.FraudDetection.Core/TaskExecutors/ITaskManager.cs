// --------------------------------------------------------------------------
// <copyright file="ITaskManager.cs" company="Nexcel Solutions Vietnam">
// Copyright (coffee) Nexcel Solutions Vietnam. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------
// <history>
//   <change who="Long.Luu" date="2015.06.12">Created</change>
// </history>

namespace FD.Lib.TaskExecutors
{
    using System.Collections.Generic;
    using BusinessManager.TaskExecutors;
    using DTO;

    public interface ITaskManager
    {
        IList<TaskExecutor> TaskExecutors { get; set; }

        /// <summary>
        ///     Add and start a task in the thread pool.
        /// </summary>
        /// <param name="taskId">The task to execute.</param>
        void AddTask(int taskId);

        /// <summary>
        ///     Update a task.
        /// </summary>
        /// <param name="task">The task to update.</param>
        void UpdateTask(int task);

        /// <summary>
        ///     Remove a task out the thread pool.
        /// </summary>
        /// <param name="task">The task name.</param>
        void RemoveTask(int task);

        void RemoveTasksInCase(int caseId);

        void AddTasksInCase(int caseId);

        void ClearTasks();

        bool Exist(Task task);
    }
}