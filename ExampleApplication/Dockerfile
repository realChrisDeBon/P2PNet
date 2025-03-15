FROM mcr.microsoft.com/dotnet/sdk:9.0-windowsservercore-ltsc2019 AS build
WORKDIR /app

# copy the NuGet configuration file from the root directory
COPY ["NuGet.config", "./"]

# copy the csproj and restore
COPY ["ExampleApplication/ExampleApplication.csproj", "ExampleApplication/"]
RUN dotnet restore "./ExampleApplication/ExampleApplication.csproj"

# copy everything else and build
COPY . .
WORKDIR "/app/ExampleApplication"

# download WinPcap
RUN powershell -Command Invoke-WebRequest -Uri https://github.com/realChrisDeBon/P2PNet/releases/download/dependency/winpcap-nmap-4.13.exe -OutFile winpcap-download.exe


# install WinPcap
RUN powershell -Command Start-Process -FilePath "./winpcap-download.exe" -Wait -ArgumentList "/S"

# move the installed DLLs from System32 to a safe location in the build stage
RUN powershell -Command "Copy-Item 'C:\Windows\System32\wpcap.dll' 'C:\app\wpcap.dll'; Copy-Item 'C:\Windows\System32\Packet.dll' 'C:\app\Packet.dll'"

# start publish
RUN dotnet publish "./ExampleApplication.csproj" -c Release -o /app/publish

# use .NET runtime
FROM mcr.microsoft.com/dotnet/runtime:9.0-windowsservercore-ltsc2019
WORKDIR /app

# copy built application
COPY --from=build /app/publish .

# re-inject the DLLs into the System32 folder of the runtime image
COPY --from=build C:/app/wpcap.dll C:/Windows/System32/wpcap.dll
COPY --from=build C:/app/Packet.dll C:/Windows/System32/Packet.dll

# expose the ports
EXPOSE 8001 8002 8003

# entrypoint
ENTRYPOINT ["dotnet", "ExampleApplication.dll"]
