// -----------------------------------------------------------------------
// <copyright file="CakeTinBase.cs" company="Patrik Svensson and contributors.">
//     Copyright (c) 2014, Patrik Svensson and contributors.
// </copyright>
// -----------------------------------------------------------------------
namespace Cake.Tin
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Globalization;
    using System.Linq;

    using Autofac;
    using Autofac.Builder;

    using Cake.Arguments;
    using Cake.Commands;
    using Cake.Common;
    using Cake.Common.Build;
    using Cake.Common.Build.AppVeyor;
    using Cake.Core;
    using Cake.Core.Diagnostics;
    using Cake.Core.IO;
    using Cake.Core.IO.NuGet;
    using Cake.Core.Scripting;
    using Cake.Diagnostics;
    using Cake.Scripting;
    using Cake.Tin.Enums;

    using IContainer = Autofac.IContainer;

    /// <summary>
    /// Base class for strongly-typed builds
    /// </summary>
    public abstract class CakeTinBase : ICakeContext, IDisposable
    {
        #region Fields

        /// <summary>RunBuild script host</summary>
        private BuildScriptHost buildScriptHost;

        /// <summary>Tool resolver lookup</summary>
        private ILookup<string, IToolResolver> toolResolverLookup;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="CakeTinBase"/> class.
        /// </summary>
        protected CakeTinBase()
            : this(null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CakeTinBase"/> class.
        /// </summary>
        /// <param name="container">The container.</param>
        protected CakeTinBase(IContainer container)
        {
            this.Container = container ?? CreateContainer();
              this.SetProperties(
              this.Container.Resolve<IFileSystem>(),
              this.Container.Resolve<ICakeEnvironment>(),
              this.Container.Resolve<IGlobber>(),
              this.Container.Resolve<ICakeLog>(),
              this.Container.Resolve<ICakeArguments>(),
              this.Container.Resolve<IProcessRunner>(),
              this.Container.Resolve<IEnumerable<IToolResolver>>(),
              this.Container.Resolve<IRegistry>());
              this.ArgOptions = ArgumentOptions.CommandLine;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CakeTinBase"/> class.
        /// </summary>
        /// <param name="fileSystem">The file system.</param>
        /// <param name="environment">The environment.</param>
        /// <param name="globber">The globber.</param>
        /// <param name="log">The log.</param>
        /// <param name="arguments">The arguments.</param>
        /// <param name="processRunner">The process runner.</param>
        /// <param name="toolResolvers">The tool resolvers.</param>
        /// <param name="registry">The registry.</param>
        protected CakeTinBase(
            IFileSystem fileSystem,
            ICakeEnvironment environment,
            IGlobber globber,
            ICakeLog log,
            ICakeArguments arguments,
            IProcessRunner processRunner,
            IEnumerable<IToolResolver> toolResolvers,
            IRegistry registry)
        {
            this.SetProperties(fileSystem, environment, globber, log, arguments, processRunner, toolResolvers, registry);
              this.ArgOptions = ArgumentOptions.CommandLine;
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets the arguments.
        /// </summary>
        public ICakeArguments Arguments { get; private set; }

        /// <summary>
        /// Gets the environment.
        /// </summary>
        public ICakeEnvironment Environment { get; private set; }

        /// <summary>
        /// Gets the file system.
        /// </summary>
        public IFileSystem FileSystem { get; private set; }

        /// <summary>
        /// Gets the globber.
        /// </summary>
        public IGlobber Globber { get; private set; }

        /// <summary>
        /// Gets the log.
        /// </summary>
        public ICakeLog Log { get; private set; }

        /// <summary>
        /// Gets the options.
        /// </summary>
        public CakeTinOptions Options { get; private set; }

        /// <summary>
        /// Gets the process runner.
        /// </summary>
        public IProcessRunner ProcessRunner { get; private set; }

        /// <summary>
        /// Gets the registry.
        /// </summary>
        public IRegistry Registry { get; private set; }

        /// <summary>
        /// Gets the AppVeyor class for interaction on that build system.
        /// </summary>
        protected IAppVeyorProvider AppVeyor
        {
            get
              {
            return this.AppVeyor();
              }
        }

        /// <summary>
        /// Gets or sets the argument options.
        /// </summary>
        protected ArgumentOptions ArgOptions { get; set; }

        /// <summary>
        /// Gets the build system.
        /// </summary>
        protected BuildSystem BuildSystem
        {
            get { return this.BuildSystem(); }
        }

        /// <summary>
        /// Gets the IoC container.
        /// </summary>
        protected IContainer Container { get; private set; }

        #endregion Properties

        #region Methods

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            this.Container.Dispose();
        }

        /// <summary>
        /// Executes this instance.
        /// </summary>
        /// <returns>True if build successful; otherwise false</returns>
        public bool Execute()
        {
            try
              {
            // Parse options.
            var argumentParser = Container.Resolve<IArgumentParser>();
            var options = argumentParser.Parse(System.Environment.GetCommandLineArgs());
            if (options != null)
            {
              var log = this.Log as IVerbosityAwareLog;
              if (log != null)
              {
            log.SetVerbosity(options.Verbosity);
              }

              // Create the correct command and execute it.
              var command = CreateCommand(options);
              command.Execute(options);
              this.RunBuild();
              return true;
            }
              }
              catch (Exception ex)
              {
            if (this.Log.Verbosity == Verbosity.Diagnostic)
            {
              this.Log.Error("Error: {0}", ex);
            }
            else
            {
              this.Log.Error("Error: {0}", ex.Message);
            }
              }

              return false;
        }

        /// <summary>
        /// Gets resolver by tool name
        /// </summary>
        /// <param name="toolName">resolver tool name</param>
        /// <returns>
        /// IToolResolver for tool
        /// </returns>
        public IToolResolver GetToolResolver(string toolName)
        {
            var toolResolver = this.toolResolverLookup[toolName].FirstOrDefault();
              if (toolResolver == null)
              {
            throw new CakeException(string.Format(CultureInfo.InvariantCulture, "Failed to resolve tool: {0}", toolName));
              }
              return toolResolver;
        }

        /// <summary>
        /// Arguments the specified name.
        /// </summary>
        /// <typeparam name="TReturn">The type of the return object.</typeparam>
        /// <param name="name">The name.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns>The value of the Argument if found, otherwise the default value</returns>
        protected TReturn Argument<TReturn>(string name, TReturn defaultValue)
        {
            string value = null;

              if ((this.ArgOptions & ArgumentOptions.CommandLine) == ArgumentOptions.CommandLine)
              {
            value = this.Arguments.GetArgument(name);
              }

              if (value == null
              && (this.ArgOptions & ArgumentOptions.EnvironmentalVariables) == ArgumentOptions.EnvironmentalVariables)
              {
            value = this.EnvironmentVariable(name);
              }

              return value == null
            ? defaultValue
            : Convert<TReturn>(value);
        }

        /// <summary>
        /// Kicks off the actual build in the inherited class.
        /// </summary>
        protected abstract void RunBuild();

        /// <summary>
        /// Runs the specified target.
        /// </summary>
        /// <param name="target">The target to run.</param>
        protected void RunTarget(string target)
        {
            this.buildScriptHost.RunTarget(target);
        }

        /// <summary>
        /// Allows registration of an action that's executed before any tasks are run.
        /// If setup fails, no tasks will be executed but teardown will be performed.
        /// </summary>
        /// <param name="action">The action to be executed.</param>
        protected void Setup(Action action)
        {
            this.buildScriptHost.Setup(action);
        }

        protected CakeTaskBuilder<ActionTask> Task(CakeTask task)
        {
            return Task(task.Name)
              .Does(task.Execute);
        }

        /// <summary>
        /// Registers a new task.
        /// </summary>
        /// <param name="name">The name of the task.</param>
        /// <returns>A <see cref="CakeTaskBuilder{ActionTask}"/>.</returns>
        protected CakeTaskBuilder<ActionTask> Task(string name)
        {
            return this.buildScriptHost.Task(name);
        }

        /// <summary>
        /// Allows registration of an action that's executed after all other tasks have been run.
        /// If a setup action or a task fails with or without recovery, the specified teardown action will still be executed.
        /// </summary>
        /// <param name="action">The action to be executed.</param>
        protected void Teardown(Action action)
        {
            this.buildScriptHost.Teardown(action);
        }

        /// <summary>
        /// Converts the specified value to the return type.
        /// </summary>
        /// <typeparam name="TReturn">The type of the return.</typeparam>
        /// <param name="value">The value.</param>
        /// <returns>Converted value</returns>
        private static TReturn Convert<TReturn>(string value)
        {
            var converter = TypeDescriptor.GetConverter(typeof(TReturn));
              return (TReturn)converter.ConvertFromInvariantString(value);
        }

        /// <summary>
        /// Creates the IoC container.
        /// </summary>
        /// <returns>Container instance</returns>
        private static IContainer CreateContainer()
        {
            var builder = new ContainerBuilder();

              // Core services.
              builder.RegisterType<CakeEngine>().As<ICakeEngine>().SingleInstance();
              builder.RegisterType<FileSystem>().As<IFileSystem>().SingleInstance();
              builder.RegisterType<CakeEnvironment>().As<ICakeEnvironment>().SingleInstance();
              builder.RegisterType<CakeArguments>().As<ICakeArguments>().SingleInstance();
              builder.RegisterType<Globber>().As<IGlobber>().SingleInstance();
              builder.RegisterType<ProcessRunner>().As<IProcessRunner>().SingleInstance();
              builder.RegisterType<ScriptAliasFinder>().As<IScriptAliasFinder>().SingleInstance();
              builder.RegisterType<CakeReportPrinter>().As<ICakeReportPrinter>().SingleInstance();
              builder.RegisterType<CakeConsole>().As<IConsole>().SingleInstance();
              builder.RegisterType<ScriptProcessor>().As<IScriptProcessor>().SingleInstance();
              builder.RegisterCollection<IToolResolver>("toolResolvers").As<IEnumerable<IToolResolver>>();
              builder.RegisterType<NuGetToolResolver>().As<IToolResolver>().As<INuGetToolResolver>().SingleInstance().MemberOf("toolResolvers");
              builder.RegisterType<WindowsRegistry>().As<IRegistry>().SingleInstance();
              builder.RegisterType<CakeContext>().As<ICakeContext>().SingleInstance();

              ////// Roslyn related services.
              ////builder.RegisterType<RoslynScriptEngine>().As<IScriptEngine>().SingleInstance();
              ////builder.RegisterType<RoslynScriptSessionFactory>().SingleInstance();
              ////builder.RegisterType<RoslynNightlyScriptSessionFactory>().SingleInstance();

              // Cake services.
              builder.RegisterType<ArgumentParser>().As<IArgumentParser>().SingleInstance();
              builder.RegisterType<CommandFactory>().As<ICommandFactory>().SingleInstance();
              //builder.RegisterType<CakeApplication>().SingleInstance();
              builder.RegisterType<ScriptRunner>().As<IScriptRunner>().SingleInstance();
              builder.RegisterType<CakeBuildLog>().As<ICakeLog>().As<IVerbosityAwareLog>().SingleInstance();

              // Register script hosts.
              builder.RegisterType<BuildScriptHost>().SingleInstance();
              builder.RegisterType<DescriptionScriptHost>().SingleInstance();
              builder.RegisterType<DryRunScriptHost>().SingleInstance();

              // Register commands.
              builder.RegisterType<BuildCommand>().AsSelf().InstancePerDependency();
              builder.RegisterType<DescriptionCommand>().AsSelf().InstancePerDependency();
              builder.RegisterType<DryRunCommand>().AsSelf().InstancePerDependency();
              builder.RegisterType<HelpCommand>().AsSelf().InstancePerDependency();
              builder.RegisterType<VersionCommand>().AsSelf().InstancePerDependency();

              return builder.Build();
        }

        /// <summary>
        /// Creates the command.
        /// </summary>
        /// <param name="options">The options.</param>
        /// <returns>Command for executing</returns>
        private ICommand CreateCommand(CakeOptions options)
        {
            var commandFactory = this.Container.Resolve<ICommandFactory>();
              if (options != null)
              {
            if (options.ShowHelp)
            {
              return commandFactory.CreateHelpCommand();
            }

            if (options.ShowVersion)
            {
              return commandFactory.CreateVersionCommand();
            }

            if (options.Script != null)
            {
              if (options.PerformDryRun)
              {
            return commandFactory.CreateDryRunCommand();
              }

              if (options.ShowDescription)
              {
            return commandFactory.CreateDescriptionCommand();
              }

              return commandFactory.CreateBuildCommand();
            }
              }

              this.Log.Error("Could not find a build script to execute.");
              this.Log.Error("Either the first argument must the build script's path,");
              this.Log.Error("or build script should follow default script name conventions.");

              return new ErrorCommandDecorator(commandFactory.CreateHelpCommand());
        }

        private void SetProperties(
            IFileSystem fileSystem,
            ICakeEnvironment environment,
            IGlobber globber,
            ICakeLog log,
            ICakeArguments arguments,
            IProcessRunner processRunner,
            IEnumerable<IToolResolver> toolResolvers,
            IRegistry registry)
        {
            if (fileSystem == null)
              {
            throw new ArgumentNullException("fileSystem");
              }
              if (environment == null)
              {
            throw new ArgumentNullException("environment");
              }
              if (globber == null)
              {
            throw new ArgumentNullException("globber");
              }
              if (log == null)
              {
            throw new ArgumentNullException("log");
              }
              if (arguments == null)
              {
            throw new ArgumentNullException("arguments");
              }
              if (processRunner == null)
              {
            throw new ArgumentNullException("processRunner");
              }
              if (toolResolvers == null)
              {
            throw new ArgumentNullException("toolResolvers");
              }

              this.FileSystem = fileSystem;
              this.Environment = environment;
              this.Globber = globber;
              this.Log = log;
              this.Arguments = arguments;
              this.ProcessRunner = processRunner;

              // Create the tool resolver lookup table.
              this.toolResolverLookup = toolResolvers.ToLookup(key => key.Name, value => value, StringComparer.OrdinalIgnoreCase);

              this.Registry = registry;
              this.buildScriptHost = new BuildScriptHost(this.Container.Resolve<ICakeEngine>(), this, this.Container.Resolve<ICakeReportPrinter>(), this.Log);
        }

        #endregion Methods
    }
}