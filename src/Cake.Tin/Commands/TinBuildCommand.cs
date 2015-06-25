using Cake.Commands;

namespace Cake.Tin.Commands
{
  internal class TinBuildCommand : ICommand
  {
    private readonly CakeTinBase cakeTinBase;

    public TinBuildCommand(CakeTinBase cakeTinBase)
    {
      this.cakeTinBase = cakeTinBase;
    }

    public bool Execute(CakeOptions options)
    {
      this.cakeTinBase.CreateAndExecuteBuild();
      return true;
    }
  }
}