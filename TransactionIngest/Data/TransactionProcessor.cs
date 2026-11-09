using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using TransactionIngest.Models;

namespace TransactionIngest.Data
{
    class TransactionProcessor : IDataAPI
    {
        // use this list to import all the data at once via for loop in SaveToDB
        List<String> fileNames = new List<string> { "hour_1_sample.json", "hour_2_sample.json", "hour_3_sample.json", "hour_4_sample.json", "hour_5_sample.json","hour_6_sample.json","hour_7_sample.json","hour_8_sample.json","hour_9_sample.json","hour_10_sample.json","hour_11_sample.json","hour_12_sample.json","hour_13_sample.json","hour_14_sample.json","hour_15_sample.json","hour_16_sample.json","hour_17_sample.json","hour_18_sample.json","hour_19_sample.json","hour_20_sample.json","hour_21_sample.json","hour_22_sample.json","hour_23_sample.json","hour_24_sample.json","hour_25_sample.json","hour_26_sample.json","hour_27_sample.json","hour_28_sample.json"};
        static string logSeparator = "--------------------";

        public static List<Transaction> GetTransactions(string fileName)
        {
            // TODO check to make sure fileName is populated
            // TODO check to make sure filePath exists

            Console.WriteLine(logSeparator);
            Console.WriteLine("GetTransactions start at " + DateTime.Now);
            Console.WriteLine(logSeparator);

            List<Transaction> transactions = new List<Transaction>();
            string filePath = "Data\\hourly sample files\\" + fileName;

            using (StreamReader r = new StreamReader(filePath))
            {
                string json = r.ReadToEnd();
                transactions = JsonConvert.DeserializeObject<List<Transaction>>(json);
            }
            
            Console.WriteLine(logSeparator);
            Console.WriteLine("GetTransactions end at " + DateTime.Now);
            Console.WriteLine(logSeparator);
            return transactions;
        }

        public static List<Transaction> PostTransactions(TransactionContext context)
        {
            Console.WriteLine(logSeparator);
            Console.WriteLine("PostTransactions start at " + DateTime.Now);
            Console.WriteLine(logSeparator);

            var builder = WebApplication.CreateBuilder();
            List<Transaction> returnList = new List<Transaction>();
            int displayLimit = 0;

            // TODO add error handling for if RecordDisplayLimit is not in config file
            displayLimit = builder.Configuration.GetValue<int>("RecordDisplayLimit");
            // if displayLimit is larger than number of items in list, Take just grabs all items in list; if displayLimit is negative, Take grabs 0 items in list
            returnList = (List<Transaction>)context.Transactions.ToList().Take(displayLimit).ToList(); // throws error if not cast to list again

            Console.WriteLine(logSeparator);
            Console.WriteLine("PostTransactions end at " + DateTime.Now);
            Console.WriteLine(logSeparator);

            return returnList;
        }

        public static void parseImport(TransactionContext context, List<Transaction> hourlyTransactions)
        {
            Console.WriteLine(logSeparator);
            Console.WriteLine("parseImport start at " + DateTime.Now);
            Console.WriteLine(logSeparator);

            List<Transaction> insertTransactions = new List<Transaction>();
            List<Transaction> updateTransactions = new List<Transaction>();

            // most transactions in hourlyTransactions are going to be inserts
            // however, need to check each incoming transaction to see if its primary key matches an existing transaction within the last 24 hours (1 day)
            //      want to update matching transactions & insert the rest

            foreach (Transaction transaction in hourlyTransactions){
                // modelBuilder.Entity<Transaction>().ToTable("Transaction").HasKey(c => new { c.SourceSystem, c.TransactionId });
                DateTime startRange = transaction.TransactionTime.AddDays(-1); // works great
                string uniqueID = transaction.SourceSystem + transaction.TransactionId.ToString();

                // find any transactions within the last 24 hours which need to be updated (have matching primary key)
                updateTransactions = context.Transactions.Where(transaction => (transaction.TransactionTime >= startRange && (transaction.SourceSystem + transaction.TransactionId.ToString() == uniqueID))).ToList();
                //  TODO consider adding RevisionNumber to data model to indicate how many times a transaction has been updated

                if (updateTransactions.Count == 0) {
                    insertTransactions.Add(transaction); // works great
                }
                else
                {
                    // expecting to see ID 100001 from Store Uptown - buying an H Phone instead of a flip phone
                    //  yep!
                    foreach (Transaction trans in updateTransactions) // should be just one if matching primary key
                    {

                        // TODO handle update

                        //Console.WriteLine(transaction.CardNumber);
                        //Console.WriteLine(transaction.LocationCode);
                        //Console.WriteLine(transaction.ProductName);
                        //Console.WriteLine(transaction.Amount);
                        //Console.WriteLine(transaction.TransactionTime);

                        //Console.WriteLine(logSeparator);
                        //Console.WriteLine(trans.SourceSystem + ", " + trans.TransactionId);
                        //Console.WriteLine(logSeparator);
                    }
                }

                // TODO update sample data to include updated transactions
                //      first transaction might have incorrect product name & amount, for example - user misclicked, perhaps
                // should be
                //      20% updates
                //      10% revokes
                //          include revoke to insert & revoke to prev. update
                //      70% inserts
            }

            // after processing & editing insertTransactions, set to insert into DB with AddRange
            context.Transactions.AddRange(insertTransactions);

            // works great!
            /*
            foreach (Transaction transaction in hourlyTransactions){
                Console.WriteLine(transaction.SourceSystem);
                Console.WriteLine(transaction.TransactionId);
                Console.WriteLine(transaction.CardNumber);
                Console.WriteLine(transaction.LocationCode);
                Console.WriteLine(transaction.ProductName);
                Console.WriteLine(transaction.Amount);
                Console.WriteLine(transaction.TransactionTime);
                Console.WriteLine("--------------------");
            }
            */

            Console.WriteLine(logSeparator);
            Console.WriteLine("parseImport end at " + DateTime.Now);
            Console.WriteLine(logSeparator);
        }

        public static void SaveToDB(TransactionContext context)
        {
            Console.WriteLine(logSeparator);
            Console.WriteLine("SaveToDB start at " + DateTime.Now);
            Console.WriteLine(logSeparator);

            List<Transaction> hourlyTransactions = GetTransactions("hour_2_sample.json"); // hardcoding to test
            
            parseImport(context, hourlyTransactions);

            //context.Transactions.AddRange(hourlyTransactions);
            context.SaveChanges(); // insert into DB

            Console.WriteLine(logSeparator);
            Console.WriteLine("SaveToDB end at " + DateTime.Now);
            Console.WriteLine(logSeparator);
        }
    }
}