using Newtonsoft.Json;
using TransactionIngest.Models;

namespace TransactionIngest.Data
{
    class TransactionProcessor : IDataAPI
    {
        // use this list to import all the data at once via for loop in SaveToDB
        List<String> fileNames = new List<string> { "hour_1_sample.json", "hour_2_sample.json", "hour_3_sample.json", "hour_4_sample.json", "hour_5_sample.json","hour_6_sample.json","hour_7_sample.json","hour_8_sample.json","hour_9_sample.json","hour_10_sample.json","hour_11_sample.json","hour_12_sample.json","hour_13_sample.json","hour_14_sample.json","hour_15_sample.json","hour_16_sample.json","hour_17_sample.json","hour_18_sample.json","hour_19_sample.json","hour_20_sample.json","hour_21_sample.json","hour_22_sample.json","hour_23_sample.json","hour_24_sample.json","hour_25_sample.json","hour_26_sample.json","hour_27_sample.json","hour_28_sample.json"};

        public static List<Transaction> GetTransactions(string fileName)
        {
            // TODO check to make sure fileName is populated
            // TODO check to make sure filePath exists
            List<Transaction> transactions = new List<Transaction>();
            string filePath = "Data\\hourly sample files\\" + fileName;

            using (StreamReader r = new StreamReader(filePath))
            {
                string json = r.ReadToEnd();
                transactions = JsonConvert.DeserializeObject<List<Transaction>>(json);
            }

            return transactions;
        }

        public void PostTransactions()
        {
            
        }

        public static void SaveToDB(TransactionContext context){
            List<Transaction> hourlyTransactions = GetTransactions("hour_1_sample.json"); // hardcoding to test

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

            context.Transactions.AddRange(hourlyTransactions);
            context.SaveChanges();
        }
    }
}