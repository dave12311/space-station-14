#nullable enable
using Content.Server.GameObjects.Components.Interactable;
using Content.Server.GameObjects.Components.Items;
using Content.Server.GameObjects.Components.Items.Posters;
using Content.Shared.Audio;
using Content.Shared.GameObjects.Components.Interactable;
using Content.Shared.Interfaces;
using Content.Shared.Interfaces.GameObjects.Components;
using Robust.Server.GameObjects;
using Robust.Server.GameObjects.EntitySystems;
using Robust.Server.Interfaces.GameObjects;
using Robust.Shared.GameObjects;
using Robust.Shared.GameObjects.Systems;
using Robust.Shared.IoC;
using Robust.Shared.Localization;
using Robust.Shared.Log;
using Robust.Shared.Prototypes;
using Robust.Shared.ViewVariables;
using System.Linq;
using System.Threading.Tasks;

namespace Content.Server.GameObjects.Components.Posters
{
    [RegisterComponent]
    class PosterPlacedComponent : Component, IInteractHand, IInteractUsing
    {
        public override string Name => "PosterPlaced";

        [Dependency] private readonly IPrototypeManager _prototypeManager = default!;

        private bool _ripped = false;

        private string? _currentPoster;

        [ViewVariables(VVAccess.ReadWrite)]
        public string? CurrentPoster
        {
            get => _currentPoster;
            set
            {
                _currentPoster = value;
                UpdatePoster();
            }
        }

        private const string RipSound = "/Audio/Items/poster_ripped.ogg";
        private const string RemoveSound = "/Audio/Items/wirecutter.ogg";
        private const float SoundVolume = -3F;
        private const float SoundVariation = 0.15F;

        private void UpdatePoster()
        {
            if (_currentPoster == null) return;

            if (!_prototypeManager.TryIndex(_currentPoster, out PosterPrototype prototype))
            {
                Logger.ErrorS("poster", $"Invalid poster prototype: \"{_currentPoster}\"");
                return;
            }

            Owner.Name = prototype.Contraband ? Loc.GetString("contraband poster") : Loc.GetString("motivational poster")
                + " - " + prototype.Name;
            Owner.Description = prototype.Description;

            if (Owner.TryGetComponent(out SpriteComponent? sprite))
            {
                sprite.LayerSetState(0, prototype.State);
            }
        }

        public void RipPoster()
        {
            EntitySystem.Get<AudioSystem>()
                .PlayFromEntity(RipSound, Owner, AudioHelpers
                .WithVariation(SoundVariation).WithVolume(SoundVolume));

            if (!Owner.TryGetComponent<SpriteComponent>(out var spriteComponent))
                return;

            spriteComponent.LayerSetState(0, "poster_ripped");
            Owner.Name = Loc.GetString("ripped poster");
            Owner.Description = Loc.GetString("You can't make out anything from the poster's original print. It's ruined.");

            _ripped = true;
        }

        public void DestroyPoster()
        {
            if(Owner.EntityManager.TryGetEntity(Owner.Transform.ParentUid, out var parentWall))
            {
                if(parentWall.TryGetComponent<PosterPlaceableComponent>(out var placeableComponent))
                {
                    placeableComponent.HasPoster = false;
                }
            }

            Owner.Delete();
        }

        public bool InteractHand(InteractHandEventArgs eventArgs)
        {
            if(_ripped) return false;
            RipPoster();
            return true;
        }

        public async Task<bool> InteractUsing(InteractUsingEventArgs eventArgs)
        {
            if (!_ripped) return false;

            if (!eventArgs.Using.TryGetComponent<ToolComponent>(out var tool))
                return false;

            if (tool.HasQuality(ToolQuality.Cutting))
            {
                eventArgs.User.PopupMessage(Loc.GetString("You remove the remnants of the poster."));
                EntitySystem.Get<AudioSystem>().PlayFromEntity(RemoveSound, Owner);
                DestroyPoster();
                return true;
            }

            return false;
        }
    }
}
