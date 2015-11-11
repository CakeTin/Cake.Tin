// -----------------------------------------------------------------------
// <copyright file="CakeTinOptions.cs" company="Mark Walker">
//     Copyright (c) 2015, Mark Walker and contributors. Based on Cake - Copyright (c) 2014, Patrik Svensson and contributors.
// </copyright>
// -----------------------------------------------------------------------
namespace Cake.Tin
{
    using System;

    using Cake.Core.Diagnostics;
    using Cake.Core.IO;

    /// <summary>
    /// RunBuild options
    /// </summary>
    internal class CakeTinOptions
    {
        #region Fields

        /// <summary>Arguments</summary>
        private CakeTinArguments arguments;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="CakeTinOptions"/> class.
        /// </summary>
        public CakeTinOptions()
        {
            this.ShowDescription = false;
            this.ShowHelp = false;
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets the script arguments.
        /// </summary>
        public CakeTinArguments Arguments
        {
            get
            {
                return this.arguments;
            }

            set
            {
                this.arguments = value;
                this.Verbosity =  (Verbosity)Enum.Parse(typeof(Verbosity), this.arguments.GetArgument("Verbosity", Verbosity.Normal.ToString()));
                this.WorkingDirectory = this.arguments.GetArgument("WorkingDirectory", new DirectoryPath(System.IO.Path.GetDirectoryName(typeof(Program).Assembly.Location)).ToString());
                this.Target = this.arguments.GetArgument("Target", "Default");
            }
        }

        /// <summary>
        /// Gets the build configuration.
        /// </summary>
        public string Configuration { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to perform a dry run.
        /// </summary>
        public bool PerformDryRun { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to show task descriptions.
        /// </summary>
        public bool ShowDescription { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to show help.
        /// </summary>
        public bool ShowHelp { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to show version information.
        /// </summary>
        public bool ShowVersion { get; set; }

        /// <summary>
        /// Gets or sets the build script.
        /// </summary>
        public FilePath SolutionPath { get; set; }

        /// <summary>
        /// Gets the initial target.
        /// </summary>
        public string Target { get; private set; }

        /// <summary>
        /// Gets the output verbosity.
        /// </summary>
        public Verbosity Verbosity { get; set; }

        /// <summary>
        /// Gets the Working Directory.
        /// </summary>
        public DirectoryPath WorkingDirectory { get; set; }

        #endregion Properties
    }
}