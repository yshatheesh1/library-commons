## BBCoders.Commons.Tools
Automation for ef core queries at design time

# install tool locally
`dotnet tool install --global --add-source ./nupkg BBCoders.Commons.QueryGeneratorTool`

# uninstall tool locally
`dotnet tool uninstall BBCoders.Commons.QueryGeneratorTool —global`

# publish tool to nuget
`dotnet nuget push nupkg/**/*.nupkg --api-key {apiKey} --source https://api.nuget.org/v3/index.json  --skip-duplicate`