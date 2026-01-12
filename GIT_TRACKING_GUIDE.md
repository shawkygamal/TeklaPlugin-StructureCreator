# Git Tracking Guide for TeklaPlugin Project

## 📁 Files to TRACK (Add to Git)

### ✅ Essential Code Files
- `ConcreteBeamCreator.cs` - Main structure creator class
- `Form1.cs` - Main Windows Form UI
- `Form1.Designer.cs` - Form designer code (auto-generated)
- `Program.cs` - Application entry point
- `StructureService.cs` - Core service for creating structural elements
- `StructureParameters.cs` - Parameter classes for all components
- `simpleBeamCut.cs` - Additional beam cutting functionality

### ✅ Project Configuration
- `TeklaPlugin.csproj` - Project file with references and settings
- `App.config` - Application configuration

### ✅ Resources
- `Form1.resx` - Form resources
- `Properties/Resources.resx` - Application resources
- `Properties/Resources.Designer.cs` - Auto-generated resource designer
- `Properties/Settings.settings` - Application settings
- `Properties/Settings.Designer.cs` - Auto-generated settings designer
- `Properties/AssemblyInfo.cs` - Assembly information

### ✅ Documentation & Scripts
- `README_Plugin_Installation.md` - Installation instructions
- `Cap_Geometry_Explanation.md` - Cap geometry documentation
- `GIT_TRACKING_GUIDE.md` - This file
- `install_plugin.bat` - Installation batch script
- `.gitignore` - Git ignore rules

## ❌ Files to IGNORE (Excluded by .gitignore)

### 🚫 Build Artifacts
- `bin/` - All build outputs (*.exe, *.dll, *.pdb, *.xml)
- `obj/` - Intermediate build files and caches

### 🚫 User-Specific Files
- `*.user` - User-specific project settings
- `*.suo` - Solution user options
- `.vs/` - Visual Studio user settings

### 🚫 Temporary Files
- `*.tmp`, `*.temp`, `*.log` - Temporary files
- `*.bak`, `*.backup`, `*.orig` - Backup files
- `Thumbs.db`, `Desktop.ini` - Windows system files

### 🚫 IDE Files
- `.vscode/`, `.idea/` - IDE-specific settings
- `*.swp`, `*.swo` - Vim swap files

## 🔧 Git Commands

### Initialize Repository
```bash
git init
git add .
git commit -m "Initial commit: TeklaPlugin structure creator"
```

### Add Specific Files Only
```bash
git add *.cs
git add *.csproj
git add *.config
git add *.resx
git add *.md
git add *.bat
git add .gitignore
```

### Check What's Being Tracked
```bash
git status
git ls-files
```

## 📊 Repository Size Impact

### ✅ Tracked Files (Small)
- Source code: ~25KB
- Project files: ~5KB
- Resources: ~10KB
- Documentation: ~15KB
- **Total: ~55KB**

### ❌ Ignored Files (Large)
- Build outputs: ~50MB (bin/ + obj/)
- Tekla references: ~100MB
- **Total: ~150MB+ excluded**

## 🎯 Best Practices

1. **Always commit before building** - Clean working directory
2. **Never commit build artifacts** - They can be regenerated
3. **Use .gitignore from start** - Prevents accidental commits
4. **Document your code** - README files are tracked
5. **Version control scripts** - Installation scripts are useful

## 🔍 Verification

To verify your repository only contains essential files:

```bash
# Should show only the files listed above
git ls-files | sort

# Should show build artifacts as untracked
git status --ignored
```