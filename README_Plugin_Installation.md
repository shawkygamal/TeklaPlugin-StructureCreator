# Tekla Structure Creator Plugin Installation

## Plugin Overview
This plugin allows you to create complex foundation structures in Tekla Structures including:
- Foundation beams with skew cuts
- Mat foundations
- Piles in grid patterns
- Lamelar or circular elevation elements
- Cap beams

## Installation Steps

### Method 1: Using the Batch File (Recommended)
1. Run `install_plugin.bat` as Administrator
2. The plugin will be automatically installed to the correct Tekla directory

### Method 2: Manual Installation
1. Open Command Prompt or PowerShell as Administrator
2. Create the plugin directory:
   ```
   mkdir "C:\Program Files\Tekla Structures\2024.0\bin\plugins\Tekla\Model\StructureCreator"
   ```
3. Copy the plugin DLL:
   ```
   copy "C:\Temp\TeklaPlugin.dll" "C:\Program Files\Tekla Structures\2024.0\bin\plugins\Tekla\Model\StructureCreator\"
   ```

## How to Use the Plugin

1. Open Tekla Structures
2. Go to **Applications** menu
3. Look for **ConcreteBeamCreator** or **StructureCreator** plugin
4. Click to open the plugin dialog
5. Configure your structure parameters using the tabbed interface
6. Click **"Create Structure"** to generate the elements

## Plugin Features

- **Global Parameters**: Set position, rotation, and skew angle
- **Foundation**: Configure width, length, and height
- **Mat**: Add cantilever and thickness
- **Piles**: Define grid layout with spacing
- **Elevation**: Choose between lamelar or circular columns
- **Cap**: Configure the top beam

## Requirements
- Tekla Structures 2024.0
- .NET Framework 4.8
- Windows operating system

## Troubleshooting

If the plugin doesn't appear in Tekla:
1. Restart Tekla Structures
2. Check that the DLL is in the correct directory
3. Verify the plugin name matches the class attribute in the code

## Support
The plugin uses the Tekla Open API for creating structural elements with proper materials and profiles.