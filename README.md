# LanSpeedView
LanSpeedView is a tool to measure LAN network speed by copy speed to/from shared folders.

## Usage
```
> LanSpeedView <sharePath> [-S <fileSizeMB>]
```

## Option
Edit LanSpeedView.exe.config

```json
<?xml version="1.0" encoding="utf-8" ?>
<configuration>
  <appSettings>
    <add key="LogFilePath" value="log.txt" />
    <add key="defaultFileSizeMB" value="10" />
    <add key="loopCount" value="5" />
  </appSettings>
</configuration>
```
LogFilePath as Default

defaultFileSizeMB as Default

loopCount : Specify the number of loop times
