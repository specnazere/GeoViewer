# Earth's Crust Visualization Tool

This project is a program that allows visualizing a section of the Earth's crust with properties and surface with a well.

## Data Requirements

All data files must be placed in the same directory as the executable file with these exact names (case-sensitive):

- Grid: `grid.bin`
- Grid properties: `grid.binprops.txt`
- Surface: `Surf`
- Well: `well`

## Camera Controls

### Basic Controls (outside interface):
- Rotate camera: Left Mouse Button (LMB)
- Pan camera: Middle Mouse Button (MMB)
- Zoom: Mouse Wheel

### Flight Mode:
- Press `F` to toggle flight mode on/off
- In flight mode:
  - Movement: `W` (forward), `A` (left), `S` (backward), `D` (right)
  - Vertical movement: `Q` (up), `E` (down)
  - Camera rotation: Mouse movement
  - Zoom: Mouse Wheel still works

- Press `R` to reset camera and scene transformations to initial state (other settings will be preserved)

## User Interface

### Design Basics:
- All interactive elements have white fill
- Top-right corner allows horizontal panel resizing
- Button to the left of section headers toggles section visibility
- White square to the left of text (not headers) is a toggleable element

### Slider Controls:
- Adjust by vertical mouse movement
- Bottom of slider window shows minimum value
- Top of slider window shows maximum value
- Red marker indicates default position (restored by "RESET" button)
- Between "RESET" button and slider: text field for manual value input

Each interactive panel includes explanatory labels describing its function.