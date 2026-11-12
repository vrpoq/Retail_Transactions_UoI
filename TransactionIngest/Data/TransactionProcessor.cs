//using System.Transactions;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using TransactionIngest.Models;

namespace TransactionIngest.Data
{
    public class TransactionProcessor : IDataAPI
    {
        // can use this list to import all the data at once via for loop in SaveToDB
        List<String> fileNames = new List<string> { "hour_1_sample.json", "hour_2_sample.json", "hour_3_sample.json", "hour_4_sample.json", "hour_5_sample.json","hour_6_sample.json","hour_7_sample.json","hour_8_sample.json","hour_9_sample.json","hour_10_sample.json","hour_11_sample.json","hour_12_sample.json","hour_13_sample.json","hour_14_sample.json","hour_15_sample.json","hour_16_sample.json","hour_17_sample.json","hour_18_sample.json","hour_19_sample.json","hour_20_sample.json","hour_21_sample.json","hour_22_sample.json","hour_23_sample.json","hour_24_sample.json","hour_25_sample.json","hour_26_sample.json","hour_27_sample.json","hour_28_sample.json"};
        static string logSeparator = "--------------------";
        static List<string> batchIDs = new List<string>();
        static DateTime earliestDate = DateTime.MaxValue;
        static DateTime latestDate = DateTime.MinValue;

