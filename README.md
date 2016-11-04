# UpdateLocalTimeNTP
Update a Windows machine to a list of NTP servers, removing outliers.  Console App, can be Automated.
```
===================================================================================================
UpdateLocalTimeNTP.exe Usage: *NOTE: PROGRAM MUST BE RUN AS ADMIN!*
  -nochange  Test operation but make no clock changes.
  -force (cannot be used with -nochange) Force an update even if drastically off or tiny.
  -log  Log operations to local folder for unattended troubleshooting.
    Logs do not clean themselves up! Format=YEAR-MM-DD Hour.log (one log file per hour).
    Error logs are always written even if -log is not specified (ERROR-...log
  -nopause  Exit upon completion (allow console window to close).
  -resetlist  Re-add all servers, even ones that have failed in the past.
    If servers.txt is missing, program will recreate it from internal list.
    Edit active.txt or servers.txt if you want to use your own servers. (active.txt is rebuilt
    from servers.txt if present or internal list).
  -count:X (no spaces) Number of servers to use where X is between 3 and 30 (inclusive, default 15)
===================================================================================================
```