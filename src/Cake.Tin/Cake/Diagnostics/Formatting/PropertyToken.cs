// -----------------------------------------------------------------------
// <copyright file="PropertyToken.cs" company="Mark Walker">
//     Copyright (c) 2015, Mark Walker and contributors. Based on Cake - Copyright (c) 2014, Patrik Svensson and contributors.
// </copyright>
// -----------------------------------------------------------------------
namespace Cake.Diagnostics.Formatting
{
    using System;
    using System.Globalization;

    internal sealed class PropertyToken : FormatToken
    {
        #region Fields

        /// <summary>_format</summary>
        private readonly string _format;

        /// <summary>_position</summary>
        private readonly int _position;

        #endregion Fields

        #region Constructors

        public PropertyToken(int position, string format)
        {
            _position = position;
            _format = format;
        }

        #endregion Constructors

        #region Properties

        public string Format
        {
            get { return _format; }
        }

        public int Position
        {
            get { return _position; }
        }

        #endregion Properties

        #region Methods

        public override string Render(object[] args)
        {
            var value = args[_position];
            if (!string.IsNullOrWhiteSpace(_format))
            {
                var formattable = value as IFormattable;
                if (formattable != null)
                {
                    return formattable.ToString(_format, CultureInfo.InvariantCulture);
                }
            }
            return (value == null)
                ? "[NULL]"
                : value.ToString();
        }

        #endregion Methods
    }
}