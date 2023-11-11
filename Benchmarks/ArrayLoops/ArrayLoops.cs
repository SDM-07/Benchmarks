using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;

namespace Benchmarks.ArrayLoops;

[SimpleJob(RuntimeMoniker.Net60)]
[SimpleJob(RuntimeMoniker.Net70)]
public class ArrayLoops
{
    private readonly Random _random = new(420);
    private int[] _array;

    [Params(100, 10_000, 100_000, 1_000_000)]
    public int Size;

    [GlobalSetup]
    public void Setup()
    {
        _array = new int[Size];

        for (var i = 0; i < Size; i++)
            _array[i] = _random.Next(1, 10);
    }

    [Benchmark(Baseline = true)]
    public long For()
    {
        var sum = 0L;

        for (var i = 0; i < _array.Length; i++)
            sum += _array[i];

        return sum;
    }

    [Benchmark]
    public long Foreach()
    {
        var sum = 0L;

        foreach (var item in _array)
            sum += item;

        return sum;
    }

    [Benchmark]
    public long ForSpan()
    {
        var sum = 0L;
        var span = _array.AsSpan();

        for (var i = 0; i < span.Length; i++)
            sum += span[i];

        return sum;
    }

    [Benchmark]
    public long ForeachSpan()
    {
        var sum = 0L;
        var span = _array.AsSpan();

        foreach (var item in span)
            sum += item;

        return sum;
    }

    [Benchmark]
    public long UnsafeFor()
    {
        var sum = 0L;
        ref var item = ref MemoryMarshal.GetArrayDataReference(_array);

        for (var i = 0; i < _array.Length; i++)
        {
            sum += item;
            item = ref Unsafe.Add(ref item, 1);
        }

        return sum;
    }

    [Benchmark]
    public long UnsafeWhile()
    {
        var sum = 0L;
        ref var item = ref MemoryMarshal.GetArrayDataReference(_array);
        ref var end = ref Unsafe.Add(ref item, _array.Length);

        while (Unsafe.IsAddressLessThan(ref item, ref end))
        {
            sum += item;
            item = ref Unsafe.Add(ref item, 1);
        }

        return sum;
    }
}