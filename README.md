# lazer-db-map-converter
A simple command line tool, that converts [osu!lazer](https://github.com/ppy/osu) beatmaps back into osz files, or simple folders.

## How to use

Copy and paste the `client.db` file into the same directory as the executable.

Run the executable by opening in the shell and type in either

* `LazerDBMapConverter.exe 'PathToOsuLazer (leave empty for standard path)' -osz` (for osz files)
* `LazerDBMapConverter.exe 'PathToOsuLazer (leave empty for standard path)' -dir` (for a directory structure)

Afterwards you will find a `Maps` folder in the same directory as the executable.