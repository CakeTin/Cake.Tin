//////////////////////////////////////////////////////////////////////
// ARGUMENTS
//////////////////////////////////////////////////////////////////////

var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");
var branchName = GetGitBranch();

Information("Branch is '{0}'", branchName);

//////////////////////////////////////////////////////////////////////
// PREPARATION
//////////////////////////////////////////////////////////////////////

// Get whether or not this is a local build.
var isLocalBuild = BuildSystem.IsLocalBuild;
var isRunningOnAppVeyor = AppVeyor.IsRunningOnAppVeyor;
var isPullRequest = AppVeyor.Environment.PullRequest.IsPullRequest;

EnsureCakeVersionInReleaseNotes();

// Parse release notes.
var releaseNotes = ParseReleaseNotes("./ReleaseNotes.md");

// Get version.
var buildNumber = AppVeyor.Environment.Build.Number;
var version = releaseNotes.Version.ToString();
// Define directories.
var buildDir = Directory("./src/Cake.Tin/bin") + Directory(configuration);
var buildResultDir = Directory("./build") + Directory("v" + semVersion);
var testResultsDir = buildResultDir + Directory("test-results");
var nugetRoot = buildResultDir + Directory("nuget");
var binDir = buildResultDir + Directory("bin");
var isMasterBranch = branchName == "master";
var semVersion = isLocalBuild || isMasterBranch ? version : (version + string.Concat("-pre-", buildNumber));

var assemblyInfo        = new AssemblyInfoSettings {
                                Title                   = "Cake.Tin",
                                Description             = "Cake Tin - a wrapper for Cake (C# Make)",
                                Product                 = "Cake.Tin",
                                Company                 = "Mark Walker",
                                Version                 = version,
                                FileVersion             = version,
                                InformationalVersion    = semVersion,
                                Copyright               = string.Format("Copyright (c) Mark Walker {0}. Includes Cake which is Copyright (c) Patrik Svensson, Mattias Karlsson and contributors", DateTime.Now.Year),
                                CLSCompliant            = true
                            };
var nuspecFiles = new [] 
{
    new NuSpecContent {Source = "Cake.Tin.exe"},
    new NuSpecContent {Source = "Cake.exe"},
    new NuSpecContent {Source = "Cake.Core.dll"},
    new NuSpecContent {Source = "Cake.Core.xml"},
    new NuSpecContent {Source = "Cake.Common.dll"},
    new NuSpecContent {Source = "Cake.Common.xml"},
    new NuSpecContent {Source = "Autofac.dll"},
    new NuSpecContent {Source = "Nuget.Core.dll"},
    new NuSpecContent {Source = "LICENSE"},
    new NuSpecContent {Source = "README.md"},
    new NuSpecContent {Source = "ReleaseNotes.md"},
};
var nuGetPackSettings   = new NuGetPackSettings {
                                Id                      = assemblyInfo.Product,
                                Version                 = assemblyInfo.InformationalVersion,
                                Title                   = assemblyInfo.Title,
                                Authors                 = new[] {assemblyInfo.Company},
                                Owners                  = new[] {assemblyInfo.Company},
                                Description             = assemblyInfo.Description,
                                Summary                 = "Cake wrapper that allows writing Cake 'scripts' in a Visual Studio project", 
                                ProjectUrl              = new Uri("https://github.com/CakeTin/Cake.Tin"),
                                IconUrl                 = new Uri("https://raw.githubusercontent.com/cake-build/graphics/master/png/cake-medium.png"),
                                LicenseUrl              = new Uri("https://github.com/caketin/Cake.Tin/blob/master/LICENSE"),
                                Copyright               = assemblyInfo.Copyright,
                                ReleaseNotes            = releaseNotes.Notes.ToArray(),
                                Tags                    = new [] {"Cake", "Script", "Build"},
                                RequireLicenseAcceptance= false,        
                                Symbols                 = false,
                                NoPackageAnalysis       = true,
                                Files                   = nuspecFiles,
                                BasePath                = binDir, 
                                OutputDirectory         = nugetRoot
                            };

