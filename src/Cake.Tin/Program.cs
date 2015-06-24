// -----------------------------------------------------------------------
// <copyright file="Program.cs" company="Mark Walker">
//     Copyright (c) 2015, Mark Walker and contributors. Based on Cake - Copyright (c) 2014, Patrik Svensson and contributors.
// </copyright>
// -----------------------------------------------------------------------
namespace Cake.Tin
{
    using System;

    public class Program
    {
        #region Methods

        public static void Main(string[] args)
        {
            if (args != null)
              {
            using (var build = new GenericBuild())
            {
              Environment.ExitCode = build.Execute() ? 0 : 1;
            }
              }
              else
              {
            using (var build = new CakeBuild())
            {
              Environment.ExitCode = build.Execute() ? 0 : 1;
            }
              }
        }

        #endregion Methods
    }
}