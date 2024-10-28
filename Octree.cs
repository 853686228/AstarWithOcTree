using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Octrees
{
    public class Octree
    {
        public OctreeNode root;
        public Bounds bounds;
        public Graph.Graph graph;
        private List<OctreeNode> emptyLeaves = new List<OctreeNode>();

        public Octree(GameObject[] objects, float minNodeSize, Graph.Graph wayPoints)
        {
            graph = wayPoints;
            CalculateBounds(objects);
            CreateTree(objects, minNodeSize);

            GetEmptyLeaves(root);
            GetEdges(); //在叶节点中创建相邻边
        }

        public OctreeNode FindClosestNode(Vector3 position) => FindClosestNode(root, position);

        public OctreeNode FindClosestNode(OctreeNode node, Vector3 position)
        {
            if (node.IsLeaf)
                return node;
            foreach (var child in node.children)
            {
                if (child._bounds.Contains(position))
                {
                    return FindClosestNode(child, position);
                }
            }

            return node;
        }

        private void GetEdges()
        {
            int t = 0;
            for (int i = 0; i < emptyLeaves.Count; ++i)
            {
                for (int j = 0; j < emptyLeaves.Count; ++j)
                {
                    if (emptyLeaves[i]._bounds.Intersects(emptyLeaves[j]._bounds))
                    {
                        ++t;
                        graph.AddEdge(emptyLeaves[i], emptyLeaves[j]);
                    }
                }
            }
            Debug.LogWarning(emptyLeaves.Count +"   " +t);
        }


        /// <summary>
        /// 这个算法的核心是所有物体都必须位于叶节点中
        /// </summary>
        /// <param name="node"></param>
        void GetEmptyLeaves(OctreeNode node)
        {
            if (node.IsLeaf)
            {
                emptyLeaves.Add(node);
                graph.AddNode(node);
                return;
            }

            if (node.children == null)
                return;

            foreach (var child in node.children)
            {
                GetEmptyLeaves(child);
            }

            for (int i = 0; i < node.children.Length; ++i)
            {
                for (int j = i + 1; j < node.children.Length; ++j)
                {
                    graph.AddEdge(node.children[i], node.children[j]); //只有叶节点才能成功加入
                }
            }
        }

        private void CreateTree(GameObject[] worldObjects, float minNodeSize)
        {
            root = new OctreeNode(bounds, minNodeSize);

            foreach (var obj in worldObjects)
            {
                root.Divide(obj);
            }
        }

        public void CalculateBounds(GameObject[] objects)
        {
            foreach (var obj in objects)
            {
                var b = obj.GetComponent<Collider>().bounds;
                if (bounds == default)
                    bounds = b;
                else
                    bounds.Encapsulate(b);
            }

            var max = Mathf.Max(bounds.extents.x, bounds.extents.y, bounds.extents.z);
            bounds.size = Vector3.one * max * 2.1f; //调整成一个正方形，并稍微拓展一点边界，浮点精度问题
        }

        public void Encapsulate_DetailImpl(Bounds bounds)
        {
            var min = Vector3.Min(bounds.min, this.bounds.min);
            var max = Vector3.Max(bounds.max, this.bounds.max);
            this.bounds.SetMinMax(min, max);
        }
    }
}