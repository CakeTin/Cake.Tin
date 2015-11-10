// -----------------------------------------------------------------------
// <copyright file="ArgumentParser.cs" company="Mark Walker">
//     Copyright (c) 2015, Mark Walker and contributors. Based on Cake - Copyright (c) 2014, Patrik Svensson and contributors.
// </copyright>
// -----------------------------------------------------------------------
namespace Cake.Tin
{
    using System;
    using System.ComponentModel;
    using System.Linq;

    using Cake.Core;
    using Cake.Core.Diagnostics;
    using Cake.Core.IO;

    internal sealed class ArgumentParser
    {
        #region Fields

        /// <summary>Default script name conventions</summary>
        private readonly string[] defaultSolutionNames = 
        {
            "build.sln",
            "build/build.sln",
            "build/Merlot.Aero.Build.sln",
        };

        /// <summary>File system</summary>
        private readonly IFileSystem fileSystem;

        /// <summary>Logger instance</summary>
        private readonly ICakeLog log;

        #endregion Fields

        #region Constructors

        public ArgumentParser(ICakeLog log, IFileSystem fileSystem)
        {
            this.log = log;
            this.fileSystem = fileSystem;
        }

        #endregion Constructors

        #region Methods

        public CakeTinOptions Parse(string[] args)
        {
            if (args == null)
            {
                throw new ArgumentNullException(nameof(args));
            }

            var options = new CakeTinOptions();
            options.Arguments = new CakeTinArguments();
            var isParsingOptions = false;

            var arguments = args.ToList();

            // If we don't have any arguments, search for a default script.
            if (arguments.Count == 0)
            {
                options.SolutionPath = this.GetDefaultSolution(options.WorkingDirectory);
                if (options.SolutionPath == null)
                {
                    throw new ArgumentException("No solution found to build.");
                }
            }

            foreach (var arg in arguments)
            {
                var value = arg.UnQuote();

                if (isParsingOptions)
                {
                    if (IsOption(value))
                    {
                        if (!this.ParseOption(value, options))
                        {
                            return null;
                        }
                    }
                    else
                    {
                        this.log.Error("More than one solution specified?");
                        return null;
                    }
                }
                else
                {
                    try
                    {
                        // If they didn't provide a specific build solution, search for a default.
                        if (IsOption(arg))
                        {
                            // Make sure we parse the option
                            if (!this.ParseOption(value, options))
                            {
                                return null;
                            }

                            options.SolutionPath = this.GetDefaultSolution(options.WorkingDirectory);
                            continue;
                        }

                        // Quoted?
                        options.SolutionPath = new FilePath(value);
                    }
                    finally
                    {
                        // Start parsing options.
                        isParsingOptions = true;
                    }
                }
            }

            return options;
        }

        private static bool IsOption(string arg)
        {
            if (string.IsNullOrWhiteSpace(arg))
            {
                return false;
            }
            return arg[0] == '-';
        }

        private FilePath GetDefaultSolution(DirectoryPath workingDirectory)
        {
            this.log.Verbose("Searching for default build script...");

            // Search for default build solutions in order
            foreach (var solutionName in this.defaultSolutionNames)
            {
                var currentFile = new FilePath(solutionName);
                var file = this.fileSystem.GetFile(currentFile);
                if (!file.Exists)
                {
                    currentFile = new FilePath(workingDirectory.CombineWithFilePath(file.Path).FullPath);
                    file = this.fileSystem.GetFile(currentFile);
                }

                if (file.Exists)
                {
                    this.log.Verbose("Found default build script: {0}", solutionName);
                    return currentFile;
                }
            }

            return null;
        }

        private bool ParseOption(string arg, CakeTinOptions options)
        {
            string name, value;

            var separatorIndex = arg.IndexOfAny(new[] { '=' });
            if (separatorIndex < 0)
            {
                name = arg.Substring(1);
                value = string.Empty;
            }
            else
            {
                name = arg.Substring(1, separatorIndex - 1);
                value = arg.Substring(separatorIndex + 1);
            }

            if (value.Length > 2)
            {
                if (value[0] == '\"' && value[value.Length - 1] == '\"')
                {
                    value = value.Substring(1, value.Length - 2);
                }
            }

            return this.ParseOption(name, value, options);
        }

        private bool ParseOption(string name, string value, CakeTinOptions options)
        {
            if (name.Equals("verbosity", StringComparison.OrdinalIgnoreCase)
                || name.Equals("v", StringComparison.OrdinalIgnoreCase))
            {
                // Parse verbosity.
                var converter = TypeDescriptor.GetConverter(typeof(Verbosity));
                var verbosity = converter.ConvertFromInvariantString(value);
                if (verbosity != null)
                {
                    options.Verbosity = (Verbosity)verbosity;
                }
            }

            if (name.Equals("showdescription", StringComparison.OrdinalIgnoreCase) ||
                name.Equals("s", StringComparison.OrdinalIgnoreCase))
            {
                options.ShowDescription = true;
            }

            if (name.Equals("dryrun", StringComparison.OrdinalIgnoreCase) ||
                name.Equals("noop", StringComparison.OrdinalIgnoreCase) ||
                name.Equals("whatif", StringComparison.OrdinalIgnoreCase))
            {
                options.PerformDryRun = true;
            }

            if (name.Equals("help", StringComparison.OrdinalIgnoreCase) ||
                name.Equals("?", StringComparison.OrdinalIgnoreCase))
            {
                options.ShowHelp = true;
            }

            if (name.Equals("version", StringComparison.OrdinalIgnoreCase) ||
                name.Equals("ver", StringComparison.OrdinalIgnoreCase))
            {
                options.ShowVersion = true;
            }
            if (name.Equals("workingdirectory", StringComparison.OrdinalIgnoreCase)
                || name.Equals("workingfolder", StringComparison.OrdinalIgnoreCase))
            {
                options.WorkingDirectory = value;
            }

            if (options.Arguments.HasArgument(name))
            {
                this.log.Error("Multiple arguments with the same name ({0}).", name);
                return false;
            }

            options.Arguments.SetArgument(name, value);
            return true;
        }

        #endregion Methods
    }
}