using Parquet;
using Parquet.Data;
using Parquet.Schema;
using Vertical.DbExport.Data;
using Vertical.DbExport.Models;
using Vertical.DbExport.Services;

namespace Vertical.DbExport.IO;

public class ParquetMergeStream : IMergeStream
{
    private readonly List<List<object>> _buffer;
    private readonly IReadOnlyList<DataField> _dataFields;


    public ParquetMergeStream(
        IReadOnlyList<ColumnSchema> columnSchemata, 
        long capacity)
    {
        _buffer = new List<List<object>>(columnSchemata.Select(_ => new List<object>((int)capacity)));
        _dataFields = ParquetDataChannel.BuildSchemaFields(columnSchemata).ToArray();
    }

    /// <inheritdoc />
    public async Task AppendAsync(
        Stream stream,
        long startIndex,
        long count,
        CancellationToken cancellationToken)
    {
        using var parquetReader = await ParquetReader.CreateAsync(stream, cancellationToken: cancellationToken);
        var dataFields = parquetReader.Schema.DataFields;
        var rowReader = parquetReader.OpenRowGroupReader(0);

        for (var c = 0; c < dataFields.Length; c++)
        {
            var dataField = dataFields[c];
            var result = await rowReader.ReadColumnAsync(dataField, cancellationToken: cancellationToken);
            var array = result.Data;
            var bufferElement = _buffer[c];
            bufferElement.AddRange(array
                .Cast<object>()
                .Skip((int)startIndex)
                .Take((int)count)
            );
        }
    }

    /// <inheritdoc />
    public async Task<FileDescriptor> FlushAsync(IFileSystem fileSystem, string path, CancellationToken cancellationToken)
    {
        return await fileSystem.WriteFileAsync(
            $"{path}.parquet",
            async stream => await FlushAsync(stream, cancellationToken));
    }

    private async Task FlushAsync(Stream stream, CancellationToken cancellationToken)
    {
        var schema = new ParquetSchema(_dataFields);
        using var parquetWriter = await ParquetWriter.CreateAsync(schema, stream, cancellationToken: cancellationToken);
        using var groupWriter = parquetWriter.CreateRowGroup();

        for (var c = 0; c < _dataFields.Count; c++)
        {
            var field = _dataFields[c];
            var data = _buffer[c];
            var castedTypeArray = ArrayAdapters.BuildArray(_dataFields[c].ClrType, data);
            var column = new DataColumn(field, castedTypeArray);
            await groupWriter.WriteColumnAsync(column, cancellationToken);
        }
    }
}