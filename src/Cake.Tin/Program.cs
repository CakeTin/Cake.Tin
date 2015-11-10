// -----------------------------------------------------------------------
// <copyright file="Program.cs" company="Mark Walker">
//     Copyright (c) 2015, Mark Walker and contributors. Based on Cake - Copyright (c) 2014, Patrik Svensson and contributors.
// </copyright>
// -----------------------------------------------------------------------
namespace Cake.Tin
{
    /////// <summary>
    /////// Program entry point
    /////// </summary>
    ////public static class Program
    ////{
    ////    #region Methods
    ////    /// <summary>
    ////    /// Defines the entry point of the application.
    ////    /// </summary>
    ////    /// <param name="args">Command line args.</param>
    ////    public static void Main(string[] args)
    ////    {
    ////        var argsList = new List<string>(args);
    ////        if (!argsList.Any())
    ////        {
    ////            throw new ArgumentException("A solution file must be passed as a minimum");
    ////        }
    ////        string solutionName = argsList[0];
    ////        argsList.RemoveAt(0);
    ////        if (!argsList.Any())
    ////        {
    ////            argsList.AddRange(new[]
    ////                         {
    ////                            "-target=Default",
    ////                            "-configuration=Release",
    ////                            "-verbosity=Normal"
    ////                        });
    ////        }
    ////        BuildCompiler.Compile(solutionName, argsList.ToArray())
    ////    }
    ////    #endregion Methods
    ////}
    using System;
    using System.Collections.Generic;

    using Cake.Common.Diagnostics;
    using Cake.Common.Tools.NuGet;

    using Common.IO;
    using Common.Tools.MSBuild;

    using Core;
    using Core.Diagnostics;
    using Core.IO;

    public class ConsoleLog : ICakeLog
    {
        #region Fields

        /// <summary>Verbosity</summary>
        private readonly Verbosity verbosity;

        #endregion Fields

        #region Constructors

        public ConsoleLog(Verbosity verbosity)
        {
            this.verbosity = verbosity;
        }

        #endregion Constructors

        #region Properties

        public Verbosity Verbosity
        {
            get
            {
                return this.verbosity;
            }
        }

        #endregion Properties

        #region Methods

        public void Write(Verbosity verbose, LogLevel level, string format, params object[] args)
        {
            if (verbose >= this.Verbosity)
            {
                Console.WriteLine(format, args);
            }
        }

        #endregion Methods
    }

    public class Program
    {
        #region Methods

        public static void Main(string[] args)
        {
            if (System.Diagnostics.Debugger.IsAttached)
            {
                args = new[] { "-WorkingDirectory=C:\\Dev\\1Trunk", "-Target=PreBuild" };
            }

            ICakeLog logger = null;
            try
            {
                // Setup the input.
                logger = new ConsoleLog(Verbosity.Normal);
                var parser = new ArgumentParser(logger, new FileSystem());
                var options = parser.Parse(args);
                logger = new ConsoleLog(options.Verbosity);
                if (options.SolutionPath.IsRelative)
                {
                    options.SolutionPath = options.WorkingDirectory.CombineWithFilePath(options.SolutionPath);
                }

                EmbeddedCake.Run(options, cake => Script(cake, options), logger);
            }
            catch (Exception e)
            {
                if (logger != null)
                {
                    logger.Error(Verbosity.Normal, e.ToString());
                }
                else
                {
                    Console.WriteLine(e);
                }
            }
        }

        // This is the "Script"
        private static void Script(ICakeEngine cake, CakeTinOptions options)
        {
            cake.RegisterTask("Prebuild-Initial")
             .Does(context => context.Information("Building {0} configuration of {1}", options.Configuration, options.SolutionPath));

            cake.RegisterTask("Prebuild-Restore-NuGet-Packages")
             .IsDependentOn("Prebuild-Initial")
             .Does(context => context.NuGetRestore(options.SolutionPath));

            cake.RegisterTask("Prebuild-Core")
             .IsDependentOn("Prebuild-Restore-NuGet-Packages")
             .Does(
                 context =>
                 {
                     context.Information("Building {0} configuration of {1}", options.Configuration, options.SolutionPath);
                     context.MSBuild(
                             options.SolutionPath,
                             settings =>
                             settings.SetConfiguration(options.Configuration)
                                 .UseToolVersion(MSBuildToolVersion.NET45)
                                 .SetNodeReuse(false));
                 });

            cake.RegisterTask("Prebuild")
              .IsDependentOn("Prebuild-Core");

            ////var directory = new DirectoryPath("./build");

            ////// Clean directory task.
            ////cake.RegisterTask("Clean").Does(context =>
            ////{
            ////    context.CleanDirectory(directory);
            ////});

            ////// Build project.
            ////cake.RegisterTask("Build")
            ////    .IsDependentOn("Clean")
            ////    .Does(context =>
            ////    {
            ////        context.MSBuild("./src/Project.sln");
            ////    });

            ////cake.RegisterTask("Default")
            ////    .IsDependentOn("Build");
        }

        #endregion Methods
    }

    internal class EmbeddedCake
    {
        #region Methods

        public static void Run(CakeTinOptions arguments, Action<ICakeEngine> script, ICakeLog logger)
        {
            // Create the engine and setup a new context.
            var engine = new CakeEngine(logger);
            var context = CreateContext(arguments.WorkingDirectory, arguments.Verbosity, arguments.Arguments);

            // Execute the script.
            script(engine);

            // Run the target.
            engine.RunTarget(context, new DefaultExecutionStrategy(context.Log), arguments.Arguments.GetArgument("Target", "Default"));
        }

        private static ICakeContext CreateContext(DirectoryPath workingDirectory, Verbosity verbosity, CakeTinArguments args)
        {
            var fileSystem = new FileSystem();

            var environment = new CakeEnvironment { WorkingDirectory = workingDirectory };

            var globber = new Globber(fileSystem, environment);
            var log = new ConsoleLog(verbosity);

            var processRunner = new ProcessRunner(environment, log);

            var registry = new WindowsRegistry();

            return new CakeContext(
                fileSystem,
                environment,
                globber,
                log,
                args,
                processRunner,
                registry);
        }

        #endregion Methods
    }
}