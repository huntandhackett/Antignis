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


# Antignis client
Antignis client will collect information of the system that it is run on. It collects the following information:  
  
- Hostname
- IP Address and network mask
- Roles configured
- Software installed
- Firewall configuration
- Firewall rules
- Ports listened on
- Established TCP Connections

Once collected, the information will be saved as a JSON file in the startup directory of the tool. To specify a different location, use the `--savelocation` parameter followed with the location where the JSON file should be saved.

After the file has been saved, the ACL of the parent directory will be copied to the ACL of the newly created file. This is because if a remote directory has been created with write-only permissions, only the owner of the file will be able to access the file. 
