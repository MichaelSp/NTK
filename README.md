# NTK - Not Today Kids
[![Github All Releases](https://img.shields.io/github/downloads/MichaelSp/NTK/total.svg)](https://github.com/MichaelSp/NTK/releases)

Is a windows application that limits kids screen time using computer. prevents them from browsing prohibited websites.
## Getting Started
Download the latest release (a single file excutable)
### Prerequisites
* .net 3.1 get it [here](https://dotnet.microsoft.com/download/dotnet/3.1)
### Installation
* run on the start up
1. download the latest release. just be sure the executable file can be access by the other users without asking administrator password. eg. C:\ | D:\ and not in the Program Files
2. Login to the user account
3. locate the downloaded executable then copy, 
4. press win + r then type shell:appsFolder it will open the start up folder.
5. right click the start up folder and click paste shortcut.
### Note
You can edit the time limit(in seconds) and the message by editing the ntk-config.json
```JSON
   {
       "Limit": 10800,
       "Message": "You have used your alloted time for today, Come back tomorrow."
   }
```

