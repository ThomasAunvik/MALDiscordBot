#tool "nuget:?package=xunit.runner.console"

var target = Argument("target", "Default");

string configuration;
var appVeyorBranch = EnvironmentVariable("https://ci.appveyor.com/project/ThomasAunvik/animelistbot");

switch (appVeyorBranch)
{
    case "master":
        configuration = "Release";
        break;
    case "development":
        configuration = "QA";
        break;
    default:
        configuration = "Release";
        break;
}

var artifactsDir = Directory("./artifacts");
var solution = "./TheSolution.sln";

Task("Clean")
    .Does(() =>
    {
        CleanDirectory(artifactsDir);
    });

Task("Restore-NuGet-Packages")
    .IsDependentOn("Clean")
    .Does(() =>
    {
        NuGetRestore(solution);
    });

Task("Build")
    .IsDependentOn("Restore-NuGet-Packages")
    .Does(() =>
    {
        MSBuild(solution, settings =>
            settings.SetConfiguration(configuration)
                .WithProperty("TreatWarningsAsErrors", "True")
                .SetVerbosity(Verbosity.Minimal)
                .AddFileLogger());
    });

Task("Run-Tests")
    .IsDependentOn("Build")
    .Does(() =>
    {
        XUnit2("./tests/**/bin/" + configuration + "/*.Tests.dll", new XUnit2Settings
        {
            // If needed:
            // Parallelism = ParallelismOption.None
            // or similar.
        });
    });

Task("Package")
    .IsDependentOn("Run-Tests")
    .Does(() =>
    {
        MSBuild("src/Api/Api.csproj", settings =>
            settings.SetConfiguration(configuration)
                .WithProperty("TreatWarningsAsErrors", "True")
                .SetVerbosity(Verbosity.Minimal)
                .WithTarget("Package")
                .WithProperty("PackageLocation", Directory("../..") + artifactsDir));

        NuGetPack("./src/Client/Client.csproj", new NuGetPackSettings
        {
            OutputDirectory = artifactsDir,
            Properties = new Dictionary<string, string>
            {
                { "Configuration", configuration }
            }
        });
    });

Task("Default")
    .IsDependentOn("Package");

RunTarget(target);