dotnet pack -c Debug -o ./nupkg
dotnet tool update --global --add-source ./nupkg demos
dotnet tool update --add-source ./nupkg demos
