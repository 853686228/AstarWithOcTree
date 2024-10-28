using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Octrees
{
    public class OctreeNode
    {
        public List<OctreeObject> objects = new();

        private static int nextId;
        public readonly int id;

        public Bounds _bounds;
        private Bounds[] childBounds = new Bounds[8];
        public OctreeNode[] children;
        public bool IsLeaf => children == null;

        private float minNodeSize;

        public OctreeNode(Bounds bounds, float minNodeSize)
        {
            id = nextId++;
            this._bounds = bounds;
            this.minNodeSize = minNodeSize;

            //切分成8份
            Vector3 newSize = bounds.size * .5f;
            Vector3 centerOffset = bounds.size * .25f;
            Vector3 parentCenter = bounds.center;

            for (int i = 0; i < 8; ++i)
            {
                Vector3 childCenter = parentCenter;
                childCenter.x += ((i & 1) == 0 ? -1 : 1) * centerOffset.x;
                childCenter.y += ((i & 2) == 0 ? -1 : 1) * centerOffset.y;
                childCenter.z += ((i & 4) == 0 ? -1 : 1) * centerOffset.z;
                childBounds[i] = new Bounds(childCenter, newSize);
            }
        }

        /// <summary>
        /// 调用这个接口的前提是这个Obj一定在当前节点的Bounds之内
        /// </summary>
        /// <param name="obj"></param>
        public void Divide(GameObject obj) => Divide(new OctreeObject(obj));

        /// <summary>
        /// 将一个物体插入到树中
        /// </summary>
        /// <param name="obj"></param>
        public void Divide(OctreeObject obj)
        {
            if (_bounds.size.x <= minNodeSize) // 太小了 不继续切分
            {
                AddObject(obj);
                return;
            }
            else
            {
                children ??= new OctreeNode[8];
                bool intersected = false;
                for (int i = 0; i < 8; ++i)
                {
                    children[i] ??= new OctreeNode(childBounds[i], minNodeSize);

                    //TODO: 如果同时在多个子空间内，可以直接放在本层，就不往下递归了
                    if (obj.Intersects(childBounds[i]))
                    {
                        children[i].Divide(obj);
                        intersected = true;
                    }
                }

                if (!intersected)
                {
                    AddObject(obj);
                }
            }
        }

        public void AddObject(OctreeObject obj) => objects.Add(obj);

        public void DrawNode()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(_bounds.center, _bounds.size);

            foreach (var VARIABLE in objects)
            {
                if (VARIABLE.Intersects(_bounds))
                {
                    Gizmos.color = Color.red;
                    Gizmos.DrawCube(VARIABLE._bounds.center, VARIABLE._bounds.size * 1.01f);
                }
            }

            if (children != null)
            {
                foreach (var child in children)
                {
                    child.DrawNode();
                }
            }
        }
    }
}