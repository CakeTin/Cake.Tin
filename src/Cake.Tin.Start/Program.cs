// -----------------------------------------------------------------------
// <copyright file="Program.cs" company="Mark Walker">
//     Copyright (c) 2015, Mark Walker and contributors. Based on Cake - Copyright (c) 2014, Patrik Svensson and contributors.
// </copyright>
// -----------------------------------------------------------------------
namespace Cake.Tin.Start
{
    using System;

    /// <summary>
    /// Program entry point
    /// </summary>
    public static class Program
    {
        #region Methods

        /// <summary>
        /// Defines the entry point of the application.
        /// </summary>
        public static void Main()
        {
            Environment.SetEnvironmentVariable("NUGET_EXE", @"C:\Repositories\c\cake\tools\nuget.exe");
            BuildCompiler.Compile(@"C:\Repositories\c\Cake.Tin.Bootstrapper\Build\Build.sln", "-configuration=\"Release\"");
        }

        #endregion Methods
    }
}