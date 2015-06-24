// -----------------------------------------------------------------------
// <copyright file="CakeTinOptions.cs" company="Patrik Svensson and contributors.">
//     Copyright (c) 2014, Patrik Svensson and contributors.
// </copyright>
// -----------------------------------------------------------------------
namespace Cake.Tin
{
  using System.Collections.Generic;

  using Cake.Core.Diagnostics;

  /// <summary>
  /// RunBuild options
  /// </summary>
  public class CakeTinOptions
  {
    #region Constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="CakeTinOptions"/> class.
    /// </summary>
    /// <param name="cakeOptions">The cake options.</param>
    public CakeTinOptions(CakeOptions cakeOptions)
    {
      this.CakeOptions = cakeOptions;
    }

    #endregion Constructors

    #region Properties

    public CakeOptions CakeOptions { get; set; }

    #endregion Properties
  }
}