using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class AStar
{
    private class Node
    {
        public Vector2Int position;
        public Node parent;
        public float gCost; 
        public float hCost; 
        public float fCost => gCost + hCost; 

        public Node(Vector2Int pos)
        {
            position = pos;
        }
    }

    private GridManager grid;

    public AStar(GridManager gridManager)
    {
        grid = gridManager;
    }

  
    public Vector2Int FindNextDirection(Vector2Int start, Vector2Int target)
    {
        List<Vector2Int> path = FindPath(start, target);

        if (path != null && path.Count > 1)
        {
           
            Vector2Int nextPos = path[1];
            return nextPos - start;
        }

       
        return Vector2Int.zero;
    }


    private List<Vector2Int> FindPath(Vector2Int start, Vector2Int target)
    {

        if (!grid.IsWalkable(target))
        {
            return null;
        }

        List<Node> openSet = new List<Node>();
        HashSet<Vector2Int> closedSet = new HashSet<Vector2Int>();

        Node startNode = new Node(start);
        startNode.gCost = 0;
        startNode.hCost = GetHeuristic(start, target);

        openSet.Add(startNode);

        int maxIterations = 1000; 
        int iterations = 0;

        while (openSet.Count > 0 && iterations < maxIterations)
        {
            iterations++;

         
            Node currentNode = openSet.OrderBy(n => n.fCost).ThenBy(n => n.hCost).First();

           
            if (currentNode.position == target)
            {
                return ReconstructPath(currentNode);
            }

            openSet.Remove(currentNode);
            closedSet.Add(currentNode.position);

    
            foreach (Vector2Int direction in GetDirections())
            {
                Vector2Int neighborPos = currentNode.position + direction;

           
                if (closedSet.Contains(neighborPos) || !grid.IsWalkable(neighborPos))
                {
                    continue;
                }

                float tentativeGCost = currentNode.gCost + 1;

                Node neighborNode = openSet.FirstOrDefault(n => n.position == neighborPos);

                if (neighborNode == null)
                {
                    neighborNode = new Node(neighborPos);
                    neighborNode.gCost = tentativeGCost;
                    neighborNode.hCost = GetHeuristic(neighborPos, target);
                    neighborNode.parent = currentNode;
                    openSet.Add(neighborNode);
                }
                else if (tentativeGCost < neighborNode.gCost)
                {
                    neighborNode.gCost = tentativeGCost;
                    neighborNode.parent = currentNode;
                }
            }
        }

   
        return null;
    }


    private float GetHeuristic(Vector2Int a, Vector2Int b)
    {
        return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
    }


    private List<Vector2Int> ReconstructPath(Node endNode)
    {
        List<Vector2Int> path = new List<Vector2Int>();
        Node current = endNode;

        while (current != null)
        {
            path.Add(current.position);
            current = current.parent;
        }

        path.Reverse();
        return path;
    }


    private Vector2Int[] GetDirections()
    {
        return new Vector2Int[]
        {
            Vector2Int.up,
            Vector2Int.down,
            Vector2Int.left,
            Vector2Int.right
        };
    }
}