// -----------------------------------------------------------------------
// <copyright file="CommandFactory.cs" company="Mark Walker">
//     Copyright (c) 2015, Mark Walker and contributors. Based on Cake - Copyright (c) 2014, Patrik Svensson and contributors.
// </copyright>
// -----------------------------------------------------------------------
namespace Cake.Commands
{
    internal sealed class CommandFactory : ICommandFactory
    {
        #region Fields

        /// <summary>_build command factory</summary>
        private readonly BuildCommand.Factory _buildCommandFactory;

        /// <summary>_description command factory</summary>
        private readonly DescriptionCommand.Factory _descriptionCommandFactory;

        /// <summary>_dry run command factory</summary>
        private readonly DryRunCommand.Factory _dryRunCommandFactory;

        /// <summary>_help command factory</summary>
        private readonly HelpCommand.Factory _helpCommandFactory;

        /// <summary>_version command factory</summary>
        private readonly VersionCommand.Factory _versionCommandFactory;

        #endregion Fields

        #region Constructors

        public CommandFactory(
            BuildCommand.Factory buildCommandFactory,
            DescriptionCommand.Factory descriptionCommandFactory,
            DryRunCommand.Factory dryRunCommandFactory,
            HelpCommand.Factory helpCommandFactory,
            VersionCommand.Factory versionCommandFactory)
        {
            _buildCommandFactory = buildCommandFactory;
            _descriptionCommandFactory = descriptionCommandFactory;
            _dryRunCommandFactory = dryRunCommandFactory;
            _helpCommandFactory = helpCommandFactory;
            _versionCommandFactory = versionCommandFactory;
        }

        #endregion Constructors

        #region Methods

        public ICommand CreateBuildCommand()
        {
            return _buildCommandFactory();
        }

        public ICommand CreateDescriptionCommand()
        {
            return _descriptionCommandFactory();
        }

        public ICommand CreateDryRunCommand()
        {
            return _dryRunCommandFactory();
        }

        public ICommand CreateHelpCommand()
        {
            return _helpCommandFactory();
        }

        public ICommand CreateVersionCommand()
        {
            return _versionCommandFactory();
        }

        #endregion Methods
    }
}