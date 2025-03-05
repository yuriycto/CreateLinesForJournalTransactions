# CreateLinesForJournalTransactions

## How to Create Lines for Journal Transactions in Acumatica

Creating journal transaction lines from the Invoice screen in Acumatica is essential for automating financial processes and ensuring accurate ledger entries. This guide explains the process and provides a practical implementation.

### Detailed Explanation  
A complete step-by-step explanation can be found here:  
[Create Lines for Journal Transaction from Invoice Screen](https://blog.zaletskyy.com/post/2025/02/17/create-lines-for-journal-transaction-from-invoice-screen)  

If you need additional assistance, leave a request here:  
[Contact Us](https://acupowererp.com/contact-us)  

### Code Implementation  

The provided C# class, `AcuPowerSampleSOInvoiceEntryExt`, demonstrates how to extend the Acumatica ERP system to generate journal transaction lines automatically when an invoice is processed. Key functionalities include:  

- **Identifying Allowance Items**: The extension retrieves specific inventory items classified as "ALLOWANCES" and checks if they match predefined customer allowance rules.  
- **Calculating Allowance Amounts**: It calculates allowance-based transactions by applying percentage-based logic to eligible invoice lines.  
- **Creating Journal Entries**: Debit and credit entries are automatically created for the appropriate accounts (`expenseAccountID` and `accrualAccountID`).  

This extension ensures seamless integration between sales invoices and financial records, reducing manual efforts and improving financial accuracy in Acumatica.  
