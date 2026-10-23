using MealPlanner.Planning.DataStructures;

namespace MealPlanner.Planning.Tests;

// Tests for IndexedBinaryHeap
public sealed class IndexedBinaryHeapTests
{
    // Same test with different arity values (2 and 4)
    [Theory]
    [InlineData(2)]
    [InlineData(4)]
    public void Random_operations_preserve_the_heap_and_pop_in_order(int arity)
    {
        // Id capasity (from 0 to 31)
        const int Capacity = 32;

        // Random number with fixed seed to reproduce the same failure if test fails
        var random = new Random(20261021);
        var heap = new IndexedBinaryHeap<int>(Capacity, arity);
        var priority = new int?[Capacity];

        // 400 operation with checks if the heap is still valid after each
        for (int step = 0; step < 400; step++)
        {
            // Randomly chooses an operation.
            // If the heap is empty the PopMinAndCheck is not done,
            // because only random.Next(3) can make roll = 2
            int roll = random.Next(heap.IsEmpty ? 2 : 3);

            // Tests pushing new item and PushOrDecrease
            if (roll == 0)
            {
                // Random id and priority
                int id = random.Next(Capacity);
                int value = random.Next(0, 500);

                // If this id is not in the heap, pushes
                // it (real heap + in priority array here)
                if (priority[id] is null)
                {
                    heap.Push(id, value);
                    priority[id] = value;
                }

                // If the existing id receives a better priority
                else if (value < priority[id])
                {
                    // Checks that method worked successfully 
                    Assert.True(heap.PushOrDecrease(id, value));
                    priority[id] = value;
                }
                else
                {
                    // If new priority is not better
                    // PushOrDecrease should'nt change anything
                    Assert.False(heap.PushOrDecrease(id, value));
                }
            }

            // Tests PushOrDecrease specifically
            else if (roll == 1)
            {
                // Creates random id
                int id = random.Next(Capacity);

                // Only if this id is present assigns
                // priority[id] value to current
                if (priority[id] is int current)
                {
                    // Creates a for sure better priority
                    int better = current - random.Next(1, 30);

                    // Checks that method worked successfully +
                    // Pushes the priority id to priority[id]
                    Assert.True(heap.PushOrDecrease(id, better));
                    priority[id] = better;
                }
            }

            // Checks that PopMin works correctly
            else
            {
                PopMinAndCheck(heap, priority);
            }

            // After every operation checks that the heap is still valid
            Assert.True(heap.IsHeapValid());
        }

        // Empties the heap
        while (!heap.IsEmpty)
        {
            // Checks that PopMin works correctly
            PopMinAndCheck(heap, priority);

            // Checks that the heap is still valid
            Assert.True(heap.IsHeapValid());
        }

        // Checks that every value in priority is null now
        Assert.All(priority, value => Assert.Null(value));
    }

    // Verifies PopMin work
    private static void PopMinAndCheck(IndexedBinaryHeap<int> heap, int?[] priority)
    {
        int best = int.MaxValue;

        foreach (int? value in priority)
        {
            // If the value is not null and smaller than
            // the current best it will be the new minimum
            if (value is int current && current < best)
            {
                best = current;
            }
        }

        // Pops the minimum value from the heap
        int id = heap.PopMin();

        // Checks that the minimum value popped from
        // the heap and minimum in priority are equal
        Assert.Equal(best, priority[id]);

        // Removes id from priority too
        priority[id] = null;
    }
}
