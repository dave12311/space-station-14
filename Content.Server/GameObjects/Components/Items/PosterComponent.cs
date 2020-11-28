using Content.Shared.Interfaces.GameObjects.Components;
using Robust.Shared.GameObjects;
using Robust.Shared.Log;
using Robust.Shared.Prototypes;
using Robust.Shared.IoC;
using Robust.Shared.Serialization;
using Robust.Shared.Interfaces.Random;
using Robust.Server.GameObjects;
using System.Linq;
using Robust.Shared.Random;

namespace Content.Server.GameObjects.Components.Items
{
    [RegisterComponent]
    public class PosterComponent : Component
    {
        [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
        [Dependency] private readonly IRobustRandom _random = default!;

        public override string Name => "Poster";

        private bool _contraband;

        private PosterPrototype _prototype;

        public override void ExposeData(ObjectSerializer serializer)
        {
            base.ExposeData(serializer);
            serializer.DataField(ref _contraband, "contraband", false);
        }

        public override void Initialize()
        {
            base.Initialize();

            // Select random prototype and set entity name
            System.Collections.Generic.List<PosterPrototype> prototypes;
            prototypes = _prototypeManager.EnumeratePrototypes<PosterPrototype>()
                .Where((_contraband ? p => p.Contraband : p => !p.Contraband))
                .ToList();
            _prototype = _random.Pick(prototypes);
            Owner.Name += " - " + _prototype.Name;
        }
    }
}