        public static List<Transaction> GetTransactions(string fileName)
        {
            try
            {
                if (fileName == string.Empty || fileName == "")
                {
                    Console.WriteLine("ERROR - GetTransactions - file name is empty ");
                    return null;
                }

                Console.WriteLine(logSeparator);
                Console.WriteLine("GetTransactions start at " + DateTime.Now);
                Console.WriteLine(logSeparator);

                List<Transaction> transactions = new List<Transaction>();
                string filePath = "Data\\hourly sample files\\" + fileName;

                if (!File.Exists(filePath))
                {
                    Console.WriteLine("ERROR - GetTransactions - file path does not exist: " + filePath);
                    return null;
                }

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
            catch (DirectoryNotFoundException e)
            {
                Console.WriteLine("ERROR - GetTransactions - DirectoryNotFound: " + e.Message);
                return null;
            }
            catch (FileNotFoundException e)
            {
                Console.WriteLine("ERROR - GetTransactions - FileNotFound: " + e.Message);
                return null;
            }
            catch (Exception e)
            {
                Console.WriteLine("ERROR - GetTransactions - general: " + e.Message);
                return null;
            }
        }

        public static List<Transaction> PostTransactions(TransactionContext context)
        {
            try
            {
                Console.WriteLine(logSeparator);
                Console.WriteLine("PostTransactions start at " + DateTime.Now);
                Console.WriteLine(logSeparator);

                var builder = WebApplication.CreateBuilder();
                List<Transaction> returnList = new List<Transaction>();
                int displayLimit = 0;

                displayLimit = builder.Configuration.GetValue<int>("RecordDisplayLimit");
                if (displayLimit == 0) // if specified node is not in config file, displayLimit is set to 0/null
                {
                    displayLimit = 1000;
                }
                // if displayLimit is larger than number of items in list, Take just grabs all items in list; if displayLimit is negative, Take grabs 0 items in list
                returnList = context.Transactions.ToList().Take(displayLimit).OrderBy(transac => transac.TransactionId).ToList(); // throws error if not cast to list again

                Console.WriteLine(logSeparator);
                Console.WriteLine("PostTransactions end at " + DateTime.Now);
                Console.WriteLine(logSeparator);

                return returnList;
            }
            catch(Exception e)
            {
                Console.WriteLine("ERROR - PostTransactions: " + e.Message);
                return null;
            }
        }

        public static Transaction markInsert(Transaction currTransaction)
        {
            try
            {
                if(currTransaction.SourceSystem == "" || string.IsNullOrEmpty(currTransaction.SourceSystem))
                {
                    Console.WriteLine("ERROR - markInsert - source system is invalid");
                    return null;
                }
                if(currTransaction.TransactionId == 0)
                {
                    Console.WriteLine("ERROR - markInsert - transaction ID is invalid");
                    return null;
                }

                Console.WriteLine("Inserting transaction " + currTransaction.SourceSystem + ", " + currTransaction.TransactionId);
                currTransaction.FullDayRange = currTransaction.TransactionTime.AddDays(1);
                currTransaction.AuditHistory = "insert";
                currTransaction.RevisionNumber = 0;

                return currTransaction;
            }
            catch (Exception e)
            {
                Console.WriteLine("ERROR - markInsert: " + e.Message);
                return null;
            }
        }

        public static List<Transaction> markInsertUpdate(TransactionContext context, List<Transaction> hourlyTransactions)
        {
            List<Transaction> insertUpdateTransactions = new List<Transaction>();
            Transaction insertedTransaction = new Transaction() {SourceSystem = "", TransactionId = 0};
            Transaction updatedTransaction = new Transaction() {SourceSystem = "", TransactionId = 0};

            try
            {
                // update matching transactions & insert the rest
                foreach (Transaction currTransaction in hourlyTransactions){
                    DateTime startRange = currTransaction.TransactionTime.AddDays(-1);
                    string uniqueID = currTransaction.SourceSystem + currTransaction.TransactionId.ToString();

                    // determine range of hourly import for handling revokes later
                    if (currTransaction.TransactionTime < earliestDate){earliestDate = currTransaction.TransactionTime;}
                    if (currTransaction.TransactionTime > latestDate){latestDate = currTransaction.TransactionTime;}
                    batchIDs.Add(uniqueID);

                    // find any transaction within the last 24 hours which need to be updated (has matching primary key)
                    Transaction transToUpdate = context.Transactions.FirstOrDefault(trans => trans.TransactionTime >= startRange && (trans.SourceSystem + trans.TransactionId.ToString() == uniqueID));

                    // Logging is handled in the functions called here
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
            catch (Exception e)
            {
                Console.WriteLine("ERROR - markInsertUpdate: " + e.Message);
                return null;
            }
        }

        public static Transaction markUpdated(TransactionContext context, Transaction currTransaction, Transaction transToUpdate)
        {
            try
            {
                if(transToUpdate.SourceSystem == "" || string.IsNullOrEmpty(transToUpdate.SourceSystem))
                {
                    Console.WriteLine("ERROR - markUpdated - transToUpdate source system is invalid");
                    return null;
                }
                if(transToUpdate.TransactionId == 0)
                {
                    Console.WriteLine("ERROR - markUpdated - transToUpdate transaction ID is invalid");
                    return null;
                }

                // make sure values are different; capture details from incoming record
                string newCardNum = currTransaction.CardNumber;
                string newLocCode = currTransaction.LocationCode;
                string newProdName = currTransaction.ProductName;
                decimal newAmount = currTransaction.Amount;
                DateTime newTransacTime = currTransaction.TransactionTime;

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
                    currTransaction.FullDayRange = currTransaction.TransactionTime.AddDays(1);
                    currTransaction.AuditHistory = "updated";
                    currTransaction.RevisionNumber ++;
                    return currTransaction; // only want to re-add if updated
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("ERROR - markUpdated: " + e.Message);
                return null;
            }
        }

        public static List<Transaction> identifyRevoked(TransactionContext context)
        {
            List<Transaction> revokeTransactions = new List<Transaction>();

            try
            {
                // want to remove original version & insert updated version
                foreach (Transaction origTransac in context.Transactions){
                    bool inRange = false;
                    string uniqueID = origTransac.SourceSystem + origTransac.TransactionId.ToString(); 

                    // no need to check anything else if transaction has already been revoked
                    if (origTransac.AuditHistory == "revoked")
                    {
                        continue;
                    }

                    // if any batch during 24 hour range does not include ID, revoke
                    //      need to capture overlap where batch range includes start date & where batch range includes end date
                    // TransactionTime indicates start of 24H range, FullDayRange indicates end of 24H range
                    if ((earliestDate <= origTransac.TransactionTime && latestDate <= origTransac.FullDayRange) || (earliestDate >= origTransac.TransactionTime && latestDate >= origTransac.FullDayRange))
                    {
                        inRange = true;
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
            catch (Exception e)
            {
                Console.WriteLine("ERROR - identifyRevoked:" + e.Message);
                return null;
            }
        }

        public static List<Transaction> markRevoked(TransactionContext context, List<Transaction> identifiedRevoked)
        {
            List<Transaction> revokedTransactions = new List<Transaction>();
            try
            {
                foreach (Transaction toRevoke in identifiedRevoked)
                {
                    if(toRevoke.SourceSystem == "" || string.IsNullOrEmpty(toRevoke.SourceSystem))
                    {
                        Console.WriteLine("ERROR - markRevoked - source system is invalid");
                        return null;
                    }
                    if(toRevoke.TransactionId == 0)
                    {
                        Console.WriteLine("ERROR - markRevoked - transaction ID is invalid");
                        return null;
                    }

                    Transaction currTransaction = Transaction.DeepCopy(toRevoke);
                    currTransaction.AuditHistory = "revoked";
                    currTransaction.RevisionNumber ++;
                    
                    Console.WriteLine("Revoking transaction for " + currTransaction.SourceSystem + ", " + currTransaction.TransactionId);
                    context.Remove(toRevoke);
                    revokedTransactions.Add(currTransaction);
                }
                return revokedTransactions;
            }
            catch (Exception e)
            {
                Console.WriteLine("ERROR - markRevoked: " + e.Message);
                return null;
            }
        }

        public static void parseImport(TransactionContext context, List<Transaction> hourlyTransactions)
        {
            try
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
            catch (Exception e)
            {
                Console.WriteLine("ERROR - parseImport: " + e.Message);
            }
        }

        public static void SaveToDB(TransactionContext context, string fileName)
        {
            try
            {
                Console.WriteLine(logSeparator);
                Console.WriteLine("SaveToDB start at " + DateTime.Now);
                Console.WriteLine(logSeparator);

                List<Transaction> hourlyTransactions = GetTransactions(fileName);
                parseImport(context, hourlyTransactions);
                context.SaveChanges(); // save insert into DB

                Console.WriteLine(logSeparator);
                Console.WriteLine("SaveToDB end at " + DateTime.Now);
                Console.WriteLine(logSeparator);
            }
            catch (Exception e)
            {
                Console.WriteLine("ERROR - SaveToDB: " + e.Message);
            }
        }
    }
}