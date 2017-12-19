#tool "nuget:?package=GitVersion.CommandLine"

//Folder Variables
var RepoRootFolder = MakeAbsolute(Directory(".")); 
var InstallSourceFolder = RepoRootFolder + "/Installer";
var BuildFolder = RepoRootFolder + "/Build";
var ReleaseFolder = BuildFolder + "/Release";
var SolutionFile = RepoRootFolder + "/CommandLineParser.sln";
var ToolsFolder = RepoRootFolder + "/Tools";

var nugetAPIKey = EnvironmentVariable("NUGETAPIKEY");

var target = Argument("target", "Default");

GitVersion version;

try{
    version = GitVersion(new GitVersionSettings{UpdateAssemblyInfo = true}); //This updates all AssemblyInfo files automatically.
}
catch
{
    //Unable to get version.
}

Task("Default")
    .IsDependentOn("Restore")
    .IsDependentOn("Build");

Task("Restore")
    .IsDependentOn("CommandArgs.Restore");

Task("Clean");

Task("Build")
    .IsDependentOn("CommandArgs.Build");

Task("Test");

Task("Deploy")
    .IsDependentOn("CommandArgs.Deploy");

Task("Version")
    .Does(() => {
        Information("Assembly Version: " + version.AssemblySemVer);
        Information("SemVer: " + version.SemVer);
        Information("Branch: " + version.BranchName);
        Information("Commit Date: " + version.CommitDate);
        Information("Build Metadata: " + version.BuildMetaData);
        Information("PreReleaseLabel: " + version.PreReleaseLabel);
        Information("FullBuildMetaData: " + version.FullBuildMetaData);
    });


/*****************************************************************************************************
VMLab
*****************************************************************************************************/
Task("CommandArgs.Clean")
    .IsDependentOn("CommandArgs.Clean.Main");

Task("CommandArgs.Restore")
    .IsDependentOn("CommandArgs.DotNetRestore");    

Task("CommandArgs.Build")
    .IsDependentOn("CommandArgs.Build.Compile");

Task("CommandArgs.Test");

Task("CommandArgs.Deploy")
    .IsDependentOn("CommandArgs.Deploy.NuGet");

Task("CommandArgs.DotNetRestore")
    .Does(() => {
        var proc = StartProcess("dotnet", new ProcessSettings { Arguments = "restore", WorkingDirectory = RepoRootFolder + "/CommandLineParser"  });

        if(proc != 0)
            throw new Exception("dotnet didn't return 0 it returned " + proc);
    });

Task("CommandArgs.UpdateVersion")
    .Does(() => {
        var file = RepoRootFolder + "/CommandLineParser/CommandLineParser.csproj";
        XmlPoke(file, "/Project/PropertyGroup/Version", version.SemVer);
        XmlPoke(file, "/Project/PropertyGroup/AssemblyVersion", version.AssemblySemVer);
        XmlPoke(file, "/Project/PropertyGroup/FileVersion", version.AssemblySemVer);
        XmlPoke(file, "/Project/PropertyGroup/PackageReleaseNotes", version.FullBuildMetaData);
    });

Task("CommandArgs.Clean.Main")
    .Does(() => 
    {
        CleanDirectory(RepoRootFolder + "/CommandLineParser/Bin");
    });

Task("CommandArgs.Build.Compile")
    .IsDependentOn("CommandArgs.UpdateVersion")
    .IsDependentOn("CommandArgs.Clean.Main")
    .Does(() => {
        MSBuild(SolutionFile, config =>
            config.SetVerbosity(Verbosity.Minimal)
            .SetConfiguration("Release")
            .UseToolVersion(MSBuildToolVersion.VS2017)
            .SetMSBuildPlatform(MSBuildPlatform.Automatic)
            .SetPlatformTarget(PlatformTarget.MSIL));
        });

Task("CommandArgs.Deploy.NuGet")
    .Does(() => {
        NuGetPush(RepoRootFolder + "/CommandLineParser/Bin/Release/WilTaylor.CommandLineParser." + version.SemVer + ".nupkg",
        new NuGetPushSettings{
            Source = "https://api.nuget.org/v3/index.json",
            ApiKey = nugetAPIKey
        });
    });


/*****************************************************************************************************
End of script
*****************************************************************************************************/
RunTarget(target);