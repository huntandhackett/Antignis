# Antignis

       ,_     _,            _____          __  .__              .__
         '._.'             /  _  \   _____/  |_|__| ____   ____ |__| ______
    '-,   (_)   ,-'       /  /_\  \ /    \   __\  |/ ___\ /    \|  |/  ___/
      '._ .:. _.'        /    |    \   |  \  | |  / /_/  >   |  \  |\___ \
       _ '|Y|' _         \____|__  /___|  /__| |__\___  /|___|  /__/____  >
     ,` `>\ /<` `,               \/     \/       /_____/      \/        \/
    ` ,-`  I  `-, `
      |   /=\   |        
    ,-'   |=|   '-,      
          )-(
          \_/

Link to blogpost: https://www.huntandhackett.com/blog/introducing-antignis-a-data-driven-tool-to-configure-windows-hostbased-firewall

## About this repository

Antignis comes with two tools: `Antignis.Server` and `Antignis.Client`.  
These tools have their own project and dedicated READMEs that goes into more detail on how to use it, how it was created and more. 

## How to download
Go to Releases to download the packages. Or clone the repository and build the solution yourself.

## Requirements
Antignis was created using `.NET Framework 4.8`. You will need to install this before you can use Antignis.

For Antignis.Server, there are additional requirements:
 - The tool must be run on a domain joined computer with a domain user account
 - The user running the tool must have enough privileges to:
   -  create groups and populate the groups
   -  create GPOs

For Antignis.Client, the user or computer account must be able to write to the output directory. If no output directory is specified, this defaults to the startup directory of the tool.

## Use other data sources to enrich the Antignis database
If you do not want to use Antignis.Client but rather use other data sources to enrich the database, you can do so by exporting the existing data into a JSON file. There's a JSON schema available in `https://github.com/huntandhackett/Antignis/tree/main/Misc` to validate the output. If the schema successfully validates the JSON file, the JSON files can be imported into the database as well. 

## License
Apache 2.0