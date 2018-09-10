sed "s|<Version>99.88.77</Version>|<Version>$DOCKER_TAG</Version>|" src/Letmein.Web/Letmein.Web.csproj > src/Letmein.Web/Letmein.Web.csproj.temp
mv src/Letmein.Web/Letmein.Web.csproj.temp src/Letmein.Web/Letmein.Web.csproj
cat src/Letmein.Web/Letmein.Web.csproj