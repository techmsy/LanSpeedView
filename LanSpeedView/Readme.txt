���v���O����

  LanSpeedView


���v���O�����̋@�\

  LanSpeedView is a tool to measure LAN network speed by copy speed to/from shared folders.


���g�p���@

  LanSpeedView <sharePath> [-s <fileSizeMB>] [-savelog]



  LanSpeedView <sharePath> [-size <fileSizeMB>] [--savelog] [-mode <max>]
                                                                   max, min, ave...default


���L�q��
xxx
  doMail.vbs /E:29 /S:c:\test\nyuko.txt /D:c:\test\conv_nyuko.txt /L:Config_doMail_Label_test.vbs
xxx


���I�v�V����
���ݒ�t�@�C��

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


�����ŗ���

��Ver.0.04 (2024/8/22) t

	�C��

��Ver.0.01 (2024/*/*) t

	�C��

��Ver.0.02 (2024/*/*) t

	�C��

��Ver.0.01 (2024/*/*) t

	����
