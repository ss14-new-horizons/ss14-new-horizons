Shell (.sh) scripts are located here.
If you are running a Microsoft operating system (Windows, MS-DOS), use Batch scripts instead.

buildAllDebug
    Builds all projects with debug configuration
buildAllRelease
    Builds all projects with release configuration
buildAllTools
    Builds all projects with tools configuration

The debug vs release build is simply what people develop in vs the actual server.
The release build contains various optimizations, while the debug build contains debugging tools.
If you're mapping, use the tools or release build as it will run smoother with less crashes.

runClientNoBuild
    Runs the client without building
runServerNoBuild
    Runs the server without building

runTests
    Runs the unit tests, makes sure various C# systems work as intended
runTestsIntegration
    Runs the integration tests, makes sure various C# systems work as intended
runTestsYAML
    Runs the YAML linter and finds issues with the YAML files that you probably wouldn't otherwise

buildRunPackagingRelease
    Builds and then runs packaging project with release configuration for all platforms (OS and architecture left unspecified)

Order of readying your test environment for testing and development:
1. Run buildAllDebug or buildAllTools or buildAllRelease (once, unless you made a C# change or if you pulled from master).
2. Run runServerNoBuild to launch the server.
3. Run runClientNoBuild to launch the client.
