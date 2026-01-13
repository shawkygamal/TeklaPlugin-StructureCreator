# TeklaPlugin Project Structure V2

## 📁 Enhanced Folder Organization with Subfolders

### 🎨 Forms/
**Purpose:** Contains all Windows Forms organized by type
```
Forms/
├── Main/                           # Main application forms
│   ├── StructureCreatorForm.cs          # Main form with parameter inputs
│   ├── StructureCreatorForm.Designer.cs # Form designer code (auto-generated)
│   └── StructureCreatorForm.resx       # Form resources
└── Plugins/                        # Tekla plugin interfaces
    └── StructureCreatorPlugin.cs       # Tekla plugin entry point
```

### 🏗️ Services/
**Purpose:** Individual services for each structural element with co-located parameter models
```
Services/
├── Core/                          # Core services and shared parameters
│   ├── StructureCreatorService.cs     # Main orchestrator service
│   └── Models/                        # Shared parameters only
│       └── GlobalParameters.cs            # Position, rotation, skew (shared)
├── Foundation/                    # Foundation services & models
│   ├── FoundationService.cs           # Foundation beams + rebar creation
│   └── Models/
│       └── FoundationParameters.cs        # Foundation dimensions (W, L, H, Material, Class)
├── Mat/                           # Mat foundation services & models
│   ├── MatService.cs                  # Mat foundation creation
│   └── Models/
│       └── MatParameters.cs               # Mat specifications (Cantilever, Thickness, Material, Class)
├── Piles/                         # Pile services & models
│   ├── PilesService.cs                # Pile grid creation
│   └── Models/
│       └── PileParameters.cs              # Pile layout (Rows, Columns, spacing, Material, Class)
├── Elevation/                     # Elevation/column services & models
│   ├── ElevationService.cs            # Lamelar/circular column creation
│   └── Models/
│       └── ElevationParameters.cs         # Column specifications (Lamelar/Circular, Material, Class)
└── Cap/                           # Cap beam services & models
    ├── CapService.cs                  # Trapezoidal cap beam creation
    └── Models/
        └── CapParameters.cs               # Cap dimensions (H, B, W, P, SlopeHeight, Material, Class)
```

### 🎨 Assets/
**Purpose:** Branding and visual assets
```
Assets/
└── InfraNovaLogo.webp             # Company logo for UI branding
```

### 🔍 TeklaQueries/
**Purpose:** Services for querying Tekla-specific data
```
TeklaQueries/
└── MaterialsService.cs             # Material catalog access service
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
User Input (Forms/Main/StructureCreatorForm)
    ↓
Plugin Interface (Forms/Plugins/StructureCreatorPlugin)
    ↓
StructureCreatorService (Services/Core/StructureCreatorService)
    ↓
├── FoundationService (Services/Foundation/) → Creates foundation + rebar
├── MatService (Services/Mat/) → Creates mat foundation
├── PilesService (Services/Piles/) → Creates pile grid
├── ElevationService (Services/Elevation/) → Creates columns/beams
└── CapService (Services/Cap/) → Creates trapezoidal cap
    ↓
Tekla Model (Final structure)
```

## 📊 Benefits of Co-Located Models Architecture

