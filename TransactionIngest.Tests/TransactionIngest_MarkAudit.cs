using Microsoft.VisualStudio.TestTools.UnitTesting;
using TransactionIngest.Models;
using TransactionIngest.Data;
using TransactionIngest.Pages.Transactions;

namespace TransactionIngest.UnitTests
{
    [TestClass]
    public class TransactionIngest_MarkAudit
    {

        /*
        private readonly TransactionIngest.Data.TransactionContext _testContext;
        public TransactionIngest_MarkAudit(TransactionIngest.Data.TransactionContext testContext)
        {
            _testContext = testContext;
        }
        */

        [TestMethod]
        public void IsMarkInsertValid_InputIsNull_ReturnNull()
        {
            Transaction result = new Transaction();
            result = TransactionProcessor.markInsert(null);

            Assert.IsNull(result, "no currTransaction");
        }

        [TestMethod]
        public void IsMarkInsertValid_InputPrimaryKeyNull_ReturnNull()
        {
            Transaction currTransaction = new Transaction
            {
                SourceSystem = null,
                TransactionId = 0,
                CardNumber = "****1234",
                LocationCode = "UPTOWN",
                ProductName = "B Tablet",
                Amount = 60.02m,
                TransactionTime = DateTime.Now
            };

            Transaction result = new Transaction(){SourceSystem = null, TransactionId = 0};
            result = TransactionProcessor.markInsert(currTransaction);

            Assert.IsNull(result, "invalid primary key");
        }

        [TestMethod]
        public void IsMarkInsertValid_InputValid_ReturnTransaction()
        {
            Transaction currTransaction = new Transaction
            {
                SourceSystem = "Store Uptown",
                TransactionId = 100900,
                CardNumber = "****1234",
                LocationCode = "UPTOWN",
                ProductName = "B Tablet",
                Amount = 60.02m,
                TransactionTime = DateTime.Now
            };

            Transaction result = new Transaction(){SourceSystem = null, TransactionId = 0};
            result = TransactionProcessor.markInsert(currTransaction);

            Assert.IsNotNull(result, "valid primary key");
        }

        // TODO fix methods using _testContext - uncomment and troubleshoot test build failing 
        //  not test result is failure, something is wrong with the implementation of _testContext

        /*
        [TestMethod]
        public void IsMarkInsertUpdateValid_InputValid_ReturnTransaction()
        {
            List<Transaction> currTransactions = new List<Transaction>();
            List<Transaction> returnTransactions = new List<Transaction>();

           returnTransactions = TransactionProcessor.markInsertUpdate(_testContext, currTransactions);

            Assert.IsEmpty(returnTransactions, "no input transactions");
        }
        */
        
        // TODO add more unit tests here for marking updated, marking revoked
        //      mark updated - test return null for idempotent rerun, return null for invalid primary key, return valid transaction object for valid update
        //      mark revoked - test return null for invalid primary key, return empty list for input empty list, return valid revoked list for valid input list
        // TODO create new unit test file(s) for identify revoked, SaveToDB
        //      identify revoked - test return empty list for input empty list, etc.
        //      SaveToDB - test no context, invalid file name

    }
}