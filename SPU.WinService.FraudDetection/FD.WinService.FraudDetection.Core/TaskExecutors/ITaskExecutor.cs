// --------------------------------------------------------------------------
// <copyright file="ITaskExecutor.cs" company="Nexcel Solutions Vietnam">
// Copyright (c) Nexcel Solutions Vietnam. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------
// <history>
//   <change who="Long.Luu" date="2015.03.25">Created</change>
// </history>

namespace FD.Lib.TaskExecutors
{
    public interface ITaskExecutor
    {
        void Start();

        void Stop();

        void Execute();
    }
}