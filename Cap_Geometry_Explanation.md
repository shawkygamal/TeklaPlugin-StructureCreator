# Cap Geometry Explanation

## Cap Parameters

The cap beam has a **trapezoidal cross-section** (front view) that sits on top of the elevation columns.

### Parameter Definitions

```
Front View (2D):
     B (top width)
  ┌───────────────┐
  │               │
  │               │  ← SlopeHeight (where slope starts)
  │               │
  └───────────────┘
        (wider)
        H (height)
```

### Parameters:

1. **H (Height)**
   - Vertical dimension (Z direction)
   - Total height of the cap beam
   - Default: 500 mm

2. **B (Top Width)**
   - Transverse dimension (Y direction in local coordinates)
   - Width at the top of the trapezoid
   - **Visible in 2D front view**
   - Default: 2000 mm

3. **W (Depth/Width)**
   - Longitudinal dimension (X direction in local coordinates)
   - The dimension going "into the page" in a front view
   - **NOT visible in 2D front view**
   - Default: 4000 mm

4. **P (Position Offset)**
   - Offset from the column center
   - Allows positioning the cap offset from the column axis
   - Applied in the local X direction (along W dimension)
   - Default: 0 mm (centered on column)

5. **SlopeHeight**
   - The height at which the trapezoidal slope begins
   - Measured from the bottom of the cap
   - Creates the angled sides of the trapezoid
   - Default: 250 mm

## How It Works

1. The cap is created as a rectangular beam with dimensions `(B + skew_adjustment) × W`
2. The skew adjustment accounts for the global skew angle applied to the structure
3. Cut planes are applied to create:
   - **Skewed sides** (in plan view) based on the global skew angle
   - **Sloped sides** (in elevation view) creating the trapezoidal shape
4. The cap is positioned:
   - Centered on the column + P offset
   - At height: `global.PositionZ + elevationHeight`
   - Extends upward by H

## Coordinate System

- **Global X**: Typically along the structure length
- **Global Y**: Typically across the structure width  
- **Global Z**: Vertical (upward positive)

The cap can be rotated by the global rotation angle, and all dimensions follow the rotated coordinate system.

## Trapezoid Shape

The trapezoidal profile is created by:
- Starting with a rectangular profile
- Applying lateral cut planes at SlopeHeight from the bottom
- The cut planes have a slope factor of ±0.4 (creating angled sides)
- This makes the top narrower than the bottom (inverted trapezoid)

## Example Values

For a typical bridge pier cap:
- H = 500 mm (height)
- B = 2000 mm (top width)
- W = 4000 mm (length along pier)
- P = 0 mm (centered)
- SlopeHeight = 250 mm (slope starts halfway up)