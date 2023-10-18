using System.Data;

namespace Vertical.DbExport.Data;

public delegate Task<T> ConnectionFactory<T>(CancellationToken cancellationToken)
    where T : IDbConnection;