# Giraffe as a service

1. Install the Giraffe template:
    ```powershell
    dotnet new -i "giraffe-template::*"
    ```
1. Create the project from the template:
    ```powershell
    mkdir c:\git\GiraffeAsAService
    cd c:\git\GiraffeAsAService
    dotnet new giraffe
    ```
1. Open `GiraffeAsAService.fsproj` file in an editor, and change:
    1. `TargetFramework` from `netcoreapp2.0` to `net461`
    1. `FrameworkVersion` from `2.0.0` to `4.6.1`
1. Add the Nuget package `Microsoft.AspNetCore.Hosting.WindowsServices` to the project
1. Edit `Program.fs`, first to add:
    ```fsharp
    open Microsoft.AspNetCore.Hosting.WindowsServices
    ```
1. Then change the contents of `main` to:
    ```fsharp
    let pathToAssembly = System.Reflection.Assembly.GetExecutingAssembly().Location
    let contentRoot = Path.GetDirectoryName(pathToAssembly)
    let webRoot     = Path.Combine(contentRoot, "WebRoot")
    
    let host =
        WebHostBuilder()
            .UseKestrel()
            .UseContentRoot(contentRoot)
            .UseIISIntegration()
            .UseWebRoot(webRoot)
            .Configure(Action<IApplicationBuilder> configureApp)
            .ConfigureServices(configureServices)
            .ConfigureLogging(configureLogging)
            .Build()

    if Environment.UserInteractive then
        host.Run()
    else
        host.RunAsService()
    
    0
    ```
1. Publish the app:
    ```powershell
    dotnet publish -c Release -r win10-x64
    ```
1. Copy the publish directory to an appropriate place from which to run the Windows service:
    ```powershell
    mkdir c:\Services\GiraffeAsAService
    cp -r bin\Release\net461\win10-x64\publish\* c:\Services\GiraffeAsAService
    ```
1. Create the Windows service (this needs to be done as Administrator):
    ```powershell
    New-Service `
        -Name GiraffeAsAService `
        -BinaryPathName c:\Services\GiraffeAsAService\GiraffeAsAService.exe `
        -DisplayName "Giraffe as a Windows Service" `
        -Description "Example of running a Giraffe application as a Windows Service"
1. Start the service (also needs to be done as Administrator):
    ```powershell
    Start-Service GiraffeAsAService
    ```
1. Test the service:
    ```powershell
    start http://localhost:5000/ # This should open in your default browser
    ```

## References

1. Dustin Moris Gorski's [Giraffe](https://github.com/dustinmoris/Giraffe) - 
A native functional ASP.NET Core web framework for F# developers
1. [How to run and ASP.Net Core application as a Windows service](https://docs.microsoft.com/en-us/aspnet/core/hosting/windows-service)