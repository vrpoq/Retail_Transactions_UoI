using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TransactionIngest.Models;

namespace TransactionIngest.Data
{
    public class TransactionContext : DbContext
    {
        public TransactionContext (DbContextOptions<TransactionContext> options)
            : base(options)
        {
        }

        public DbSet<Transaction> Transactions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Transaction>().ToTable("Transaction");
        }
    }
}
