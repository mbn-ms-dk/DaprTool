dotnet pack -c Debug -o ./nupkg
dotnet tool update --global --add-source ./nupkg M.Dapr.Tool