### ✅ Enhanced Separation of Concerns
- **Forms/Main/**: User interface logic
- **Forms/Plugins/**: Tekla integration logic
- **Services/Core/**: Orchestration logic
- **Services/[Element]/**: Element-specific logic + parameters

### ✅ Tight Coupling Where Appropriate
- **Models co-located with services**: Parameters evolve with their services
- **Shared models in Core**: GlobalParameters available to all services
- **Element-specific models**: FoundationParameters only in Foundation service

### ✅ Improved Maintainability
- Related files are co-located (service + its parameters)
- Easy to find and modify specific functionality
- Clear ownership boundaries

### ✅ Better Scalability
- Add new services with their models in dedicated folders
- Extend existing services without affecting others
- Models can be versioned with their services

## 🎨 Modern UI & Material Selection Features

### Branded Modern User Interface
- **InfraNova Logo:** Prominent company logo embedded as resource in header (with fallback text logo)
- **Application Icon:** InfraNova logo set as the form's window icon for professional branding
- **Professional Header:** Gray header panel with logo, title, and subtitle
- **Company Branding:** "Powered by InfraNova" branding text
- **Copyright Footer:** Professional copyright and version information at bottom
- **Clean Layout:** Simple tabbed interface with accessible input fields
- **Direct Input:** All textboxes placed directly on tabs for immediate access
- **Modern Styling:** Segoe UI fonts, flat buttons, consistent colors
- **Easy Navigation:** Clear tab structure without nested complexity
- **Fully Accessible:** All controls are clickable and accept input immediately
- **Tooltips:** Helpful tooltips for guidance

### Enhanced Per-Tab Visualizations with Parameter Labels
- **Interactive Parameter-Labeled Diagrams:** Detailed visualizations showing parameter relationships
- **Auto-Update:** Visualizations refresh automatically when switching tabs
- **Dimension Indicators:** Color-coded arrows and labels showing:
  - **Blue labels:** Foundation dimensions (Width, Length, Height)
  - **Green labels:** Mat dimensions (Cantilever, Thickness)
  - **Purple labels:** Pile dimensions (Row/Column spacing, Diameter)
  - **Red labels:** Elevation dimensions (Width, Height, Diameter, Distance)
  - **Orange labels:** Cap dimensions (H-Height, B-Top Width, Slope)
- **Enhanced Component Illustrations:**
  - **Global:** X/Y coordinate system with reference grid and axes
  - **Foundation:** 3D isometric block with Width × Length × Height dimension arrows
  - **Mat:** Slab above foundation showing cantilever extensions and thickness relationships
  - **Piles:** 3×3 grid with spacing indicators, diameter circles, and layout dimensions
  - **Elevation:** Column(s) with height, width/diameter, and spacing measurement lines
  - **Cap:** Trapezoidal beam above column with slope indicators and H/B measurements
- **Educational Design:** Visual learning tool showing parameter relationships and component positioning

### Dynamic Material Dropdowns
Each component tab now includes material selection dropdowns populated with:
- **Foundation:** Concrete materials (C12/15 to C90/105) + Class selection (default "8")
- **Mat:** Concrete materials (default C12/15) + Class selection (default "1")
- **Piles:** Concrete materials (default C50/60) + Class selection (default "8")
- **Elevation:** Concrete materials for both lamelar and circular (default C50/60) + Class selection (default "8")
- **Cap:** Concrete materials (default C12/15) + Class selection (default "8")

### Simple Input System
- **Fully Accessible Textboxes:** All dimension fields are now clickable and editable
- **No Input Restrictions:** Users can type any values freely
- **Default Values Provided:** Sensible defaults pre-filled in all fields
- **No Validation Blocks:** Removed all input validation that was preventing typing
- **Easy Data Entry:** Simple, straightforward text input for all parameters

### Class Selection
All component tabs include class dropdowns with values 1-10:
- **Foundation:** Default "8" (structural concrete)
- **Mat:** Default "1" (mat foundation)
- **Piles:** Default "8" (structural concrete)
- **Elevation:** Default "8" (columns/beams)
- **Cap:** Default "8" (cap beams)

### Material Service Architecture
- **TeklaQueries/MaterialsService.cs**: Provides material catalog access
- **Fallback Materials**: Comprehensive list of common concrete and steel grades
- **Validation**: Ensures selected materials are valid
- **Extensible**: Easy to add new material categories

### User Experience
- **Pre-populated Dropdowns**: All material options loaded on form startup
- **Default Selections**: Sensible defaults for each component type
- **Real-time Updates**: Material changes applied immediately
- **Error Handling**: Graceful fallback if material loading fails

### ✅ Professional Structure
- Follows domain-driven design principles
- Clear naming conventions and namespaces
- Logical grouping by bounded contexts

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

## 📈 Future Extensions

- **Add new plugins**: Create in `Forms/Plugins/`
- **Add new services**: Create in `Services/[NewElement]/`
- **Extend models**: Add to appropriate `Services/[Element]/Models/`
- **Add forms**: Create in `Forms/Main/` or new subfolders

This structure provides maximum flexibility for future development while maintaining clean organization.