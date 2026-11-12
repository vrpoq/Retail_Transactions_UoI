using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TransactionIngest.Data;
using TransactionIngest.Models;

namespace TransactionIngest.Pages.Transactions
{
    public class IndexModel : PageModel
    {
        private readonly TransactionIngest.Data.TransactionContext _context;

        public IndexModel(TransactionIngest.Data.TransactionContext context)
        {
            _context = context;
        }

        public List<Transaction> Transaction { get;set; } = default!;

        public async Task OnGetAsync()
        {
            Transaction = TransactionProcessor.PostTransactions(_context);
        }

        public IActionResult OnPost(string fileName) // pass in name/id of textbox value
        {
            // import new values
            TransactionProcessor.SaveToDB(_context, fileName);
            // add to display list
            Transaction = TransactionProcessor.PostTransactions(_context);
            // refresh index
            return RedirectToPage("./Index");
        }
    }
}
