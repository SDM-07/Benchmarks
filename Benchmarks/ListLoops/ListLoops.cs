using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;

namespace Benchmarks.ListLoops;

[SimpleJob(RuntimeMoniker.Net60)]
[SimpleJob(RuntimeMoniker.Net70)]
public class ListLoops
{
    private readonly Random _random = new(420);
    private List<int> _list;

    [Params(100, 10_000, 100_000, 1_000_000)]
    public int Size;

    [GlobalSetup]
    public void Setup()
    {
        _list = new(Size);

        for (var i = 0; i < Size; i++)
            _list.Add(_random.Next(1, 10));
    }

    [Benchmark(Baseline = true)]
    public long For()
    {
        var sum = 0L;

        for (var i = 0; i < _list.Count; i++)
            sum += _list[i];

        return sum;
    }

    [Benchmark]
    public long Foreach()
    {
        var sum = 0L;

        foreach (var item in _list)
            sum += item;

        return sum;
    }

    [Benchmark]
    public long ForSpan()
    {
        var sum = 0L;
        var span = CollectionsMarshal.AsSpan(_list);

        for (var i = 0; i < span.Length; i++)
            sum += span[i];

        return sum;
    }

    [Benchmark]
    public long ForeachSpan()
    {
        var sum = 0L;
        var span = CollectionsMarshal.AsSpan(_list);

        foreach (var item in span)
            sum += item;

        return sum;
    }

    [Benchmark]
    public long UnsafeFor()
    {
        var sum = 0L;
        var span = CollectionsMarshal.AsSpan(_list);
        ref var item = ref MemoryMarshal.GetReference(span);

        for (var i = 0; i < _list.Count; i++)
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
        var span = CollectionsMarshal.AsSpan(_list);
        ref var item = ref MemoryMarshal.GetReference(span);
        ref var end = ref Unsafe.Add(ref item, _list.Count);

        while (Unsafe.IsAddressLessThan(ref item, ref end))
        {
            sum += item;
            item = ref Unsafe.Add(ref item, 1);
        }

        return sum;
    }
}