using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class RepositionObject : MonoBehaviour {

    public GridManager gridManager;

	// Use this for initialization
	void Start () {
        Reposition();
	}
    
    // Update is called once per frame
    void Update () {
        
	}

    /// <summary>
    /// Function to reposition GameObject it is attached to, to nearest walkable Node from Grid.
    /// </summary>
    public void Reposition()
    {
        if (gridManager.grid != null)
        {
            Queue<Node> queue = new Queue<Node>();

            Node node = gridManager.grid.GetNodeFromPosition(GetComponent<Transform>().position);
            queue.Enqueue(node);

            while (!node.walkable && queue.Count > 0)
            {
                Action<int, int> enqueue = (x, y) =>
                {
                    if (gridManager.grid.IsWithin(x, y))
                    {
                        Node tNode = gridManager.grid[x, y];
                        if (!queue.Contains(tNode))
                        {
                            queue.Enqueue(tNode);
                        }
                    }
                };

                enqueue.Invoke(node.x - 1, node.y);
                enqueue.Invoke(node.x + 1, node.y);
                enqueue.Invoke(node.x, node.y - 1);
                enqueue.Invoke(node.x, node.y + 1);

                node = queue.Dequeue();
            }

            if (node != null)
            {
                GetComponent<Transform>().position = node.position;
            }
        }
    }
}
