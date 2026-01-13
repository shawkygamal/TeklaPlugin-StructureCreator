# TeklaPlugin Project Structure

## 📁 New Folder Organization

### 🎨 Forms/
**Purpose:** Contains all Windows Forms and plugin interfaces
```
Forms/
├── StructureCreatorForm.cs          # Main form with parameter inputs
├── StructureCreatorForm.Designer.cs # Form designer (auto-generated)
├── StructureCreatorForm.resx       # Form resources
└── StructureCreatorPlugin.cs       # Tekla plugin entry point
```

### 🏗️ Services/
**Purpose:** Individual services for each structural element
```
Services/
├── StructureCreatorService.cs      # Main orchestrator service
├── FoundationService.cs            # Handles foundation beams + rebar
├── MatService.cs                   # Handles mat foundations
├── PilesService.cs                 # Handles pile grid creation
├── ElevationService.cs             # Handles lamelar/circular elevations
└── CapService.cs                   # Handles cap beams with trapezoidal shape
```

### 📋 Models/
**Purpose:** Parameter classes and data models
```
Models/
├── GlobalParameters.cs             # Position, rotation, skew
├── FoundationParameters.cs         # Width, length, height
├── MatParameters.cs                # Cantilever, thickness
├── PileParameters.cs               # Grid layout, dimensions
├── ElevationParameters.cs          # Lamelar/circular parameters
└── CapParameters.cs                # H, B, W, P, slope dimensions
```

### 📄 Root Files/
```
├── Program.cs                       # Application entry point
├── TeklaPlugin.csproj              # Project configuration
├── App.config                      # Application configuration
├── .gitignore                      # Git ignore rules
└── Properties/                     # Assembly properties
    ├── AssemblyInfo.cs
    ├── Resources.resx
    ├── Settings.settings
    └── ...
```

## 🔄 Architecture Flow

```
User Input (Form)
    ↓
StructureCreatorService (Orchestrator)
    ↓
├── FoundationService → Creates foundation + rebar
├── MatService → Creates mat foundation
├── PilesService → Creates pile grid
├── ElevationService → Creates columns/beams
└── CapService → Creates trapezoidal cap
    ↓
Tekla Model (Final structure)
```

## 📊 Benefits of New Structure

### ✅ Separation of Concerns
- Each service handles one specific element type
- Forms focus only on UI logic
- Parameters are clearly defined and typed

### ✅ Maintainability
- Easy to modify individual components
- Clear interfaces between layers
- Easier testing and debugging

### ✅ Scalability
- Simple to add new element types
- Easy to create additional plugin forms
- Modular architecture supports growth

### ✅ Code Organization
- Logical grouping by functionality
- Clear naming conventions
- Consistent namespace structure

## 🚀 Usage

### Running as Standalone App:
```bash
# Run the executable directly
TeklaPlugin.exe
```

### Running as Tekla Plugin:
1. Copy `TeklaPlugin.dll` to Tekla plugins folder
2. Access via Tekla Applications menu
3. Plugin name: `StructureCreator`

## 📈 Next Steps

- Add new plugin forms in `Forms/` folder
- Extend services for additional element types
- Add validation and error handling
- Implement unit tests for services