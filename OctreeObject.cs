using UnityEngine;

namespace Octrees
{
    public class OctreeObject
    {
        public Bounds _bounds;

        public OctreeObject(GameObject obj)
        {
            _bounds = obj.GetComponent<Collider>().bounds;
        }

        public bool Intersects(Bounds other) => _bounds.Intersects(other);

        public bool Intersects_DetailImpl(Bounds other)
        {
            return (_bounds.min.x <= other.max.x && _bounds.max.x >= other.min.x) &&
                   (_bounds.min.y <= other.max.y && _bounds.max.y >= other.min.y) &&
                   (_bounds.min.z <= other.max.z && _bounds.max.z >= other.min.z);
        }
    }
}