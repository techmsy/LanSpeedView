■プログラム

  LanSpeedView


■プログラムの機能

  LanSpeedView is a tool to measure LAN network speed by copy speed to/from shared folders.


■使用方法

  LanSpeedView <sharePath> [-s <fileSizeMB>] [-savelog]



  LanSpeedView <sharePath> [-size <fileSizeMB>] [--savelog] [-mode <max>]
                                                                   max, min, ave...default


■記述例
xxx
  doMail.vbs /E:29 /S:c:\test\nyuko.txt /D:c:\test\conv_nyuko.txt /L:Config_doMail_Label_test.vbs
xxx


■オプション
■設定ファイル

Edit LanSpeedView.exe.config


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


■改版履歴

◎Ver.0.04 (2024/8/22) t

	修正

◎Ver.0.01 (2024/*/*) t

	修正

◎Ver.0.02 (2024/*/*) t

	修正

◎Ver.0.01 (2024/*/*) t

	初版
