# Custom libraries

# build
`dotnet build -c release`

# pack
`dotnet pack --output nupkg`

# publish
`dotnet nuget push nupkg/**/*.nupkg --api-key key --source https://api.nuget.org/v3/index.json  --skip-duplicate`