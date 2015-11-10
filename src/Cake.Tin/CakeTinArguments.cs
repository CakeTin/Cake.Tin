// -----------------------------------------------------------------------
// <copyright file="CakeTinArguments.cs" company="Mark Walker">
//     Copyright (c) 2015, Mark Walker and contributors. Based on Cake - Copyright (c) 2014, Patrik Svensson and contributors.
// </copyright>
// -----------------------------------------------------------------------
namespace Cake.Tin
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Linq;

    using Cake.Core;
    using Cake.Tin.Enums;

    /// <summary>
    /// The Cake tin arguments
    /// </summary>
    internal class CakeTinArguments : ICakeArguments
    {
        #region Fields

        /// <summary>The command line arguments</summary>
        private IDictionary<string, string> arguments = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        #endregion Fields

        #region Constructors

        internal CakeTinArguments()
        {
            this.ArgumentOptions = ArgumentOptions.ApplicationSettings | ArgumentOptions.CommandLine
                                   | ArgumentOptions.EnvironmentalVariables;
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets or sets the argument options.
        /// </summary>
        public ArgumentOptions ArgumentOptions { get; set; }

        #endregion Properties

        #region Methods

        /// <summary>
        /// Gets an argument.
        /// </summary>
        /// <param name="name">The argument name.</param>
        /// <returns>
        /// The argument value.
        /// </returns>
        public string GetArgument(string name)
        {
            return this.GetArgument(name, null);
        }

        /// <summary>
        /// Gets an argument.
        /// </summary>
        /// <param name="name">The argument name.</param>
        /// <param name="defaultIfMissing"></param>
        /// <returns>
        /// The argument value.
        /// </returns>
        /// ReSharper disable once MethodOverloadWithOptionalParameter
        public string GetArgument(string name, string defaultIfMissing = null)
        {
            string value = null;

            if ((this.ArgumentOptions & ArgumentOptions.CommandLine) == ArgumentOptions.CommandLine)
            {
                this.arguments.TryGetValue(name, out value);
            }

            if (value == null && (this.ArgumentOptions & ArgumentOptions.EnvironmentalVariables) == ArgumentOptions.EnvironmentalVariables)
            {
                value = Environment.GetEnvironmentVariable(name);
            }

            if (value == null && (this.ArgumentOptions & ArgumentOptions.ApplicationSettings) == ArgumentOptions.ApplicationSettings)
            {
                value = ConfigurationManager.AppSettings[name];
            }

            return value ?? defaultIfMissing;
        }

        /// <summary>
        /// Determines whether or not the specified argument exist.
        /// </summary>
        /// <param name="name">The argument name.</param>
        /// <returns>
        /// <c>true</c> if the argument exist; otherwise <c>false</c>.
        /// </returns>
        public bool HasArgument(string name)
        {
            if ((this.ArgumentOptions & ArgumentOptions.CommandLine) == ArgumentOptions.CommandLine)
            {
                if (this.arguments.ContainsKey(name))
                {
                    return true;
                }
            }

            if ((this.ArgumentOptions & ArgumentOptions.EnvironmentalVariables) == ArgumentOptions.EnvironmentalVariables)
            {
                if (Environment.GetEnvironmentVariable(name) != null)
                {
                    return true;
                }
            }

            if ((this.ArgumentOptions & ArgumentOptions.ApplicationSettings) == ArgumentOptions.ApplicationSettings)
            {
                if (ConfigurationManager.AppSettings.AllKeys.Contains(name))
                {
                    return true;
                }
            }

            return false;
        }

        public void SetArgument(string name, string value)
        {
            this.arguments.Add(name, value);
        }

        /// <summary>
        /// Initializes the argument list.
        /// </summary>
        /// <param name="args">The arguments.</param>
        public void SetArguments(IDictionary<string, string> args)
        {
            this.arguments = args;
        }

        #endregion Methods
    }
}