using TransactionIngest.Models;

namespace TransactionIngest.Data
{
    public static class DbInitializer
    {
        public static void Initialize(TransactionContext context)
        {
            // Look for any students.
            if (context.Transactions.Any())
            {
                return;   // DB has been seeded
            }

            var transactions = new Transaction[]
            {
                new Transaction{SourceSystem="Store Uptown",TransactionId=100001,CardNumber="***1111",LocationCode="UPTOWN",ProductName="Wireless Mouse",Amount=24.99m,TransactionTime=DateTime.Parse("2026-02-27T08:15:30Z")},
                new Transaction{SourceSystem="Store Downtown",TransactionId=100001,CardNumber="***0002",LocationCode="DOWNTOWN",ProductName="Mechanical Keyboard",Amount=89.99m,TransactionTime=DateTime.Parse("2026-02-27T09:45:12Z")},
                new Transaction{SourceSystem="Online",TransactionId=100001,CardNumber="***4444",LocationCode="ONLINE",ProductName="USB-C Docking Station",Amount=129.95m,TransactionTime=DateTime.Parse("2026-02-27T11:20:05Z")}
            };

            context.Transactions.AddRange(transactions);
            context.SaveChanges();
        }
    }
}