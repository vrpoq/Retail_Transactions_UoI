using Microsoft.VisualStudio.TestTools.UnitTesting;
using TransactionIngest.Models;
using TransactionIngest.Data;

namespace TransactionIngest.UnitTests
{
    [TestClass]
    public class TransactionIngest_PostTransactions
    {

        [TestMethod]
        public void IsContextValid_InputIsNull_ReturnNull()
        {
            List<Transaction> results = new List<Transaction>();
            results = TransactionProcessor.PostTransactions(null);

            Assert.IsNull(results, "no context");
        }

    }
}