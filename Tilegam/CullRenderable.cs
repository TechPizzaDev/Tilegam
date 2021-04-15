using Veldrid.Utilities;

namespace Tilegam.Client
{
    public abstract class CullRenderable : Renderable
    {
        public abstract BoundingBox BoundingBox { get; }

        public bool Cull(ref BoundingFrustum visibleFrustum)
        {
            return visibleFrustum.Contains(BoundingBox) == ContainmentType.Disjoint;
        }
    }
}
