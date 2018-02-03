using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// T has to implement IHeapItem
public class Heap<T> where T : IHeapItem<T> {

	T[] items;
	int currentItemCount;   // Number of items in Heap.

	public Heap(int maxHeapSize) {
		items = new T[maxHeapSize];
	}

    /// <summary>
    /// Add item to the Heap.
    /// </summary>
	public void Add(T item)
	{
        // Assign next index to the item.
		item.HeapIndex = currentItemCount;
        // Assign item to the Heap.
		items[currentItemCount] = item;

		SortUp(item);
		++currentItemCount;
	}

    /// <summary>
    /// Function to remove first item from the Heap and return it.
    /// </summary>
	public T RemoveFirst()
	{
		T firstItem = items[0];
		--currentItemCount;

        // Assign last item to the Heap.
		items[0] = items[currentItemCount];
		items[0].HeapIndex = 0;

        // Sort it down.
		SortDown(items[0]);
		return firstItem;
	}

	public void UpdateItem(T item)
	{
		SortUp(item);       // We only increase priority, so we doesn't add SortDown.
        // SortDown(item);
    }

	public int Count
	{
		get
		{
			return currentItemCount;
		}
	}

	public bool Contains(T item)
	{
		return Equals(items[item.HeapIndex], item);
	}

    /// <summary>
    /// Function to sort the item down in the hierarchy.
    /// </summary>
	void SortDown(T item)
	{
		while (true)
		{
            // Calculate indexes of the children.
			int childIndexLeft = item.HeapIndex * 2 + 1;
			int childIndexRight = item.HeapIndex * 2 + 2;
			int swapIndex = 0;

			if (childIndexLeft < currentItemCount)
			{
				swapIndex = childIndexLeft;

				if (childIndexRight < currentItemCount)
				{
                    // In case both children have lower indexes, compare items to chose one with lower index.
					if (items[childIndexLeft].CompareTo(items[childIndexRight]) < 0)
					{
						swapIndex = childIndexRight;
					}
				}

                // Swap if parent has lower priority than it's chosen child.
				if (item.CompareTo(items[swapIndex]) < 0)
				{
					Swap(item, items[swapIndex]);
				}
				else
				{	// Parent has higher priority than it's children - correct position;
					return;
				}
			}
			else
			{   // Parent doesn't have any children - correct position.
				return;
			}
		}
	}

    /// <summary>
    /// Function to sort items up in hierarchy.
    /// </summary>
	void SortUp(T item)
	{
        // Calculate parent index.
		int parentIndex = (item.HeapIndex - 1) / 2;

		while(true)
		{
			T parentItem = items[parentIndex];

            // Swap with parent if item has lower cost, in case of nodes, than its parent.
			if (item.CompareTo(parentItem) > 0)
				Swap(item, parentItem);
			else
				break;

			parentIndex = (item.HeapIndex - 1) / 2;
		}
	}

    /// <summary>
    /// Function to swap two items and their indexes.
    /// </summary>
	void Swap(T itemA, T itemB)
	{
		items[itemA.HeapIndex] = itemB;
		items[itemB.HeapIndex] = itemA;

		int tempIndex = itemA.HeapIndex;
		itemA.HeapIndex = itemB.HeapIndex;
		itemB.HeapIndex = tempIndex;
	}
}

public interface IHeapItem<T> : IComparable<T>
{
	int HeapIndex
	{
		get; set;
	}
}
