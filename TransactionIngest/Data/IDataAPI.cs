using TransactionIngest.Models;
interface IDataAPI
{
    // import hourly data & save to database
    // for this exercise, fileName identifies which static file to retrieve data from
    static List<Transaction> GetTransactions(string fileName)
    {
        return new List<Transaction>(); // see if specifiying "new" here results in runtime errors
    }

    // display from database to GUI
    void PostTransactions();
}