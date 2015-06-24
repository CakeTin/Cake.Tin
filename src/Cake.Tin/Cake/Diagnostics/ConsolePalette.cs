// -----------------------------------------------------------------------
// <copyright file="ConsolePalette.cs" company="Mark Walker">
//     Copyright (c) 2015, Mark Walker and contributors. Based on Cake - Copyright (c) 2014, Patrik Svensson and contributors.
// </copyright>
// -----------------------------------------------------------------------
namespace Cake.Diagnostics
{
    using System;

    internal sealed class ConsolePalette
    {
        #region Constructors

        public ConsolePalette(ConsoleColor background, ConsoleColor foreground, 
            ConsoleColor argumentBackground, ConsoleColor argumentForeground)
        {
            Background = background;
            Foreground = foreground;
            ArgumentBackground = argumentBackground;
            ArgumentForeground = argumentForeground;
        }

        #endregion Constructors

        #region Properties

        public ConsoleColor ArgumentBackground { get; set; }

        public ConsoleColor ArgumentForeground { get; set; }

        public ConsoleColor Background { get; set; }

        public ConsoleColor Foreground { get; set; }

        #endregion Properties
    }
}