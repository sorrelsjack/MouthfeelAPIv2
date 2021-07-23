using Microsoft.EntityFrameworkCore;
using MouthfeelAPIv2.DbModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MouthfeelAPIv2
{
    public interface IMouthfeelContextFactory
    {
        public MouthfeelContext CreateContext();
    }

    public class MouthfeelContextFactory : IMouthfeelContextFactory
    {
        private static string _connectionString { get; set; }

        public MouthfeelContextFactory(string connectionString)
        {
            _connectionString = connectionString;
        }

        public MouthfeelContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<MouthfeelContext>();
            options.UseSqlServer(_connectionString);
            return new MouthfeelContext(options.Options);
        }
    }
}
