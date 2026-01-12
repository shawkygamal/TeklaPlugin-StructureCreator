# Renaming Your Git Repository

## Current Situation
Your repository is currently named "teklaplugin" and you want to change it to avoid conflicts with another repository.

## Option 1: Rename Local Repository Only

### Change the local folder name:
```bash
# Move to parent directory
cd "G:\Tekla Api Projects\TeklaPlugin"

# Rename the folder
mv TeklaPlugin TeklaPlugin-StructureCreator

# Update Git remote (if you add one later)
cd TeklaPlugin-StructureCreator
git remote set-url origin https://github.com/YOUR_USERNAME/TeklaPlugin-StructureCreator.git
```

## Option 2: Rename Remote Repository

### If you haven't pushed yet:
```bash
# Add remote with new name
git remote add origin https://github.com/YOUR_USERNAME/TeklaPlugin-StructureCreator.git

# Push to new repository
git push -u origin main
```

### If you have a remote already configured:
```bash
# Check current remote
git remote -v

# Remove old remote
git remote remove origin

# Add new remote with different name
git remote add origin https://github.com/YOUR_USERNAME/TeklaPlugin-StructureCreator.git

# Push to new repository
git push -u origin main
```

## Option 3: Rename on GitHub/GitLab (if repo exists remotely)

1. Go to your repository settings on GitHub/GitLab
2. Change the repository name in settings
3. Update your local remote URL:
   ```bash
   git remote set-url origin https://github.com/YOUR_USERNAME/NEW_REPO_NAME.git
   ```

## Suggested New Names

Choose one of these to avoid conflicts:

- `TeklaPlugin-StructureCreator`
- `Tekla-Structure-Generator`
- `Tekla-Foundation-Creator`
- `TeklaPlugin-Foundation`
- `TeklaPlugin-Core`

## Complete Setup Example

```bash
# Make initial commit
git add .
git commit -m "Initial commit: Tekla structure creator plugin"

# Add remote with new name
git remote add origin https://github.com/YOUR_USERNAME/TeklaPlugin-StructureCreator.git

# Push to new repository
git push -u origin main
```

## Verification

After renaming, verify everything works:

```bash
git remote -v
git status
git log --oneline
```