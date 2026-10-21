# tools/register_windows_autostart_task.ps1
# Registers a persistent Windows Scheduled Task that boots the ExamiFy 10-Lens Watcher automatically on restart.

$TaskName = "ExamiFy-Agent-Watcher"
$PythonPath = "C:\Python315\python.exe"
$WorkingDir = "D:\Projects\ExamiFy-Core"

$Action = New-ScheduledTaskAction -Execute $PythonPath -Argument "-m debate_engine.auto_evolve --watch" -WorkingDirectory $WorkingDir
$Trigger = New-ScheduledTaskTrigger -AtLogOn
$Settings = New-ScheduledTaskSettingsSet -AllowStartIfOnBatteries -DontStopIfGoingOnBatteries -ExecutionTimeLimit (New-TimeSpan -Days 365) -RestartCount 3 -RestartInterval (New-TimeSpan -Minutes 1)

Register-ScheduledTask -TaskName $TaskName -Action $Action -Trigger $Trigger -Settings $Settings -Description "ExamiFy Autonomous 10-Lens Proactive Evolution Watcher" -Force
Write-Host "✅ Persistent Task '$TaskName' registered successfully. The watcher will now start automatically whenever your computer restarts or logs in."
