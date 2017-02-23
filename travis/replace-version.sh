sed "s|AssemblyVersion(\"1.0.*\")|AssemblyVersion(\"$DOCKER_TAG\")|" src/Letmein.Web/Properties/AssemblyInfo.cs > src/Letmein.Web/Properties/AssemblyInfo.temp.cs
mv src/Letmein.Web/Properties/AssemblyInfo.temp.cs src/Letmein.Web/Properties/AssemblyInfo.cs
cat src/Letmein.Web/Properties/AssemblyInfo.cs