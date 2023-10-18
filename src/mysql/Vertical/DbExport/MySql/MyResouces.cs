using Vertical.DbExport.Resources;

namespace Vertical.DbExport.MySql;

internal static class MyResouces
{
    internal const string ResourceFile = "queries.mysql.txt";

    internal static string ColumnSchemaQuery => StringResourceReader.GetResource(ResourceFile, "schema");

    internal static string SortedDataQuery => StringResourceReader.GetResource(ResourceFile, "sorted-query");
    
    internal static string UnsortedDataQuery => StringResourceReader.GetResource(ResourceFile, "unsorted-query");

}