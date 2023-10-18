using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Vertical.DbExport.Options;

namespace Vertical.DbExport.Extensions;

public static class HashExtensions
{
    /// <summary>
    /// Computes a SHA hash code.
    /// </summary>
    /// <param name="obj">Object to hash</param>
    /// <returns>Hash code</returns>
    public static string Sha<T>(this T obj)
    {
        var bytesToHash = obj is string str
            ? Encoding.UTF8.GetBytes(str)
            : JsonSerializer.SerializeToUtf8Bytes(obj);
        
        return Convert.ToHexString(SHA1.HashData(bytesToHash)).ToLower();
    }

    /// <summary>
    /// Computes the sha of a job for a restore point.
    /// </summary>
    /// <param name="job">Job</param>
    /// <returns>Hash code</returns>
    public static string Sha(this JobOptions job)
    {
        return new
        {
            job.Name,
            job.DataSource,
            job.Output,
            Parallelization = new
            {
                job.Parallelization.PartitionSize,
                job.Parallelization.QueryBatchSize
            }
        }.Sha();
    }
}