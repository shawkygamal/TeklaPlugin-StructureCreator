@echo off
echo Installing Tekla Structure Creator Plugin...
echo.

REM Create plugin directory
mkdir "C:\Program Files\Tekla Structures\2024.0\bin\plugins\Tekla\Model\StructureCreator"

REM Copy plugin DLL
copy "C:\Temp\TeklaPlugin.dll" "C:\Program Files\Tekla Structures\2024.0\bin\plugins\Tekla\Model\StructureCreator\"

echo.
echo Plugin installed successfully!
echo You can now access it in Tekla Structures under Applications menu.
echo.
pause