using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace Klario.DAL.Extensions;

public static class DbUpdateExceptionExtensions
{
    public static bool IsUniqueConstraintViolation(this DbUpdateException ex) =>
        ex.InnerException is SqlException sqlEx && (sqlEx.Number == 2601 || sqlEx.Number == 2627);
}
