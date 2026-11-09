using TransactionIngest.Models;
using TransactionIngest.Data;
interface IDataAPI
{
    // import hourly data & save to database
    // for this exercise, fileName identifies which static file to retrieve data from
    static List<Transaction> GetTransactions(string fileName)
    {
        return new List<Transaction>();
    }

    // display from database to GUI
    static List<Transaction> PostTransactions(TransactionContext context)
    {
        return new List<Transaction>();
    }
}