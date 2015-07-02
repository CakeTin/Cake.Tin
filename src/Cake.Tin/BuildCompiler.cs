// -----------------------------------------------------------------------
// <copyright file="BuildCompiler.cs" company="Mark Walker">
//     Copyright (c) 2015, Mark Walker and contributors. Based on Cake - Copyright (c) 2014, Patrik Svensson and contributors.
// </copyright>
// -----------------------------------------------------------------------
namespace Cake.Tin
{
    using System;
    using System.Linq;

    using Cake.Common;
    using Cake.Common.Diagnostics;
    using Cake.Common.Tools.MSBuild;
    using Cake.Common.Tools.NuGet;
    using Cake.Core;
    using Cake.Core.Diagnostics;

    /// <summary>
    /// Class designed to be called from PowerShell to compile the build
    /// </summary>
    public static class BuildCompiler
    {
        #region Methods

        /// <summary>
        /// Compiles the specified solution filename.
        /// </summary>
        /// <param name="solutionFilename">The solution filename.</param>
        /// <param name="arguments">Arguments to pass to build</param>
        /// <returns>"Success" if build succeeded; otherwise error message </returns>
        public static string Compile(string solutionFilename, params string[] arguments)
        {
            ////System.Diagnostics.Debugger.Launch();
            Builder builder = null;
            string message = "Build failed.";
            try
            {
                if (arguments == null || !arguments.Any())
                {
                    arguments = new[]
                            {
                                "-target=Default",
                                "-configuration=Release",
                                "-verbosity=Normal"
                            };
                }

                builder = new Builder(solutionFilename);
                if (builder.Execute(arguments))
                {
                    message = "Success";
                }
            }
            catch (Exception ex)
            {
                if (builder == null)
                {
                    message = ex.ToString();
                }
                else
                {
                    builder.Log.Error("Error: {0}", ex);
                }
            }

            return message;
        }

        #endregion Methods

        #region Nested Types

        /// <summary>
        /// Class for building the Build solution
        /// </summary>
        private class Builder : CakeTinBase
        {
            #region Fields

            /// <summary>Solution filename</summary>
            private readonly string solutionFilename;

            #endregion Fields

            #region Constructors

            /// <summary>
            /// Initializes a new instance of the <see cref="Builder"/> class.
            /// </summary>
            /// <param name="solutionFilename">The solution filename.</param>
            internal Builder(string solutionFilename)
            {
                this.solutionFilename = solutionFilename;
            }

            #endregion Constructors

            #region Methods

            /// <summary>
            /// Creates and executes the build.
            /// </summary>
            protected internal override void CreateAndExecuteBuild()
            {
                this.Information("CreateAndExecuteBuild");
                var target = this.Argument("target", "Prebuild");
                var configuration = this.Argument("configuration", "Release");
                Task("Prebuild-Restore-NuGet-Packages")
                 .Does(() => this.NuGetRestore(this.solutionFilename));

                Task("Prebuild-Core")
                 .IsDependentOn("Prebuild-Restore-NuGet-Packages")
                 .Does(() => this.MSBuild(this.solutionFilename, settings =>
                    settings.SetConfiguration(configuration)
                        .UseToolVersion(MSBuildToolVersion.NET45)
                        .SetNodeReuse(false)));

                Task("Prebuild")
                  .IsDependentOn("Prebuild-Core");

                this.RunTarget(target);
            }

            #endregion Methods
        }

        #endregion Nested Types
    }
}