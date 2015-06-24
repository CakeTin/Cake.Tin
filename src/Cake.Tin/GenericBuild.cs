// -----------------------------------------------------------------------
// <copyright file="GenericBuild.cs" company="Mark Walker">
//     Copyright (c) 2015, Mark Walker and contributors. Based on Cake - Copyright (c) 2014, Patrik Svensson and contributors.
// </copyright>
// -----------------------------------------------------------------------
namespace Cake.Tin
{
    using System.Linq;

    using Cake.Common.Diagnostics;
    using Cake.Common.IO;
    using Cake.Common.Tools.MSBuild;
    using Cake.Common.Tools.NuGet;
    using Cake.Core;

    /// <summary>
    /// Generic build class
    /// </summary>
    public class GenericBuild : CakeTinBase
    {
        #region Methods

        /// <summary>
        /// Kicks off the actual build in the inherited class.
        /// </summary>
        protected override void RunBuild()
        {
            ///////////////////////////////////////////////////////////////////////////////
              // ARGUMENTS
              ///////////////////////////////////////////////////////////////////////////////

              var target = Argument("target", "Default");
              var configuration = Argument("configuration", "Release");

              ///////////////////////////////////////////////////////////////////////////////
              // GLOBAL VARIABLES
              ///////////////////////////////////////////////////////////////////////////////
              var solutions = this.GetFiles("./**/*.sln").Where(fp => !fp.ToString().ToLowerInvariant().EndsWith("build.sln")).ToArray();
              var solutionPaths = solutions.Select(solution => solution.GetDirectory());

              ///////////////////////////////////////////////////////////////////////////////
              // SETUP / TEARDOWN
              ///////////////////////////////////////////////////////////////////////////////

              // Executed BEFORE the first task.
              Setup(() => this.Information("Running tasks..."));

              // Executed AFTER the last task.
              Teardown(() => this.Information("Finished running tasks."));

              ///////////////////////////////////////////////////////////////////////////////
              // TASK DEFINITIONS
              ///////////////////////////////////////////////////////////////////////////////

              Task("Clean")
              .Does(() =>
              {
            // Clean solution directories.
            foreach (var path in solutionPaths)
            {
              this.Information("Cleaning {0}", path);
              this.CleanDirectories(path + "/**/bin/" + configuration);
              this.CleanDirectories(path + "/**/obj/" + configuration);
            }
              });

              Task("Restore")
              .Does(() =>
              {
            // Restore all NuGet packages.
            foreach (var solution in solutions)
            {
              this.Information("Restoring {0}...", solution);
              this.NuGetRestore(solution);
            }
              });

              Task("Build")
              .IsDependentOn("Clean")
              .IsDependentOn("Restore")
              .Does(() =>
              {
            // Build all solutions.
            foreach (var solution in solutions)
            {
              this.Information("Building {0}", solution);
              this.MSBuild(solution, settings =>
            settings.SetPlatformTarget(PlatformTarget.MSIL)
                .WithProperty("TreatWarningsAsErrors", "true")
                .WithTarget("Build")
                .SetConfiguration(configuration));
            }
              });

              ///////////////////////////////////////////////////////////////////////////////
              // TARGETS
              ///////////////////////////////////////////////////////////////////////////////

              Task("Default")
              .IsDependentOn("Build");

              ///////////////////////////////////////////////////////////////////////////////
              // EXECUTION
              ///////////////////////////////////////////////////////////////////////////////

              RunTarget(target);
        }

        #endregion Methods
    }
}