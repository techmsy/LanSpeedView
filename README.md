# LanSpeedView
LanSpeedView is a tool to measure LAN network speed by copy speed to/from shared folders.

## Usage
```
> LanSpeedView <sharePath> [-s <fileSizeMB>] [--savelog]
```

## Option
Edit LanSpeedView.exe.config
```xml
<?xml version="1.0" encoding="utf-8" ?>
<configuration>
  <appSettings>
    <add key="logFilePath" value="log.txt" />
    <add key="defaultFileSizeMB" value="10" />
    <add key="loopCount" value="5" />
  </appSettings>
</configuration>
```
logFilePath ... Default LogFile Path

defaultFileSizeMB ... Default CopiedFile Size

loopCount ... Number of Loop Times
