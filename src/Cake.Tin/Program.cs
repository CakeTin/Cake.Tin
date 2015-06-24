using System;

namespace Cake.Tin
{
  public class Program
  {
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
  }
}
