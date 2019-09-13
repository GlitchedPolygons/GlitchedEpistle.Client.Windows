[![API](https://img.shields.io/badge/api-docs-informational.svg)](https://glitchedpolygons.github.io/GlitchedEpistle.Client.Windows)
[![License Shield](https://img.shields.io/badge/license-GPLv3-brightgreen)](https://github.com/GlitchedPolygons/GlitchedEpistle.Client.Windows/blob/master/LICENSE)

# Glitched Epistle
## Windows Client

### How to set up the development environment:

* Make sure you have [Visual Studio](https://visualstudio.microsoft.com/) >2017 installed with the [.NET Framework SDK](https://dotnet.microsoft.com/download/visual-studio-sdks) >=4.7.1
* If you use the SSH URL, make sure you have your git SSH keys set up to authenticate with git correctly!
* Make sure you have git lfs installed and enabled (run `git lfs install` at least once on your system).
* Navigate to a directory where you wish to clone this repository into.
* Open the command line interface in that folder and execute the following command:
```
git clone https://github.com/GlitchedPolygons/GlitchedEpistle.Client.Windows "GlitchedEpistle.Client.Windows"
```

### Where are my messages stored

Your entire Glitched Epistle user data, including settings, convos, metadata and messages are stored inside `C:\Users\{YOUR_USERNAME}\AppData\Local\GlitchedPolygons\GlitchedEpistle\{YOUR_EPISTLE_USER_ID}`. 

The messages are stored (in their encrypted form!) inside [SQLite](https://sqlite.org) database files inside the `Convos/` directory.

### Contributing
Just fork & open a PR and I'll take a look at it ASAP :)
