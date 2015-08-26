// --------------------------------------------------------------------------
// <copyright file="FileWriter.cs" company="Nexcel Solutions Vietnam">
// Copyright (coffee) Nexcel Solutions Vietnam. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------
// <history>
//   <change who="Long.Luu" date="2015.07.23">Created</change>
// </history>

namespace FD.WinService.FraudDetection.Core.MailSenders
{
    using System.IO;

    public class FileWriter
    {
        public static void WriteFile(string content)
        {
            File.WriteAllText("suspicious-statistics.txt", content);
        }
    }
}