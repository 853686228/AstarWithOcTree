using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

namespace Octrees.Graph
{
    public class Graph
    {
        private const int MAX_ITERATION_TIMES = 1000;

        //Astar算法运行时Node与OctreeNode一一对应，是按照OctreeNode的业叶节点作为可寻路的Node
        public readonly Dictionary<OctreeNode, Node>
            nodes = new Dictionary<OctreeNode, Node>(); //所有的叶节点, 用字典保证不重复、能通过空间中的OctreeNode结点映射到寻路中Graph的节点数据结构

        public readonly HashSet<Edge> edges = new HashSet<Edge>(); //相邻节点的联通边

        private List<Node> pathList = new List<Node>();
        public int GetPathLength() => pathList.Count;
        public List<Node> GetPath() => pathList;

        public OctreeNode GetPathNode(int index) => pathList[index].OctreeNode;

        public void AddNode(OctreeNode octreeNode)
        {
            if (!nodes.ContainsKey(octreeNode))
            {
                nodes.Add(octreeNode, new Node(octreeNode));
            }
        }

        Node FindNode(OctreeNode octreeNode)
        {
            nodes.TryGetValue(octreeNode, out var node);
            return node;
        }

        public void AddEdge(OctreeNode start, OctreeNode end)
        {
            var startNode = FindNode(start);
            var endNode = FindNode(end);
            if (startNode == null || endNode == null)
            {
                return;
            }

            var edge = new Edge(startNode, endNode);
            if (!edges.Contains(edge))
            {
                edges.Add(edge);
                startNode.edges.Add(edge);
                endNode.edges.Add(edge);
            }
        }


        /// <summary>
        /// 将上次寻路使用的变量进行重置
        /// </summary>
        private void ResetAllNodeValue()
        {
            foreach (var node in nodes.Values)
            {
                node.g = int.MaxValue;
                node.h = int.MaxValue;
            }
        }

        /// <summary>
        /// 常规Astar算法，不赘述
        /// </summary>
        /// <param name="startNode"></param>
        /// <param name="endNode"></param>
        /// <returns></returns>
        public bool AStar(OctreeNode startNode, OctreeNode endNode)
        {
            pathList.Clear();
            ResetAllNodeValue();

            Node start = FindNode(startNode);
            Node end = FindNode(endNode);

            if (start == null || end == null)
            {
                Debug.LogError("起点/终点不在Graph内");
                return false;
            }

            SortedSet<Node> openSet = new SortedSet<Node>(new NodeComparer());
            HashSet<Node> closedSet = new HashSet<Node>();
            int iterationCount = 0;

            start.g = 0;
            start.h = EuclideanDistance(start, end);
            start.from = null;
            openSet.Add(start);

            while (openSet.Count > 0)
            {
                if (++iterationCount > MAX_ITERATION_TIMES) //建议给一个最大迭代次数，防止死循环/溢出等问题
                {
                    Debug.LogError("超出最大迭代此处");
                    return false;
                }

                Node cur = openSet.First();
                openSet.Remove(cur);

                if (cur.Equals(end))
                {
                    ReconstructPath(cur);
                    return true;
                }

                closedSet.Add(cur);
                foreach (var edge in cur.edges)
                {
                    Node neighbor = edge.Start.Equals(cur) ? edge.End : edge.Start;
                    if (closedSet.Contains(neighbor))
                        continue;

                    var costToNeighbor = cur.g + EuclideanDistance(cur, neighbor);
                    if (!openSet.Contains(neighbor) && (costToNeighbor < neighbor.g))
                    {
                        neighbor.g = costToNeighbor;
                        neighbor.h = EuclideanDistance(neighbor, end);
                        neighbor.from = cur;
                        openSet.Add(neighbor);
                    }
                }
            }

            Debug.LogError("未找到合理Path");
            return false;
        }

        private void ReconstructPath(Node node)
        {
            while (node != null)
            {
                pathList.Add(node);
                node = node.from;
            }

            pathList.Reverse();
        }

        /// <summary>
        /// 这里可以用其他的启发式函数计算代价，如Chebyshev/Manhattan
        /// 这里直接用了欧氏距离
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        float EuclideanDistance(Node a, Node b) =>
            (a.OctreeNode._bounds.center - b.OctreeNode._bounds.center).sqrMagnitude;

        public void DrawGraph()
        {
            Gizmos.color = Color.blue;
            //绘制边
            foreach (var edge in edges)
            {
                Gizmos.DrawLine(edge.Start.OctreeNode._bounds.center, edge.End.OctreeNode._bounds.center);
            }

            //绘制Node节点
            foreach (var node in nodes.Values)
            {
                Gizmos.DrawWireSphere(node.OctreeNode._bounds.center, .2f);
            }
        }
    }

    public class NodeComparer : IComparer<Node>
    {
        public int Compare(Node x, Node y)
        {
            if (x == null || y == null)
                return 0;
            int compare = x.f.CompareTo(y.f);
            if (compare == 0)
                return x.id.CompareTo(y.id);
            return compare;
        }
    }

    public class Node
    {
        private static int nextId;
        public readonly int id;
        public float g, h;
        public float f => g + h;
        public Node from;

        public List<Edge> edges = new List<Edge>(); //与这个节点相连的通路

        public OctreeNode OctreeNode; //此处引用其对应的OctreeNode是为了方便计算代价（实际距离是通过OctreeNode的中心距离来计算的）

        public Node(OctreeNode node)
        {
            this.id = nextId++;
            this.OctreeNode = node;
        }
    }

    public class Edge
    {
        public Node Start;
        public Node End;

        public Edge(Node start, Node end)
        {
            Start = start;
            End = end;
        }

        public override bool Equals(object obj)
        {
            return obj is Edge other &&
                   (Start == other.Start && End == other.End || Start == other.End && End == other.Start);
        }

        public override int GetHashCode()
        {
            return Start.GetHashCode() ^ End.GetHashCode();
        }

        public static bool operator ==(Edge left, Edge right)
        {
            // 处理 null 情况
            if (ReferenceEquals(left, null)) return ReferenceEquals(right, null);
            if (ReferenceEquals(right, null)) return false;

            return left.Equals(right);
        }

        public static bool operator !=(Edge left, Edge right)
        {
            return !(left == right);
        }
    }
}