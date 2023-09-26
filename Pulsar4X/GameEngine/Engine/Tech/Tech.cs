using Pulsar4X.Blueprints;
using Pulsar4X.Components;

namespace Pulsar4X.Engine
{
    public class Tech : TechBlueprint
    {
        public ComponentDesign Design { get; set; }
        public Entity Faction { get; set; }
        public int Level { get; set; } = 0;

        public Tech() { }

        public Tech(TechBlueprint blueprint)
        {
            FullIdentifier = blueprint.FullIdentifier;
            UniqueID = blueprint.UniqueID;
            Name = blueprint.Name;
            Description = blueprint.Description;
            MaxLevel = blueprint.MaxLevel;
            CostFormula = blueprint.CostFormula;
            DataFormula = blueprint.DataFormula;
            Category = blueprint.Category;
            Unlocks = blueprint.Unlocks;
        }
    }
}