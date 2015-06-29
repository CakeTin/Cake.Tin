// -----------------------------------------------------------------------
// <copyright file="CakeTinOptions.cs" company="Mark Walker">
//     Copyright (c) 2015, Mark Walker and contributors. Based on Cake - Copyright (c) 2014, Patrik Svensson and contributors.
// </copyright>
// -----------------------------------------------------------------------
namespace Cake.Tin
{
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

        /// <summary>
        /// Gets or sets the cake options.
        /// </summary>
        /// <value>
        /// The cake options.
        /// </value>
        public CakeOptions CakeOptions { get; set; }

        #endregion Properties
    }
}