namespace Cake.Tin.Enums
{
  using System;

  /// <summary>
  /// Argument Options
  /// </summary>
  [Flags]
  public enum ArgumentOptions
  {
    /// <summary>Allow arguments from the command line</summary>
    CommandLine = 1,

    /// <summary>Allow arguments from Environmental Variables</summary>
    EnvironmentalVariables = 2,
  }
}