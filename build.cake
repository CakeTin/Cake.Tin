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

EnsureCakeVersionInReleaseNotes();

// Parse release notes.
var releaseNotes = ParseReleaseNotes("./ReleaseNotes.md");

// Get version.
var buildNumber = AppVeyor.Environment.Build.Number;
var version = releaseNotes.Version.ToString();
var semVersion = local ? version : (version + string.Concat("-build-", buildNumber));

// Define directories.
var buildDir = Directory("./src/Cake.Tin/bin") + Directory(configuration);
var buildResultDir = Directory("./build") + Directory("v" + semVersion);
var testResultsDir = buildResultDir + Directory("test-results");
var nugetRoot = buildResultDir + Directory("nuget");
var binDir = buildResultDir + Directory("bin");

///////////////////////////////////////////////////////////////////////////////
// SETUP / TEARDOWN
///////////////////////////////////////////////////////////////////////////////

Setup(() =>
{
    Information("Building version {0} of Cake.Tin.", semVersion);
});

//////////////////////////////////////////////////////////////////////
// TASKS
//////////////////////////////////////////////////////////////////////

Task("Clean")
    .Does(() =>
{
    CleanDirectories(new DirectoryPath[] {
        buildResultDir, binDir, testResultsDir, nugetRoot});
});

Task("Restore-NuGet-Packages")
    .IsDependentOn("Clean")
    .Does(() =>
{
    NuGetRestore("./src/Cake.Tin.sln", new NuGetRestoreSettings {
        Source = new List<string> {
            "https://www.nuget.org/api/v2/",
            "https://www.myget.org/F/roslyn-nightly/"
        }
    });
});

Task("Patch-Assembly-Info")
    .IsDependentOn("Restore-NuGet-Packages")
    .Does(() =>
{
    var file = "./src/SolutionInfo.cs";
    CreateAssemblyInfo(file, new AssemblyInfoSettings {
        Product = "Cake.Tin",
        Version = version,
        FileVersion = version,
        InformationalVersion = semVersion,
        Copyright = "Copyright (c) Patrik Svensson, Mattias Karlsson and contributors"
    });
});

Task("Build")
    .IsDependentOn("Restore-NuGet-Packages")
    .Does(() =>
{
    MSBuild("./src/Cake.Tin.sln", settings =>
        settings.SetConfiguration(configuration)
            .WithProperty("TreatWarningsAsErrors", "true")
            .UseToolVersion(MSBuildToolVersion.NET45)
            .SetNodeReuse(false));
});

Task("Run-Unit-Tests")
    .IsDependentOn("Build")
    .Does(() =>
{
    XUnit2("./src/**/bin/" + configuration + "/*.Tests.dll", new XUnit2Settings {
        OutputDirectory = testResultsDir,
        XmlReportV1 = true
    });
});


Task("Copy-Files")
    .IsDependentOn("Run-Unit-Tests")
    .Does(() =>
{
    CopyFileToDirectory(buildDir + File("Cake.Tin.dll"), binDir);
    CopyFileToDirectory(buildDir + File("Cake.exe"), binDir);
    CopyFileToDirectory(buildDir + File("Cake.Core.dll"), binDir);
    CopyFileToDirectory(buildDir + File("Cake.Core.xml"), binDir);
    CopyFileToDirectory(buildDir + File("Cake.Common.dll"), binDir);
    CopyFileToDirectory(buildDir + File("Cake.Common.xml"), binDir);
    CopyFileToDirectory(buildDir + File("Autofac.dll"), binDir);
    CopyFileToDirectory(buildDir + File("Nuget.Core.dll"), binDir);

    CopyFiles(new FilePath[] { "LICENSE", "README.md", "ReleaseNotes.md" }, binDir);
});

Task("Zip-Files")
    .IsDependentOn("Copy-Files")
    .Does(() =>
{
    var packageFile = File("Cake-bin-v" + semVersion + ".zip");
    var packagePath = buildResultDir + packageFile;

    Zip(binDir, packagePath);
});

Task("Create-NuGet-Packages")
    .IsDependentOn("Copy-Files")
    .Does(() =>
{
    // Create Cake package.
    NuGetPack("./nuspec/Cake.Tin.nuspec", new NuGetPackSettings {
        Version = semVersion,
        ReleaseNotes = releaseNotes.Notes.ToArray(),
        BasePath = binDir,
        OutputDirectory = nugetRoot,
        Symbols = false,
        NoPackageAnalysis = true
    });
});

