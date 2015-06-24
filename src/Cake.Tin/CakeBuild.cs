using System;
using System.Collections.Generic;
using System.Linq;
using Cake.Common.IO.Paths;
using Cake.Common.Solution.Project.Properties;
using Cake.Common.Tools.MSBuild;
using Cake.Common.Tools.NuGet.Pack;
using Cake.Common.Tools.NuGet.Push;
using Cake.Common.Tools.NuGet.Restore;
using Cake.Common.Tools.XUnit;
using Cake.Core;
using Cake.Core.IO;

namespace Cake.Tin
{
  using Cake.Common;
  using Cake.Common.Diagnostics;
  using Cake.Common.IO;
  using Cake.Common.Tools.NuGet;

  /// <summary>
  /// Cake tin build script for Cake
  /// </summary>
  internal class CakeBuild : CakeTinBase
  {
    /// <summary>
    /// Kicks off the actual build in the inherited class.
    /// </summary>
    protected override void RunBuild()
    {
      //////////////////////////////////////////////////////////////////////
      // ARGUMENTS
      //////////////////////////////////////////////////////////////////////

      var target = Argument("target", "Default");
      var configuration = Argument("configuration", "Release");

      //////////////////////////////////////////////////////////////////////
      // PREPARATION
      //////////////////////////////////////////////////////////////////////

      // Get whether or not this is a local build.
      var local = BuildSystem.IsLocalBuild;
      var isRunningOnAppVeyor = AppVeyor.IsRunningOnAppVeyor;
      var isPullRequest = AppVeyor.Environment.PullRequest.IsPullRequest;

      // Parse release notes.
      var releaseNotes = this.ParseReleaseNotes("./ReleaseNotes.md");

      // Get version.
      var buildNumber = AppVeyor.Environment.Build.Number;
      var version = releaseNotes.Version.ToString();
      var semVersion = local ? version : (version + string.Concat("-build-", buildNumber));

      // Define directories.
      var buildDir = this.Directory("./src/Cake/bin") + this.Directory(configuration);
      var buildResultDir = this.Directory("./build") + this.Directory("v" + semVersion);
      var testResultsDir = buildResultDir + this.Directory("test-results");
      var nugetRoot = buildResultDir + this.Directory("nuget");
      var binDir = buildResultDir + this.Directory("bin");

      ///////////////////////////////////////////////////////////////////////////////
      // SETUP / TEARDOWN
      ///////////////////////////////////////////////////////////////////////////////

      Setup(() => this.Information("Building version {0} of Cake.", semVersion));

      //////////////////////////////////////////////////////////////////////
      // TASKS
      //////////////////////////////////////////////////////////////////////
      CakeTask x = new CleanTask("Clean", buildResultDir, binDir, testResultsDir, nugetRoot);
      Task(x);
      Task("Clean")
          .Does(() => this.CleanDirectories(new string[] { buildResultDir, binDir, testResultsDir, nugetRoot }));

      Task("Restore-NuGet-Packages")
          .IsDependentOn("Clean")
          .Does(
              () =>
              this.NuGetRestore(
                  "./src/Cake.sln",
                  new NuGetRestoreSettings
                      {
                        Source =
                            new List<string>
                                        {
                                            "https://www.nuget.org/api/v2/",
                                            "https://www.myget.org/F/roslyn-nightly/"
                                        }
                      }));

      Task("Patch-Assembly-Info").IsDependentOn("Restore-NuGet-Packages").Does(
          () =>
          {
            const string File = "./src/SolutionInfo.cs";
            this.CreateAssemblyInfo(
                File,
                new AssemblyInfoSettings
                    {
                      Product = "Cake",
                      Version = version,
                      FileVersion = version,
                      InformationalVersion = semVersion,
                      Copyright =
                          "Copyright (c) Patrik Svensson, Mattias Karlsson and contributors"
                    });
          });

      Task("Build")
          .IsDependentOn("Patch-Assembly-Info")
          .Does(
              () => this.MSBuild(
                  "./src/Cake.sln",
                  settings =>
                  settings.SetConfiguration(configuration)
                      .WithProperty("TreatWarningsAsErrors", "true")
                      .UseToolVersion(MSBuildToolVersion.NET45)
                      .SetNodeReuse(false)));

      Task("Run-Unit-Tests")
          .IsDependentOn("Build")
          .Does(
              () => this.XUnit2(
                  "./src/**/bin/" + configuration + "/*.Tests.dll",
                  new XUnit2Settings
                      {
                        OutputDirectory = testResultsDir,
                        XmlReportV1 = true
                      }));

      Task("Copy-Files").IsDependentOn("Run-Unit-Tests").Does(
          () =>
          {
            this.CopyFileToDirectory(buildDir + this.File("Cake.exe"), binDir);
            this.CopyFileToDirectory(buildDir + this.File("Cake.Core.dll"), binDir);
            this.CopyFileToDirectory(buildDir + this.File("Cake.Core.xml"), binDir);
            this.CopyFileToDirectory(buildDir + this.File("Cake.Core.pdb"), binDir);
            this.CopyFileToDirectory(buildDir + this.File("Cake.Common.dll"), binDir);
            this.CopyFileToDirectory(buildDir + this.File("Cake.Common.xml"), binDir);
            this.CopyFileToDirectory(buildDir + this.File("Autofac.dll"), binDir);
            this.CopyFileToDirectory(buildDir + this.File("Nuget.Core.dll"), binDir);

            this.CopyFiles(new FilePath[] { "LICENSE", "README.md", "ReleaseNotes.md" }, binDir);
          });

      Task("Zip-Files").IsDependentOn("Copy-Files").Does(
          () =>
          {
            var packageFile = this.File("Cake-bin-v" + semVersion + ".zip");
            var packagePath = buildResultDir + packageFile;

            this.Zip(binDir, packagePath);
          });

      Task("Create-NuGet-Packages").IsDependentOn("Copy-Files").Does(
          () =>
          {
            // Create Cake package.
            this.NuGetPack(
                "./nuspec/Cake.nuspec",
                new NuGetPackSettings
                    {
                      Version = semVersion,
                      ReleaseNotes = releaseNotes.Notes.ToArray(),
                      BasePath = binDir,
                      OutputDirectory = nugetRoot,
                      Symbols = false,
                      NoPackageAnalysis = true
                    });

            // Create core package.
            this.NuGetPack(
                "./nuspec/Cake.Core.nuspec",
                new NuGetPackSettings
                    {
                      Version = semVersion,
                      ReleaseNotes = releaseNotes.Notes.ToArray(),
                      BasePath = binDir,
                      OutputDirectory = nugetRoot,
                      Symbols = false
                    });
          });

      //Task("Update-AppVeyor-Build-Number")
      //    .WithCriteria(() => isRunningOnAppVeyor)
      //    .Does(() => this.AppVeyor.UpdateBuildVersion(semVersion));

      Task("Upload-AppVeyor-Artifacts")
          .IsDependentOn("Package")
          .WithCriteria(() => isRunningOnAppVeyor)
          .Does(
              () =>
              {
                var artifact = buildResultDir + this.File("Cake-bin-v" + semVersion + ".zip");
                AppVeyor.UploadArtifact(artifact);
              });

      Task("Publish-MyGet").WithCriteria(() => !local).WithCriteria(() => !isPullRequest).Does(
          () =>
          {
            // Resolve the API key.
            var apiKey = this.EnvironmentVariable("MYGET_API_KEY");
            if (string.IsNullOrEmpty(apiKey))
            {
              throw new InvalidOperationException("Could not resolve MyGet API key.");
            }

            // Get the path to the package.
            var package = nugetRoot + this.File("Cake." + semVersion + ".nupkg");

            // Push the package.
            this.NuGetPush(
                package,
                new NuGetPushSettings
                    {
                      Source = "https://www.myget.org/F/cake/api/v2/package",
                      ApiKey = apiKey
                    });
          });

      //////////////////////////////////////////////////////////////////////
      // TASK TARGETS
      //////////////////////////////////////////////////////////////////////

      Task("Package").IsDependentOn("Zip-Files").IsDependentOn("Create-NuGet-Packages");

      Task("Default").IsDependentOn("Package");

      Task("AppVeyor")
          .IsDependentOn("Update-AppVeyor-Build-Number")
          .IsDependentOn("Upload-AppVeyor-Artifacts")
          .IsDependentOn("Publish-MyGet");

      //////////////////////////////////////////////////////////////////////
      // EXECUTION
      //////////////////////////////////////////////////////////////////////

      RunTarget(target);
    }
  }

  internal class CleanTask : CakeTask
  {
    private readonly ConvertableDirectoryPath[] pathsToClean;

    public CleanTask(string name, params ConvertableDirectoryPath[] pathsToClean)
      : base(name)
    {
      this.pathsToClean = pathsToClean;
    }

    public override void Execute(ICakeContext context)
    {
      context.CleanDirectories(this.pathsToClean.Select(p => p.Path));
    }
  }

}