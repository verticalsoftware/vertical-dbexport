using System.Collections.Concurrent;
using Vertical.DbExport.Models;

namespace Vertical.DbExport.Data;

internal static class ArrayAdapters
{
    private interface IArrayBuilder
    {
        Array BuildArray(IReadOnlyList<Record> records, string field);

        Array BuildArray(IReadOnlyList<object> source);
    }

    private sealed class ArrayBuilder<T> : IArrayBuilder
    {
        /// <inheritdoc />
        public Array BuildArray(IReadOnlyList<Record> records, string field)
        {
            return records.Select(record => record.GetValue<T>(field)).ToArray();
        }

        /// <inheritdoc />
        public Array BuildArray(IReadOnlyList<object> source)
        {
            return source.Cast<T>().ToArray();
        }
    }

    private static readonly ConcurrentDictionary<Type, IArrayBuilder> ArrayBuilders = new();

    public static Array BuildArray(Type type, IReadOnlyList<Record> records, string field)
    {
        return GetArrayBuilder(type).BuildArray(records, field);
    }

    public static Array BuildArray(Type type, IReadOnlyList<object> source)
    {
        return GetArrayBuilder(type).BuildArray(source);
    }

    private static IArrayBuilder GetArrayBuilder(Type type)
    {
        return ArrayBuilders.GetOrAdd(type, newType =>
        {
            var builderType = typeof(ArrayBuilder<>).MakeGenericType(newType);
            return (IArrayBuilder)Activator.CreateInstance(builderType)!;
        });
    }
}