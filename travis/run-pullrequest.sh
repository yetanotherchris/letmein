export DOCKER_TAG="1.0.$TRAVIS_BUILD_NUMBER"
export POSTGRES_CONNECTIONSTRING="host=localhost;database=letmein;username=postgres;password="
echo "Using $DOCKER_TAG for the Docker tag"
cd src/Letmein.Web/wwwroot
./node_modules/.bin/gulp --docker_tag $DOCKER_TAG
cd ../../../
chmod 777 ./travis/update-assembly-version.sh
chmod 777 ./travis/clean-wwwroot.sh
./travis/update-assembly-version.sh
./travis/clean-wwwroot.sh
dotnet restore
cd src/Letmein.Tests/
dotnet test