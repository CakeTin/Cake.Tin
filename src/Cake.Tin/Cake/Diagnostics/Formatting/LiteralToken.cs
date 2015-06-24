// -----------------------------------------------------------------------
// <copyright file="LiteralToken.cs" company="Mark Walker">
//     Copyright (c) 2015, Mark Walker and contributors. Based on Cake - Copyright (c) 2014, Patrik Svensson and contributors.
// </copyright>
// -----------------------------------------------------------------------
namespace Cake.Diagnostics.Formatting
{
    internal sealed class LiteralToken : FormatToken
    {
        #region Fields

        /// <summary>_text</summary>
        private readonly string _text;

        #endregion Fields

        #region Constructors

        public LiteralToken(string text)
        {
            _text = text;
        }

        #endregion Constructors

        #region Properties

        public string Text
        {
            get { return _text; }
        }

        #endregion Properties

        #region Methods

        public override string Render(object[] args)
        {
            return _text;
        }

        #endregion Methods
    }
}