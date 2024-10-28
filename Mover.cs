using System.Collections.Generic;
using System.Linq;
using Octrees.Graph;
using UnityEngine;

namespace Octrees
{
    public class Mover : MonoBehaviour
    {
        private float speed = 5f;
        private float accuracy = 1f;
        private float turnSpeed = 5f;

        private int curWayPoint;

        OctreeNode currentNode;
        Vector3 destination;

        public OctreeGenerator octreeGenerator;
        Graph.Graph graph;


        void Start()
        {
            graph = octreeGenerator.wayPoints;
            currentNode = GetClosestNode(transform.position);
            GetRandomDestination();
            
        }

        void Update()
        {
            if (graph == null) return;

            if (graph.GetPathLength() == 0 || curWayPoint >= graph.GetPathLength())
            {
                GetRandomDestination();
                return;
            }

            if (Vector3.Distance(graph.GetPathNode(curWayPoint)._bounds.center, transform.position) < accuracy)
            {
                curWayPoint++;
                Debug.Log($"Waypoint {curWayPoint} reached");
            }

            if (curWayPoint < graph.GetPathLength())
            {
                currentNode = graph.GetPathNode(curWayPoint);
                destination = currentNode._bounds.center;

                Vector3 direction = destination - transform.position;
                direction.Normalize();

                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction),
                    turnSpeed * Time.deltaTime);
                transform.Translate(0, 0, speed * Time.deltaTime);
            }
            else
            {
                GetRandomDestination();
            }
        }

        OctreeNode GetClosestNode(Vector3 position)
        {
            return octreeGenerator.ot.FindClosestNode(transform.position);
        }

        void GetRandomDestination()
        {
            OctreeNode destinationNode;
            do
            {
                destinationNode = graph.nodes.ElementAt(Random.Range(0, graph.nodes.Count)).Key;
            } while (!graph.AStar(currentNode, destinationNode));

            curWayPoint = 0;
        }

        void OnDrawGizmos()
        {
            if (graph == null || graph.GetPathLength() == 0) return;

            Gizmos.color = Color.red;
            Gizmos.DrawSphere(graph.GetPathNode(0)._bounds.center, 0.2f);

            Gizmos.color = Color.blue;
            Gizmos.DrawSphere(graph.GetPathNode(graph.GetPathLength() - 1)._bounds.center, 0.2f);

            for (int i = 0; i < graph.GetPathLength(); i++)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawSphere(graph.GetPathNode(i)._bounds.center, 0.2f);
                if (i < graph.GetPathLength() - 1)
                {
                    Vector3 start = graph.GetPathNode(i)._bounds.center;
                    Vector3 end = graph.GetPathNode(i + 1)._bounds.center;
                    Gizmos.color = Color.red;
                    Gizmos.DrawLine(start, end);
                }
            }
        }
    }
}