///////////////////////////////////////////////////////////////////////////////
// Output some information about the current build.
///////////////////////////////////////////////////////////////////////////////
var buildStartMessage = string.Format("Building version {0} of {1} ({2}).", version, assemblyInfo.Product, semVersion);
Information(buildStartMessage);

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
    CreateAssemblyInfo(file, assemblyInfo);
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
    CopyFileToDirectory(buildDir + File("Cake.Tin.exe"), binDir);
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
    // Create Cake.Tin package.
    if (!System.IO.Directory.Exists(nugetRoot))
    {
        CreateDirectory(nugetRoot);
    }
    NuGetPack("./nuspec/Cake.Tin.nuspec", nuGetPackSettings);
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

//////////////////////////////////////////////////////////////////////
// TASK TARGETS
//////////////////////////////////////////////////////////////////////

Task("Package")
    .IsDependentOn("Zip-Files")
    .IsDependentOn("Create-NuGet-Packages");

Task("Publish-NuGet-Packages")
    .IsDependentOn("Create-NuGet-Packages")
    .WithCriteria(() => !isLocalBuild)
    .WithCriteria(() => !isPullRequest) 
    .Does(() =>
{
    var packages  = GetFiles(nugetRoot.ToString() + "/*.nupkg");
    foreach (var package in packages)
    {
        Information(string.Format("Found {0}", package));

        // Push the package.
        string apiKey = EnvironmentVariable("NUGET_API_KEY");
        if (string.IsNullOrEmpty(apiKey))
        {
            throw new Exception("NUGET_API_KEY variable not found");
        }

        NuGetPush(package, new NuGetPushSettings {
                Source = "https://www.nuget.org/api/v2/package",
                ApiKey = apiKey
            }); 
    }
}); 

Task("Default")
    .IsDependentOn("Package");

Task("AppVeyor")
    .IsDependentOn("Update-AppVeyor-Build-Number")
    .IsDependentOn("Upload-AppVeyor-Artifacts")
    .IsDependentOn("Publish-NuGet-Packages");

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
            RunGit("config --global push.default simple");
            if (AppVeyor.IsRunningOnAppVeyor)
            {
                string token = EnvironmentVariable("gittoken");
                if (string.IsNullOrEmpty(token))
                {
                    throw new Exception("gittoken variable not found");
                }
                
                string auth = string.Format("https://{0}:x-oauth-basic@github.com\n", token);
                string credentialsStore = System.Environment.ExpandEnvironmentVariables(@"%USERPROFILE%\.git-credentials");
                //Information("Writing {0} to {1}", auth, credentialsStore);
                System.IO.File.AppendAllText(credentialsStore, auth);
                //Information("{0} now contains:\n{1}", credentialsStore, System.IO.File.ReadAllText(credentialsStore));
            }

            RunGit("add " + fileName);
            RunGit("commit -m\"Update release notes\"");
            RunGit("push");
        }
        else
        {
           Information("Release notes up to date");
        }
    }

    private IEnumerable<string> RunGit(string arguments, bool logOutput = true)
    {
        IEnumerable<string> output;
        var exitCode = StartProcess("git", new ProcessSettings
        {
          Arguments = arguments, 
          Timeout = (int)TimeSpan.FromMinutes(1).TotalMilliseconds,
          RedirectStandardOutput = true
        }, out output);

        output = output.ToList();
        if (logOutput)
        {
            foreach (var line in output)
            {
                Information(line);
            }
        }

        if (exitCode != 0)
        {
            Information("Git returned {0}", exitCode);
            throw new Exception("Git Error");
        }
        
        return output;
    }

    private string GetGitBranch()
    {
        string branch  = null;
        IEnumerable<string> output = RunGit("status", false);
        string line = output.FirstOrDefault(s => s.Trim().StartsWith("On branch"));
        if (line == null)
        {
            Information("Unable to determine Git Branch, number " );
            foreach (var oline in output)
            {
                Information(oline);
            }

            throw new Exception("Unable to determine Git Branch");
        }
        
        return line.Replace("On branch", string.Empty).Trim();
    }