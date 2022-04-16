using Duck.Serialization;
using Silk.NET.Maths;

namespace Duck.Scene.Components
{
    [AutoSerializable]
    public partial struct BoundingBoxComponent
    {
        public Box3D<float> Box = default;
    }
}
