■プログラム

  LanSpeedView

■プログラムの機能

  LanSpeedView is a tool to measure LAN network speed by copy speed to/from shared folders.

■使用方法

  LanSpeedView <sharePath> [-s <fileSizeMB>] [--savelog]


  以下のオプションは対応未
  [-mode <max>]
          └max, min, ave...default

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
◎Ver.0.06 (2024/12/8) techmsy

  ログの書式を変更した。
  平均速度、最も速い、最も遅いでそれぞれ1行で表現するようにした。
  計測速度を小数第2位切り捨て、小数第1位表示にした。
  経過時間を小数第3位切り上げ、小数第2位表示にした。

◎Ver.0.05 (2024/9/1) techmsy

	修正

◎Ver.0.04 (2024/8/22) techmsy

	修正

◎Ver.0.01 (2024/*/*) techmsy

	修正

◎Ver.0.02 (2024/*/*) techmsy

	修正

◎Ver.0.01 (2024/*/*) techmsy

	初版
