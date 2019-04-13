# Glitched Epistle
## Windows Client

### How to set up the development environment:

* Make sure you have [Visual Studio](https://visualstudio.microsoft.com/) >2017 installed with the [.NET Framework SDK](https://dotnet.microsoft.com/download/visual-studio-sdks) >=4.7.1
* If you use the SSH URL, make sure you have your git SSH keys set up to authenticate with git correctly!
* Make sure you have git lfs installed and enabled (run `git lfs install` at least once on your system).
* Navigate to a directory that you consider to be the root folder for all of the Glitched Epistle projects (Client + Client.Windows need to reside in the same folder!).
* Open the command line interface in that folder and execute the following commands:
```
git clone https://github.com/GlitchedPolygons/GlitchedEpistle.Client "GlitchedEpistle.Client"
git clone https://github.com/GlitchedPolygons/GlitchedEpistle.Client.Windows "GlitchedEpistle.Client.Windows"
```

#### Installer project (.MSI)
If you want to build the .msi installer yourself, make sure you have the [WiX toolset](http://wixtoolset.org/releases/) installed (>3.11).

Then, if after opening the solution in VS you can't build the Setup project due to bad paths, set the `WixUIExtension.dll` project reference to the correct path on your system. Usually, that path defaults to `C:\Program Files (x86)\WiX Toolset v3.11\bin\WixUIExtension.dll`.

### Contributing
Just fork & open a PR and I'll take a look at it ASAP :)
