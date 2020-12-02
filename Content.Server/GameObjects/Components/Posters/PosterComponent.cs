using Content.Shared.Interfaces.GameObjects.Components;
using Robust.Shared.GameObjects;
using Robust.Shared.Prototypes;
using Robust.Shared.IoC;
using Robust.Shared.Serialization;
using Robust.Shared.Interfaces.Random;
using Robust.Server.GameObjects;
using System.Linq;
using Robust.Shared.Random;
using Content.Server.GameObjects.Components.Items.Posters;
using Robust.Shared.GameObjects.Systems;
using Content.Server.GameObjects.EntitySystems.DoAfter;
using Robust.Server.GameObjects.EntitySystems;
using Content.Shared.Audio;
using Content.Server.GameObjects.Components.GUI;
using Content.Shared.Interfaces;
using Robust.Shared.Localization;

namespace Content.Server.GameObjects.Components.Items
{
    [RegisterComponent]
    public class PosterComponent : Component, IAfterInteract
    {
        [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
        [Dependency] private readonly IRobustRandom _random = default!;

        public override string Name => "Poster";

        private bool _contraband;

        private PosterPrototype _prototype;

        private const float PlaceTime = 2.8F;
        private const float SoundVolume = -3F;
        private const float SoundVariation = 0.15F;

        private const string CreateSound = "/Audio/Items/poster_being_created.ogg";

        public override void ExposeData(ObjectSerializer serializer)
        {
            base.ExposeData(serializer);
            serializer.DataField(ref _contraband, "contraband", false);
        }

        public override void Initialize()
        {
            base.Initialize();

            var prototypes = _prototypeManager.EnumeratePrototypes<PosterPrototype>()
                .Where((_contraband ? p => p.Contraband : p => !p.Contraband))
                .ToList();
            _prototype = _random.Pick(prototypes);
            Owner.Name += " - " + _prototype.Name;
        }

        public async void AfterInteract(AfterInteractEventArgs eventArgs)
        {
            var target = eventArgs.Target;

            if(target is null) return;
            if(!target.TryGetComponent<PosterPlaceableComponent>(out var placeableComponent))
                return;

            if(!eventArgs.CanReach) return;

            if(placeableComponent.HasPoster)
            {
                eventArgs.User.PopupMessage(Loc.GetString("The wall is far too cluttered to place a poster."));
                return;
            }

            var placedPoster = Owner.EntityManager.SpawnEntity("PosterPlaced", eventArgs.Target.Transform.Coordinates);
            placedPoster.Transform.AttachParent(target);
            placedPoster.GetComponent<SpriteComponent>().LayerSetState(0, "poster_being_set");

            var placeSound = EntitySystem.Get<AudioSystem>()
                .PlayFromEntity(CreateSound, target, AudioHelpers
                .WithVariation(SoundVariation).WithVolume(SoundVolume));

            var doAfterSystem = EntitySystem.Get<DoAfterSystem>();
            var doAfterArgs = new DoAfterEventArgs(eventArgs.User, PlaceTime, default, target)
            {
                BreakOnDamage = true,
                BreakOnStun = true,
                BreakOnTargetMove = true,
                BreakOnUserMove = true,
                NeedHand = true
            };

            var result = await doAfterSystem.DoAfter(doAfterArgs);

            if(result == DoAfterStatus.Cancelled)
            {
                placedPoster.Delete();

                placeSound.Stop();

                if(!eventArgs.User.TryGetComponent<HandsComponent>(out var handsComponent)) { return; }
                handsComponent.Drop(Owner);

                eventArgs.User.PopupMessage(Loc.GetString("The poster falls down!"));
                return;
            }

            placedPoster.Name = _prototype.Name;
            placedPoster.Description = _prototype.Description;
            placedPoster.GetComponent<SpriteComponent>().LayerSetState(0, _prototype.State);

            placeableComponent.HasPoster = true;
            Owner.Delete();
        }
    }
}
