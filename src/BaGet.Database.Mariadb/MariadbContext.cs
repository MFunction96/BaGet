using BaGet.Core;
using BaGet.Core.Entities;
using Microsoft.EntityFrameworkCore;
using MySqlConnector;

namespace BaGet.Database.Mariadb
{
    public class MariadbContext(DbContextOptions<BaGetDbContext> options) : BaGetDbContext(options)
    {
        /// <summary>
        /// The MySQL Server error code for when a unique constraint is violated.
        /// </summary>
        private const int UniqueConstraintViolationErrorCode = 1062;

        public override bool IsUniqueConstraintViolationException(DbUpdateException exception)
        {
            return exception.InnerException is MySqlException mysqlException &&
                   mysqlException.Number == UniqueConstraintViolationErrorCode;
        }

        /// <summary>
        /// MySQL does not support LIMIT clauses in subqueries for certain subquery operators.
        /// See: https://dev.mysql.com/doc/refman/8.0/en/subquery-restrictions.html
        /// </summary>
        public override bool SupportsLimitInSubqueries => false;
    }
}
