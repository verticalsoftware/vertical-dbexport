using Parquet;
using Parquet.Schema;
using Vertical.DbExport.Data;
using Vertical.DbExport.Models;
using Vertical.DbExport.Services;

namespace Vertical.DbExport.IO;

public class ParquetDataChannel : IDataChannel
{
    private readonly IFileSystem _fileSystem;
    private readonly string _path;

    public ParquetDataChannel(IFileSystem fileSystem, string path)
    {
        _fileSystem = fileSystem;
        _path = $"{path}.parquet";
    }

    /// <inheritdoc />
    public string FileExtension => ".parquet";

    public async Task<FileDescriptor> WriteAsync(RecordSet set)
    {
        var dataFields = BuildSchemaFields(set.Columns).ToArray();
        var schema = new ParquetSchema(dataFields.Cast<Field>());
        var dataColumns = dataFields.Select(field => InsertColumnData(set, field));

        var descriptor = await _fileSystem.WriteFileAsync(_path, async stream =>
        {
            using var parquetWriter = await ParquetWriter.CreateAsync(schema, stream);
            using var groupWriter = parquetWriter.CreateRowGroup();

            foreach (var dataColumn in dataColumns)
            {
                await groupWriter.WriteColumnAsync(dataColumn);
            }
        });

        return descriptor;
    }

    /// <inheritdoc />
    public IMergeStream CreateMergeStream(IReadOnlyList<ColumnSchema> columnSchemata, long capacity)
    {
        return new ParquetMergeStream(columnSchemata, capacity);
    }

    private static Parquet.Data.DataColumn InsertColumnData(RecordSet set, DataField dataField)
    {
        var columnValues = ArrayAdapters.BuildArray(dataField.ClrType,
            set.Rows,
            dataField.Name);

        return new Parquet.Data.DataColumn(dataField, columnValues);
    }

    public static IEnumerable<DataField> BuildSchemaFields(IReadOnlyList<ColumnSchema> columnSchemata)
    {
        return columnSchemata.Select(column => new DataField(
            column.ColumnName,
            column.DataType,
            column.IsNullable));
    }
}