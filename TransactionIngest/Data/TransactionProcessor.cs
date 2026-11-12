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
        static List<string> batchIDs = new List<string>();
        static DateTime earliestDate = DateTime.MaxValue;
        static DateTime latestDate = DateTime.MinValue;

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

        public static Transaction markInsert(Transaction currTransaction)
        {
            Console.WriteLine("Inserting transaction " + currTransaction.SourceSystem + ", " + currTransaction.TransactionId);
            currTransaction.FullDayRange = currTransaction.TransactionTime.AddDays(1);
            currTransaction.AuditHistory = "insert";
            currTransaction.RevisionNumber = 0;

            return currTransaction;
        }

        public static List<Transaction> markInsertUpdate(TransactionContext context, List<Transaction> hourlyTransactions)
        {
            List<Transaction> insertUpdateTransactions = new List<Transaction>();
            Transaction insertedTransaction = new Transaction();
            Transaction updatedTransaction = new Transaction();

            // update matching transactions & insert the rest
            foreach (Transaction currTransaction in hourlyTransactions){
                DateTime startRange = currTransaction.TransactionTime.AddDays(-1);
                string uniqueID = currTransaction.SourceSystem + currTransaction.TransactionId.ToString();

                // determine range of hourly import
                if (currTransaction.TransactionTime < earliestDate){earliestDate = currTransaction.TransactionTime;}
                if (currTransaction.TransactionTime > latestDate){latestDate = currTransaction.TransactionTime;}
                batchIDs.Add(uniqueID);

                // find any transaction within the last 24 hours which need to be updated (has matching primary key)
                Transaction transToUpdate = context.Transactions.FirstOrDefault(trans => trans.TransactionTime >= startRange && (trans.SourceSystem + trans.TransactionId.ToString() == uniqueID));

                if (transToUpdate is null) { // no matching transaction detected; must be a new record
                    insertedTransaction = markInsert(currTransaction);
                    insertUpdateTransactions.Add(insertedTransaction);
                }
                else // matching transaction detected; might be an updateable record
                {
                    updatedTransaction = markUpdated(context, currTransaction, transToUpdate);
                    if (updatedTransaction != null){
                        insertUpdateTransactions.Add(updatedTransaction);
                    }
                    
                }
            }
            return insertUpdateTransactions;
        }

        // TODO add error handling - https://learn.microsoft.com/en-us/aspnet/core/data/ef-mvc/update-related-data?view=aspnetcore-10.0
        public static Transaction markUpdated(TransactionContext context, Transaction currTransaction, Transaction transToUpdate)
        {
            // make sure values are different; capture details from incoming record
            string newCardNum = currTransaction.CardNumber;
            string newLocCode = currTransaction.LocationCode;
            string newProdName = currTransaction.ProductName;
            decimal newAmount = currTransaction.Amount;
            DateTime newTransacTime = currTransaction.TransactionTime;
            // for revision number, want existing transaction's count
            int newRevision = transToUpdate.RevisionNumber; 

            if (transToUpdate.CardNumber == newCardNum && transToUpdate.LocationCode == newLocCode && transToUpdate.ProductName == newProdName && transToUpdate.Amount == newAmount && transToUpdate.TransactionTime == newTransacTime)
            {
                // perhaps repeat transaction included by mistake, or re-importing a file
                Console.WriteLine("Identical transaction for " + transToUpdate.SourceSystem + ", " + transToUpdate.TransactionId);
                return null; // don't want to re-add if identical
            }
            else // details have changed; remove and re-insert
            {
                Console.WriteLine("Updating transaction for " + transToUpdate.SourceSystem + ", " + transToUpdate.TransactionId);

                context.Transactions.Remove(transToUpdate);
                currTransaction.AuditHistory = "updated";
                currTransaction.RevisionNumber = newRevision + 1;
                return currTransaction; // only want to re-add if updated
            }
        }

        public static List<Transaction> identifyRevoked(TransactionContext context)
        {
            List<Transaction> revokeTransactions = new List<Transaction>();

            // want to remove original version & insert updated version
            foreach (Transaction origTransac in context.Transactions){
                bool inRange = false;
                string uniqueID = origTransac.SourceSystem + origTransac.TransactionId.ToString(); 

                // no need to check anything else if transaction has already been revoked
                if (origTransac.AuditHistory == "revoked")
                {
                    continue;
                }

                // find out why the update isn't getting revoked
                if (uniqueID == "Store Uptown100018")
                {
                    Console.WriteLine("DEBUG: earliestDate " + earliestDate); // 2/27/2026 8:00:30 AM
                    Console.WriteLine("DEBUG: origTransac.TransactionTime " + origTransac.TransactionTime); // 2/27/2026 9:42:30 AM
                    Console.WriteLine("DEBUG: latestDate "+ latestDate); // 2/27/2026 11:54:30 AM
                    Console.WriteLine("DEBUG: origTransac.FullDayRange " + origTransac.FullDayRange); // 1/1/0001 12:00:00 AM
                }

                // if any batch during 24 hour range does not include ID, revoke
                //      need to capture overlap where batch range includes start date & where batch range includes end date
                // TransactionTime indicates start of 24H range, FullDayRange indicates end of 24H range
                if ((earliestDate <= origTransac.TransactionTime && latestDate <= origTransac.FullDayRange) || (earliestDate >= origTransac.TransactionTime && latestDate >= origTransac.FullDayRange))
                {
                    inRange = true;
                }

                if (uniqueID == "Store Uptown100018")
                {
                    Console.WriteLine("DEBUG: inRange " + inRange); // False
                    Console.WriteLine("DEBUG: batchIDs.Contains(uniqueID) " + batchIDs.Contains(uniqueID)); // False
                }

                // separating out for readability & maintenance
                if (inRange && !batchIDs.Contains(uniqueID))
                {
                    // can't remove here, likely because we're still referencing it by being in its iteration of the loop
                    revokeTransactions.Add(origTransac);
                }
            }
            return revokeTransactions;
        }

        public static List<Transaction> markRevoked(TransactionContext context, List<Transaction> identifiedRevoked)
        {
            List<Transaction> revokedTransactions = new List<Transaction>();

            foreach (Transaction toRevoke in identifiedRevoked)
            {
                Transaction currTransaction = Transaction.DeepCopy(toRevoke);
                currTransaction.AuditHistory = "revoked";
                currTransaction.RevisionNumber ++;
                Console.WriteLine("Revoking transaction for " + currTransaction.SourceSystem + ", " + currTransaction.TransactionId);
                context.Remove(toRevoke);
                revokedTransactions.Add(currTransaction);
            }
            return revokedTransactions;
        }

        // TODO create a way to manually import data - file browse with button?
        // TODO split up parseImport to separate things out
        public static void parseImport(TransactionContext context, List<Transaction> hourlyTransactions)
        {
            Console.WriteLine(logSeparator);
            Console.WriteLine("parseImport start at " + DateTime.Now);
            Console.WriteLine(logSeparator);

            List<Transaction> insertUpdateTransactions = new List<Transaction>();
            List<Transaction> identifiedRevoked = new List<Transaction>();
            List<Transaction> revokedTransactions = new List<Transaction>();
            
            batchIDs.Clear();
            insertUpdateTransactions = markInsertUpdate(context, hourlyTransactions);
            identifiedRevoked = identifyRevoked(context);
            revokedTransactions = markRevoked(context, identifiedRevoked);

            // after processing, set to insert all relevant records into DB with AddRange
            context.Transactions.AddRange(insertUpdateTransactions);
            context.Transactions.AddRange(revokedTransactions);

            Console.WriteLine(logSeparator);
            Console.WriteLine("parseImport end at " + DateTime.Now);
            Console.WriteLine(logSeparator);
        }

        public static void SaveToDB(TransactionContext context)
        {
            Console.WriteLine(logSeparator);
            Console.WriteLine("SaveToDB start at " + DateTime.Now);
            Console.WriteLine(logSeparator);

            List<Transaction> hourlyTransactions = GetTransactions("hour_4_sample.json"); // hardcoding to test
            parseImport(context, hourlyTransactions);
            context.SaveChanges(); // save insert into DB

            Console.WriteLine(logSeparator);
            Console.WriteLine("SaveToDB end at " + DateTime.Now);
            Console.WriteLine(logSeparator);
        }
    }
}