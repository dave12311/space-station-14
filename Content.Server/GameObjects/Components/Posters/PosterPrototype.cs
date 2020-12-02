using Robust.Shared.Localization;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;
using YamlDotNet.RepresentationModel;

namespace Content.Server.GameObjects.Components.Items
{
    [Prototype("poster")]
    public class PosterPrototype : IPrototype, IIndexedPrototype
    {
        public string ID { get; private set; }
        public string Name { get; private set; }
        public string Description { get; private set; }
        public string State { get; private set; }
        public bool Contraband { get; private set; }

        public void LoadFrom(YamlMappingNode mapping)
        {
            ID = mapping.GetNode("id").AsString();
            Name = Loc.GetString(mapping.GetNode("name").AsString());
            State = mapping.GetNode("state").AsString();

            if (mapping.TryGetNode("description", out var node))
            {
                Description = Loc.GetString(node.AsString());
            }

            if (mapping.TryGetNode("contraband", out node))
            {
                Contraband = node.AsBool();
            }
        }
    }
}
