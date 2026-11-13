# Build instructions

- Run "dotnet build" in overall project directory (.\Retail_Transactions_UoI\Retail_Transactions_UOI)

# Run instructions

- This project is designed to throw errors if the user is not in the Development environment; always need someone else to take a look at my work before I push it to production
- Run from Visual Studio Code using Run button with open .cs file (such as Program.cs or TransactionProcessor.cs)

# Design decisions and assumptions

- No AI assistance was used in the development of this project

- instead of using APIs, created a number of JSON files with static sample data
- input files contain a growing list of hourly import data until it fills a 24-hour sliding window
	- hour 1 contains only data from the first batch, hour 2 contains data from batches 1 & 2, hour 3 contains data from batches 1, 2, and 3, etc.
	- hour 25 contains data from batches 2-25, hour 26 contains data from batches 3-26, etc.
    - Each hourly import after the first one contains 2 updates; each hourly import after the second one contains 1 revoke, alternating between revoking inserts and updates
    - Updates may occur to card number, amount, or product name & amount
- Audit history records basic info - whether the current version of a record comes from insert, update, or revoke
- Logs and errors are currently written to the debug console via Console.WriteLine()
- Unit tests can be run using "dotnet test" command in terminal in ...\Retail_Transactions_UoI\Retail_Transactions_UoI directory or through View > Testing sidebar & Test Results tab in terminal area

# Other notes

- Input process uses textbox value to find input file in static directory; if using live APIs, easiest thing would be to have them write to a file in the static directory
	- Index page is located at ...\TransactionIngest\Pages\Index.cshtml
	- Data folder is located at  ...\TransactionIngest\Data
- Local database file should be deleted for my submission
- To show hidden audit history and revision number columns, set AuditColumnStyle in appsettings.json to "display:block width=100%"
	- "display:none" hides both columns
	- I had a lot of trouble setting the styling as a class, but the inline styling does the trick

# Future scope

- Allow user to specify file directory through json config file
- Expand audit history column to specify which fields were updated
- Improve error handling to display popup message to user informing them of the root error
- Improve error handling further by logging status messages and error messages to database table with id, message, source, and severity code
- Expand unit testing (see notes in TransactionIngest_MarkAudit.cs)
- Create Product class with dictionary to make sure the right amount is set for a given product
- Add column for payment option - cash vs card vs check

- Mark transactions older than 24 hours as finalized & make immutable
- Add sorting
- Add filtering
- Add pagination and set config value for number of results per page