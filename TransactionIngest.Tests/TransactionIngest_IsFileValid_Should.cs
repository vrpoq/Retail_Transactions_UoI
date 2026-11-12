//using System.Transactions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TransactionIngest.Models;
using TransactionIngest.Data;
//using TransactionIngest.Services;

namespace TransactionIngest.UnitTests
{
    [TestClass]
    public class TransactionIngest_IsFileValid_Should
    {
        //private readonly PrimeService _primeService;

        /*
        public PrimeService_IsPrimeShould()
        {
            _primeService = new PrimeService();
        }
        */

        [TestMethod]
        public void IsFileValid_InputIsHourX_ReturnEmpty()
        {
            List<Transaction> results = TransactionProcessor.GetTransactions("hour_X_sample.json");

            Assert.IsEmpty(results, "invalid file name");

            // System.ArgumentNullException: Value cannot be null. (Parameter 'source')
        }
    }
}