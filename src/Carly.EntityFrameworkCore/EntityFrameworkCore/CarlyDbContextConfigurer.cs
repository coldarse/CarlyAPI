using System;
using System.Data.Common;
using Microsoft.EntityFrameworkCore;

namespace Carly.EntityFrameworkCore
{
    public static class CarlyDbContextConfigurer
    {
        public static void Configure(DbContextOptionsBuilder<CarlyDbContext> builder, string connectionString)
        {
            var serverVersion = new MySqlServerVersion(new Version(8, 0, 21));
            builder.UseMySql(connectionString, serverVersion);
        }

        public static void Configure(DbContextOptionsBuilder<CarlyDbContext> builder, DbConnection connection)
        {
            var serverVersion = new MySqlServerVersion(new Version(8, 0, 21));
            builder.UseMySql(connection, serverVersion);
        }
    }
}
