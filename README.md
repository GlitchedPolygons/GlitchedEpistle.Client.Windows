[![API](https://img.shields.io/badge/api-docs-informational.svg)](https://glitchedpolygons.github.io/GlitchedEpistle.Client.Windows)

# Glitched Epistle
## Windows Client

### How to set up the development environment:

* Make sure you have [Visual Studio](https://visualstudio.microsoft.com/) >2017 installed with the [.NET Framework SDK](https://dotnet.microsoft.com/download/visual-studio-sdks) >=4.7.1
* If you use the SSH URL, make sure you have your git SSH keys set up to authenticate with git correctly!
* Make sure you have git lfs installed and enabled (run `git lfs install` at least once on your system).
* Navigate to a directory that you consider to be the root folder for all of the Glitched Epistle projects ([Client](https://github.com/GlitchedPolygons/GlitchedEpistle.Client) + [Client.Windows](https://github.com/GlitchedPolygons/GlitchedEpistle.Client.Windows) need to reside in the same folder!).
* Open the command line interface in that folder and execute the following commands:
```
git clone https://github.com/GlitchedPolygons/GlitchedEpistle.Client "GlitchedEpistle.Client"
git clone https://github.com/GlitchedPolygons/GlitchedEpistle.Client.Windows "GlitchedEpistle.Client.Windows"
```

### Preparing for a release
#### Updating the Windows Client

When done creating a new update for the windows client, push everything to github (obviously) and then create a new commit (which you could comfortably name "Release x.y.z" for consistency's sake) where you:

When updating the Epistle Windows client, version numbers must be increased in the following locations:
```
    glitched-epistle-innosetup-script.iss
    [Assembly Version + File Version] (under project properties)
```
Now you can build in Release mode and upload the installer file to github (create a new release). Remember to update the checksum in the release description!


##### If the inno setup compiler fails:
Inside inno setup studio, under files:  remove all files and directories and re-add the build output (bin/Release/) by drag 'n' drop back into the inno setup studio field.
Save, and then run the inno compiler again. Don't add the .pdb files, they are unnecessary in a release build. Exceptions are almost always logged to the ILogger implementation anyway..


### Contributing
Just fork & open a PR and I'll take a look at it ASAP :)
