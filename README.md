# ServiceKiller 1.0

`ServiceKiller` is a lightweight .NET 8 Worker Service designed to run natively on Windows. Its primary purpose is to continuously monitor, stop, and disable a specified target Windows service at regular intervals (defaulting to the Windows Update service, `wuauserv`).

---

## Repository Structure

```text
ServiceKiller/
├── ServiceKiller.csproj   # .NET 8 project file with Windows Service hosting package
├── Program.cs             # Entry point configuring the generic host and Windows Service lifetime
└── Worker.cs              # Background service logic executing periodic service management commands
```

---

## Features

- **Native Windows Service Integration:** Built using `Microsoft.Extensions.Hosting.WindowsServices` to run seamlessly as a Windows background service (`ServiceKillerSvc`).
- **Aggressive Enforcement Loop:** Runs an endless execution loop every 5 seconds to ensure the target service stays stopped and disabled.
- **Process Execution (`sc.exe`):** Interacts directly with the Windows Service Control Manager using standard command-line tools.
- **Robust & Self-Cleaning Logging:** Automatically handles log directory creation and records activity, stdout/stderr streams, and errors to `C:\ProgramData\ServiceKiller\service-killer.log`. 
- **Automatic Log Rotation:** Features built-in size monitoring (default limit of 3 MB) that automatically purges older log lines and retains only the most recent entries, preventing unmanaged disk space consumption over long periods.
---

## Prerequisites

- **SDK/Runtime:** [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) (or .NET 8 Runtime for deployment).
- **OS:** Windows Operating System (required due to dependency on Windows Services and `sc.exe`).
- **Privileges:** Administrative privileges are required to install, start, stop, or configure system services.

---

## Configuration

By default, the service targets the Windows Update service (`wuauserv`) and logs to `C:\ProgramData\ServiceKiller\service-killer.log`. 

If you want to target a different service, open `Worker.cs` and modify the private fields:
```csharp
private readonly string _targetService = "YOUR_TARGET_SERVICE_NAME";
private readonly string _logPath = @"C:\ProgramData\ServiceKiller\service-killer.log";

// Log rotation configuration
private const long MaxLogSizeBytes = 3 * 1024 * 1024; // 3 MB limit
private const int MaxLinesToKeepOnTrim = 5000; // retain only the last 5,000 lines after the purge
```

---

## Building and Installing as a Windows Service

1. **Publish the project:**
   ```powershell
   dotnet publish -c Release -r win-x64 --self-contained true
   ```
   or
   ```powershell
   dotnet publish .\ServiceKiller.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -o C:\path\to\folder\output\ServiceKiller
   ```

2. **Install the Windows Service** (run Command Prompt or PowerShell as **Administrator**):
   ```cmd
   sc.exe create ServiceKillerSvc binPath= "C:\path\to\your\publish\ServiceKiller.exe" start= auto
   ```

3. **Start the Service:**
   ```cmd
   sc.exe start ServiceKillerSvc
   ```

4. **Uninstall the Service** (if needed):
   ```cmd
   sc.exe stop ServiceKillerSvc
   sc.exe delete ServiceKillerSvc
   ```

---
