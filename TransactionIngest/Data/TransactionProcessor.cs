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
            returnList = context.Transactions.ToList().Take(displayLimit).OrderBy(transac => transac.TransactionId).ToList(); // throws error if not cast to list again

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

            // update matching transactions & insert the rest
            foreach (Transaction transaction in hourlyTransactions){
                DateTime startRange = transaction.TransactionTime.AddDays(-1);
                string uniqueID = transaction.SourceSystem + transaction.TransactionId.ToString();

                // find any transaction within the last 24 hours which need to be updated (has matching primary key)
                Transaction transToUpdate = context.Transactions.FirstOrDefault(transaction => transaction.TransactionTime >= startRange && (transaction.SourceSystem + transaction.TransactionId.ToString() == uniqueID));

                if (transToUpdate is null) {

                    Console.WriteLine("Inserting transaction " + transaction.SourceSystem + ", " + transaction.TransactionId);

                    transaction.AuditHistory = "insert";
                    transaction.RevisionNumber = 0;
                    insertTransactions.Add(transaction);
                }
                else // TODO add error handling - https://learn.microsoft.com/en-us/aspnet/core/data/ef-mvc/update-related-data?view=aspnetcore-10.0
                {
                    // make sure values are different; capture details from incoming record
                    string newCardNum = transaction.CardNumber;
                    string newLocCode = transaction.LocationCode;
                    string newProdName = transaction.ProductName;
                    decimal newAmount = transaction.Amount;
                    DateTime newTransacTime = transaction.TransactionTime;
                    // for revision number, want existing transaction's count
                    int newRevision = transToUpdate.RevisionNumber; 

                    if (transToUpdate.CardNumber == newCardNum && transToUpdate.LocationCode == newLocCode && transToUpdate.ProductName == newProdName && transToUpdate.Amount == newAmount && transToUpdate.TransactionTime == newTransacTime)
                    {
                        // perhaps repeat transaction included by mistake, or re-importing a file
                        Console.WriteLine("Identical transaction for " + transToUpdate.SourceSystem + ", " + transToUpdate.TransactionId);
                    }
                    else // details have changed; remove and re-insert
                    {
                        Console.WriteLine("Updating transaction for " + transToUpdate.SourceSystem + ", " + transToUpdate.TransactionId);

                        context.Transactions.Remove(transToUpdate);
                        transaction.AuditHistory = "updated";
                        transaction.RevisionNumber = newRevision + 1;
                        insertTransactions.Add(transaction);
                    }
                }
            }

            // after processing, set to insert into DB with AddRange
            context.Transactions.AddRange(insertTransactions);

            Console.WriteLine(logSeparator);
            Console.WriteLine("parseImport end at " + DateTime.Now);
            Console.WriteLine(logSeparator);
        }

        public static void SaveToDB(TransactionContext context)
        {
            Console.WriteLine(logSeparator);
            Console.WriteLine("SaveToDB start at " + DateTime.Now);
            Console.WriteLine(logSeparator);

            List<Transaction> hourlyTransactions = GetTransactions("hour_3_sample.json"); // hardcoding to test
            parseImport(context, hourlyTransactions);
            context.SaveChanges(); // save insert into DB

            Console.WriteLine(logSeparator);
            Console.WriteLine("SaveToDB end at " + DateTime.Now);
            Console.WriteLine(logSeparator);
        }
    }
}