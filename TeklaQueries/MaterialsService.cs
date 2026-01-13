using System;
using System.Collections.Generic;
using System.Linq;

namespace TeklaPlugin.TeklaQueries
{
    public class MaterialsService
    {
        public MaterialsService(Tekla.Structures.Model.Model model)
        {
            // Model reference available if needed for future catalog access
        }

        /// <summary>
        /// Gets all available concrete materials
        /// </summary>
        /// <returns>List of concrete material names</returns>
        public List<string> GetConcreteMaterials()
        {
            // Common concrete materials used in construction
            return new List<string>
            {
                "C12/15", "C16/20", "C20/25", "C25/30", "C30/37", "C35/45",
                "C40/50", "C45/55", "C50/60", "C55/67", "C60/75", "C70/85", "C80/95", "C90/105"
            };
        }

        /// <summary>
        /// Gets steel materials only
        /// </summary>
        /// <returns>List of steel material names</returns>
        public List<string> GetSteelMaterials()
        {
            return new List<string>
            {
                "S235", "S275", "S355", "S420", "S460", "S500",
                "S235JR", "S235JO", "S235J2G3", "S235J2G4",
                "S275JR", "S275JO", "S275J2G3", "S275J2G4",
                "S355JR", "S355JO", "S355J2G3", "S355J2G4",
                "S355K2", "S355NL", "S355ML"
            };
        }

        /// <summary>
        /// Gets all available materials (concrete + steel)
        /// </summary>
        /// <returns>List of all material names</returns>
        public List<string> GetAllMaterials()
        {
            var allMaterials = new List<string>();
            allMaterials.AddRange(GetConcreteMaterials());
            allMaterials.AddRange(GetSteelMaterials());
            return allMaterials.OrderBy(m => m).ToList();
        }

        /// <summary>
        /// Validates if a material is in our known list
        /// </summary>
        /// <param name="materialName">Material name to validate</param>
        /// <returns>True if material is valid</returns>
        public bool IsValidMaterial(string materialName)
        {
            return GetAllMaterials().Contains(materialName, StringComparer.OrdinalIgnoreCase);
        }
    }
}