using System;
using UnityEngine;

namespace Octrees
{
    public class OctreeGenerator : MonoBehaviour
    {
        public GameObject[] objects;
        public float minNdoeSize = 1f;
        public Octree ot;

        public readonly Graph.Graph wayPoints = new Graph.Graph();

        private void Awake()
        {
            ot = new Octree(objects, minNdoeSize,wayPoints);
        }

        private void OnDrawGizmos()
        {
            if(!Application.isPlaying)
                return;
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(ot.bounds.center,ot.bounds.size);  
            ot.root.DrawNode();
            ot.graph.DrawGraph();
        }
    }
}