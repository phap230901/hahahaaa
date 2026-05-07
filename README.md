# Android Multi-Device Automation Desktop (WinForms, .NET 8)

Production-style starter architecture for a Windows desktop automation tool using ADB, SQLite, OpenCVSharp, and OCR.

## Project Architecture

- **UI Layer (`UI`)**
  - WinForms MainForm, DataGridView device panel, tabbed operation modules.
- **Application Layer (`Application`)**
  - Device and automation orchestration (`DeviceManager`, `AutomationEngine`).
- **Core Layer (`Core`)**
  - Contracts (`Interfaces`) and pure models (`DeviceInfo`, workflow/ocr models).
- **Infrastructure Layer (`Infrastructure`)**
  - ADB command execution, SQLite persistence, OpenCV image matching, OCR service, logging.

## Folder Structure

```
src/
  Automation.Desktop/
    Core/
      Interfaces/
      Models/
    Application/
      Managers/
      Services/
    Infrastructure/
      Adb/
      Database/
      Imaging/
      OCR/
      Logging/
    UI/
      Forms/
      Controls/
    Program.cs
    Automation.Desktop.csproj
```

## Implemented Foundation

1. **Device Manager**
   - Async refresh of ADB devices
   - In-memory cache + DB upsert
   - UI event binding
2. **ADB Wrapper**
   - `tap`, `swipe`, `input text`, `screenshot capture`, `open app`, `clear app data`, `install apk`
3. **Main UI**
   - Tabs: Actions, Auto, Text Search, Restore & Reset, Random, Settings
   - Action buttons added in Actions tab
4. **Automation Engine**
   - Step execution, retry logic, delay handling, logging hooks
5. **Database**
   - SQLite tables: `devices`, `logs`, `workflows`, `profiles`

## NuGet Packages

- `Microsoft.Extensions.DependencyInjection`
- `Microsoft.Extensions.Logging`
- `Microsoft.Extensions.Logging.Console`
- `Microsoft.Data.Sqlite`
- `OpenCvSharp4`
- `OpenCvSharp4.runtime.win`
- `Tesseract`

## Build Instructions

1. Install **.NET 8 SDK** on Windows.
2. Ensure `adb` is in PATH.
3. Restore/build:

```bash
cd src/Automation.Desktop
dotnet restore
dotnet build -c Release
```

4. Provide OCR tessdata files at runtime (`./tessdata`).

## Next Production Steps

- Add stronger command escaping strategy for ADB text input.
- Add cancellation-aware background pipelines per device.
- Persist full workflow definitions and runtime logs.
- Add PaddleOCR adapter option under `IOcrService`.
- Add per-device worker queue + max-concurrency control.
