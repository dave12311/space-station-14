using Content.Shared.Interfaces.GameObjects.Components;
using Robust.Shared.GameObjects;
using System.Threading.Tasks;

namespace Content.Server.GameObjects.Components.Items.Posters
{
    [RegisterComponent]
    public class PosterPlaceableComponent : Component
    {
        public override string Name => "PosterPlaceable";
        public bool HasPoster { get; set; }
    }
}
