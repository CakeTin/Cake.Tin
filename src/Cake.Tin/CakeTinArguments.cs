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

            return value;
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
                return Environment.GetEnvironmentVariable(name) != null;
            }

            if ((this.ArgumentOptions & ArgumentOptions.ApplicationSettings) == ArgumentOptions.ApplicationSettings)
            {
                return ConfigurationManager.AppSettings.AllKeys.Contains(name);
            }

            return false;
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