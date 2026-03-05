# Creating the v1.0.0 GitHub Release

## Option A: Automatic (recommended)

1. **Commit and push** the release workflow and notes to your **PortScanner repo** on GitHub  
   (If your repo is only the `portscanner` folder, run from that clone:  
   `git add . && git commit -m "Add release workflow and notes" && git push`)

2. **Create and push the tag** from the PortScanner repo root:
   ```bash
   git tag v1.0.0
   git push origin v1.0.0
   ```

3. The **Release** workflow runs on GitHub Actions:
   - Builds the self-contained Windows x64 single-file executable
   - Creates the release **PortScanner v1.0.0 — Initial Release**
   - Attaches **PortScanner.exe** as a downloadable asset
   - Uses the release notes with all 4 feature groups and checkmarks

No local .NET SDK needed; the build runs in the cloud.

---

## Option B: Build locally and create release manually

1. **Build the executable** (requires [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)):
   ```powershell
   .\build-release.ps1
   ```
   Output: `PortScanner\publish\win-x64\PortScanner.exe`

2. **Tag** (from repo root):
   ```bash
   git tag v1.0.0
   git push origin v1.0.0
   ```

3. On GitHub: **Releases → Draft a new release**:
   - Tag: `v1.0.0`
   - Title: **PortScanner v1.0.0 — Initial Release**
   - Description: copy from `RELEASE_NOTES_v1.0.0.md`
   - Attach `PortScanner.exe` from `PortScanner\publish\win-x64\`
   - Publish release