Task("Update-AppVeyor-Build-Number")
    .WithCriteria(() => isRunningOnAppVeyor)
    .Does(() =>
{
    AppVeyor.UpdateBuildVersion(semVersion);
});

Task("Upload-AppVeyor-Artifacts")
    .IsDependentOn("Package")
    .WithCriteria(() => isRunningOnAppVeyor)
    .Does(() =>
{
    var artifact = buildResultDir + File("Cake-bin-v" + semVersion + ".zip");
    AppVeyor.UploadArtifact(artifact);
});

Task("Publish-MyGet")
    .WithCriteria(() => !local)
    .WithCriteria(() => !isPullRequest)
    .Does(() =>
{
    // Resolve the API key.
    var apiKey = EnvironmentVariable("MYGET_API_KEY");
    if(string.IsNullOrEmpty(apiKey)) {
        throw new InvalidOperationException("Could not resolve MyGet API key.");
    }

    // Get the path to the package.
    var package = nugetRoot + File("Cake.Tin." + semVersion + ".nupkg");

    // Push the package.
    NuGetPush(package, new NuGetPushSettings {
        Source = "https://www.myget.org/F/caketin/api/v2/package",
        ApiKey = apiKey
    });
});

//////////////////////////////////////////////////////////////////////
// TASK TARGETS
//////////////////////////////////////////////////////////////////////

Task("Package")
    .IsDependentOn("Zip-Files")
    .IsDependentOn("Create-NuGet-Packages");

Task("Default")
    .IsDependentOn("Package");

Task("AppVeyor")
    .IsDependentOn("Update-AppVeyor-Build-Number")
    .IsDependentOn("Upload-AppVeyor-Artifacts")
    .IsDependentOn("Publish-MyGet");

//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////

RunTarget(target);

      private void EnsureCakeVersionInReleaseNotes()
        {
          bool updated = false;
		  List<string> lines = null;
          const string fileName = "ReleaseNotes.md";
          var releaseNotes = ParseReleaseNotes(fileName);
          var cakeVersion = System.Diagnostics.FileVersionInfo.GetVersionInfo(@"tools\Cake\Cake.exe").FileVersion;
          string cakeVersionNote = "Built against Cake v"; ;
          var note = releaseNotes.Notes.FirstOrDefault(n => n.StartsWith(cakeVersionNote));
          if (note == null)
          {
            // No cake version mentioned, add it
            lines = System.IO.File.ReadAllLines(fileName).ToList();
            int lineIndex = -1;
            do
            {
              lineIndex++;
            } while (lines[lineIndex].Trim() == String.Empty);
            lines.Insert(lineIndex + 1, "* " + cakeVersionNote + cakeVersion);
            updated = true;
          }
          else if (!note.EndsWith(cakeVersion))
          {
            // Already released against an older version of Cake, add new release notes
            Version version = releaseNotes.Version;
            version = new Version(version.Major, version.Minor, version.Build + 1);
            lines = System.IO.File.ReadAllLines(fileName).ToList();
            lines.Insert(0, "");
            lines.Insert(0, "* " + cakeVersionNote + cakeVersion);
            lines.Insert(0, String.Format("### New in {0} (Released {1})", version.ToString(3), DateTime.Today.ToString("yyyy/MM/dd")));
            updated = true;
          }

          if (updated)
          {
			 Information("Updating release notes");
             System.IO.File.WriteAllLines(fileName, lines);
             RunGit("config --global credential.helper store");
             RunGit("config --global user.email \"mark@walkersretreat.co.nz\"");
             RunGit("config --global user.name \"Mark Walker\"");
             string token = Argument("gittoken", "458dcfd1abb8dae1e4e19a27d68472cfb8940501");
             string credentialsStore = System.Environment.ExpandEnvironmentVariables(@"%USERPROFILE%\.git-credentials");
             System.IO.File.AppendAllText(credentialsStore,string.Format("https://{0}:x-oauth-basic@github.com\n", token)); 
             RunGit("add " + fileName);
             RunGit("commit -m\"Update release notes\"");
             RunGit("push");
          }
		  else
		  {
			 Information("Release notes up to date");
		  }
        }

        private void RunGit(string arguments)
        {
            IEnumerable<string> output;
            var exitCode = StartProcess("git", new ProcessSettings
            {
              Arguments = arguments, 
              Timeout = (int)TimeSpan.FromMinutes(1).TotalMilliseconds,
              RedirectStandardOutput = true
            }, out output);

            foreach (var line in output)
            {
                Information(line);
            }

            if (exitCode != 0)
            {
                Information("Git returned {0}", exitCode);
                throw new Exception("Git Error");
            }
        }