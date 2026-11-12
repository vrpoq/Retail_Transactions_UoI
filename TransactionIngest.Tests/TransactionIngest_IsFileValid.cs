using Microsoft.VisualStudio.TestTools.UnitTesting;
using TransactionIngest.Models;
using TransactionIngest.Data;

namespace TransactionIngest.UnitTests
{
    [TestClass]
    public class TransactionIngest_IsFileValid
    {

        [TestMethod]
        public void IsFileValid_InputIsHourX_ReturnNull()
        {
            List<Transaction> results = new List<Transaction>();
            results = TransactionProcessor.GetTransactions("hour_X_sample.json");

            Assert.IsNull(results, "invalid file name");
        }

        [TestMethod]
        public void IsFileValid_InputIsHour0_ReturnNull()
        {
            List<Transaction> results = new List<Transaction>();
            results = TransactionProcessor.GetTransactions("hour_0_sample.json");

            Assert.IsNull(results, "invalid file name");
        }

        [TestMethod]
        public void IsFileValid_InputIsHour1_ReturnFile()
        {
            List<Transaction> results = new List<Transaction>();
            results = TransactionProcessor.GetTransactions("hour_1_sample.json");

            Assert.IsNotEmpty(results, "valid file name");
        }
    }
}