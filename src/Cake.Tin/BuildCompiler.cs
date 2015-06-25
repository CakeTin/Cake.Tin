// -----------------------------------------------------------------------
// <copyright file="BuildCompiler.cs" company="Mark Walker">
//     Copyright (c) 2015, Mark Walker and contributors. Based on Cake - Copyright (c) 2014, Patrik Svensson and contributors.
// </copyright>
// -----------------------------------------------------------------------
namespace Cake.Tin
{
    using System;

    using Cake.Common.Tools.MSBuild;
    using Cake.Common.Tools.NuGet;
    
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
        /// <returns>"Success" if build succeeded; otherwise error message </returns>
        public static string Compile(string solutionFilename)
        {
            try
            {
                var builder = new Builder(solutionFilename);
                builder.Execute();
                return "Success";
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
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
                this.NuGetRestore(this.solutionFilename);
                this.MSBuild(this.solutionFilename);
            }

            #endregion Methods
        }

        #endregion Nested Types
    }
}