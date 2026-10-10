using System;
using System.Collections.Generic;
using System.Text;

namespace MealPlanner.Planning.DataStructures;

// Class for the indexed binary heap. Is used in the Dijkstra's algorithm 
public sealed class IndexedBinaryHeap<TPriority>
    where TPriority : IComparable<TPriority>
{
    // Represents the absene of the indentifier in the heap
#pragma warning disable IDE1006 // Naming Styles
    private const int NotInHeap = -1;
#pragma warning restore IDE1006 // Naming Styles

    // The amount of children of the node. 2 - for the standard binary heap
    private readonly int _arity;

    // The heap itself in the structure right now (id). Size - number of dishes
    private readonly int[] _heap;

    // the position in the tree. if not in it -1, if the top node - 0, then 1, 2...
    private readonly int[] _position;

    // Priority of the node
    private readonly TPriority?[] _priority;

    // Constructor of the indexed binary heap
    public IndexedBinaryHeap(int capacity, int arity = 2)
    {
        // Range cannot be negative or 0
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(capacity);

        // Less than 2 children, not a binary heap anymore
        ArgumentOutOfRangeException.ThrowIfLessThan(arity, 2);


        Capacity = capacity;
        _arity = arity;
        _heap = new int[capacity];
        _position = new int[capacity];
        _priority = new TPriority?[capacity];

        // Nothing in the heap so far, every dish is "NotInHeap"
        Array.Fill(_position, NotInHeap);
    }

    // Maximum id plus 1
    public int Capacity { get; }

    // How many elements in the heap now
    public int Count { get; private set; }

    // Is the heap empty
    public bool IsEmpty => Count == 0;

    // Current priority of the element
    public TPriority PriorityOf(int id)
    {
        ValidateId(id);

        if (_position[id] == NotInHeap)
        {
            // Not in the heap now - does not have a priority
            throw new InvalidOperationException($"Identifier {id} is not in the heap.");
        }

        // Priority is by the id of the dish, not by the place in the tree
        return _priority[id]!;
    }

    // Validation of id
    private void ValidateId(int id)
    {
        // Id cannot be negative
        ArgumentOutOfRangeException.ThrowIfNegative(id);

        // Id cannot be bigger or equal to the capacity
        // (capacity is bigger than needed by 1, since it begins with 0)
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(id, Capacity);
    }

    // Throw an exception if the heap is empty
    private void ThrowIfEmpty()
    {
        if (Count == 0)
        {
            throw new InvalidOperationException("The heap is empty.");
        }
    }

    // Does the heap contain the id
    public bool Contains(int id)
    {
        ValidateId(id);
        return _position[id] != NotInHeap;
    }

    // Adds new element to the heap
    public void Push(int id, TPriority priority)
    {
        ValidateId(id);

        if (_position[id] != NotInHeap)
        {
            // Throws an exception if the id is already in the heap
            throw new InvalidOperationException(
                $"Identifier {id} is already in the heap. Use DecreaseKey to change its priority.");
        }

        // The new element is added to the end of the array
        _heap[Count] = id;
        _position[id] = Count;
        _priority[id] = priority;
        Count++;

        // If the new element is less than its parent, we need to shift it up
        ShiftUp(Count - 1);
    }

    // Get id with the minimum priority without popping it
    public int PeekMin()
    {
        ThrowIfEmpty();
        return _heap[0];
    }

    // Get the minimum priority in the heap
    public TPriority PeekMinPriority()
    {
        ThrowIfEmpty();

        return _priority[_heap[0]]!;
    }

    // Get the id with minimum priority
    public int PopMin()
    {
        ThrowIfEmpty();

        // The node with minimum priority is the root
        int min = _heap[0];
        Count--;

        if (Count > 0)
        {
            // Put the last node in the empty before root
            int last = _heap[Count];
            _heap[0] = last;
            _position[last] = 0;

            // Then shift in down until the tree it is placed according to its priority
            ShiftDown(0);
        }

        // Now the popped node is not in the hep
        _position[min] = NotInHeap;

        // Returns the minimum priority node
        return min;
    }


    // Shifts the element up if priority is less than its parent
    private void ShiftUp(int index)
    {
        // Remember, what are we shifting up. 
        int id = _heap[index];
        TPriority priority = _priority[id]!;

        while (index > 0)
        {
            // The parent of the node in heap is (index - 1) / arity, in binary heap (index - 1) / 2
            int parent = (index - 1) / _arity;
            int parentId = _heap[parent];

            // The parent is already correctly placed by priority, do not change anything anymore
            if (_priority[parentId]!.CompareTo(priority) <= 0)
            {
                break;
            }

            // Moving the parent down 
            _heap[index] = parentId;
            _position[parentId] = index;
            index = parent;
        }

        // Moving the element up
        _heap[index] = id;
        _position[id] = index;
    }

    // Shifts the element down if priority is bigger than its child
    private void ShiftDown(int index)
    {
        int id = _heap[index];
        TPriority priority = _priority[id]!;

        while (true)
        {
            // The first child of the node. Next children are one after each other up to arity - 1
            int firstChild = (index * _arity) + 1;

            // There are no children - its a leaf
            if (firstChild >= Count)
            {
                break;
            }

            // Take the cheapest one out of existing (not more than arity and not farer than the end of the heap)
            int best = firstChild;
            int lastChild = Math.Min(firstChild + _arity, Count);

            for (int c = firstChild + 1; c < lastChild; c++)
            {
                if (_priority[_heap[c]]!.CompareTo(_priority[_heap[best]]!) < 0)
                {
                    best = c;
                }
            }

            // If the cheaper child not better than the parent we are shifting, than everything is in its place
            if (_priority[_heap[best]]!.CompareTo(priority) >= 0)
            {
                break;
            }

            // Child is cheaper. We move it up and parent takes its place
            int bestId = _heap[best];
            _heap[index] = bestId;
            _position[bestId] = index;
            index = best;
        }

        _heap[index] = id;
        _position[id] = index;
    }

    // Decrease Key operation for decreasing priority. Crucial for the Dijkstra's algorithm
    public void DecreaseKey(int id, TPriority newPriority)
    {
        ValidateId(id);
        // Where the dish is in the tree, do not need to search the tree
        int index = _position[id];

        // Id is not in the heap -nothing to decrease key for
        if (index == NotInHeap)
        {
            throw new InvalidOperationException($"Identifier {id} is not in the heap.");
        }

        // Cannot shift anything down the tree (increase key),
        // but the new priority is not less than the previous one
        if (newPriority.CompareTo(_priority[id]!) >= 0)
        {
            throw new InvalidOperationException(
                "The new priority is not less than the current one. A min-heap cannot increase a key.");
        }

        // The elements gets the new priority and can now have a
        // smaller one than its parents - needs to be shifted up
        _priority[id] = newPriority;
        ShiftUp(index);
    }

    // Push a new element in the heap or decrease its priority. Useful for Dijkstra
    public bool PushOrDecrease(int id, TPriority priority)
    {
        ValidateId(id);

        // The element is not in the keap, just pushing it there
        if (_position[id] == NotInHeap)
        {
            Push(id, priority);

            return true;
        }

        // The element is already in the heap and the
        // new priority is not better than the old one
        if (priority.CompareTo(_priority[id]!) >= 0)
        {
            return false;
        }

        // The element is in the heap, the new priority is better than
        // the old one - use DecreaseKey and not put a duplicate
        DecreaseKey(id, priority);

        return true;
    }

    // Clearing the heap of elements with keeping the allocated memory
    public void Clear()
    {
        // Work only with elements that are in the heap right now
        for (int i = 0; i < Count; i++)
        {
            _position[_heap[i]] = NotInHeap;
        }

        Count = 0;
    }

    // For future testing. Checks whether the heap is valid.
    // Priority of the parent cannot be more than its children's
    internal bool IsHeapValid()
    {
        for (int i = 0; i < Count; i++)
        {
            // Verify that the position of the element and the element in this position are consistent
            if (_position[_heap[i]] != i)
            {
                return false;
            }

            int firstChild = (i * _arity) + 1;

            for (int c = firstChild; c < firstChild + _arity && c < Count; c++)
            {
                // The parent node has bigger priority than its child's - heap is not valid
                if (_priority[_heap[i]]!.CompareTo(_priority[_heap[c]]!) > 0)
                {
                    return false;
                }
            }
        }

        return true;
    }

}
