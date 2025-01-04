using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;  // Added for Task-based async support
using WebApp.Model;
using WebApp.Services.Interface;
using WebApp.Abstraction;

namespace WebApp.Services
{
    public class TransactionService : UserBase, ITransactionService
    {
        // Method to get the application directory path
        private static string GetAppDirectoryPath()
        {
            // Get the path to the application directory
            string appDirectoryPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "Islington-Transactions" // Directory for transaction-related data
            );

            // Check if the directory exists, if not, create it
            if (!Directory.Exists(appDirectoryPath))
            {
                Directory.CreateDirectory(appDirectoryPath); // Create the directory if it doesn't exist
            }

            return appDirectoryPath; // Return the directory path
        }

            
        // Method to get the path for the users.json file
        private static string GetAppUsersFilePath()
        {
            return Path.Combine(GetAppDirectoryPath(), "users.json");
        }

        // Method to get the path for a specific user's transaction file
        private static string GetTransactionsFilePath(string username)
        {
            return Path.Combine(GetAppDirectoryPath(), $"{username}_transactions.json");
        }

        // Method to load all transactions for a specific user asynchronously
        public async Task<List<Transaction>> GetAll(string username)
        {
            string filePath = GetTransactionsFilePath(username);

            // Check if the transaction file exists
            if (!File.Exists(filePath))
            {
                return new List<Transaction> { new Transaction { Title = $"File path not found: {filePath}" } }; // Return empty list if no transactions exist for the user
            }

            try
            {
                // Read the file asynchronously
                var json = await File.ReadAllTextAsync(filePath);

                // Deserialize the JSON content into a list of transactions
                // If deserialization fails (null result), return an empty list
                return JsonSerializer.Deserialize<List<Transaction>>(json) ?? new List<Transaction>();
            }
            catch (Exception ex)
            {
                // Log the error (you could use a logging framework here)
                Console.WriteLine($"Error deserializing transactions for {username}: {ex.Message}");
                return new List<Transaction>(); // Return empty list in case of an error
            }
        }


        // Method to save all transactions for a specific user asynchronously
        private async Task SaveTransactions(string username, List<Transaction> transactions)
        {
            string filePath = GetTransactionsFilePath(username);

            var json = JsonSerializer.Serialize(transactions);
            // Write to file asynchronously
            await File.WriteAllTextAsync(filePath, json);
        }

        // Method to create and add a new transaction for a user asynchronously
        public async Task<bool> Create(
            string username,
            string title,
            decimal amount,
            string type,
            string debtSource = null,
            DateTime? dueDate = null,
            string note = null,
            string tag = null)
        {
            if (string.IsNullOrEmpty(title))
                throw new Exception("Title is required.");
            if (amount <= 0)
                throw new Exception("Amount must be greater than zero.");
            if (string.IsNullOrEmpty(type))
                throw new Exception("Transaction type is required.");

            if (type == "Debt" && (string.IsNullOrEmpty(debtSource) || !dueDate.HasValue))
                throw new Exception("Debt Source and Due Date are required for a Debt transaction.");

            var transaction = new Transaction
            {
                Title = title,
                Amount = amount,
                Type = type,
                DebtSource = debtSource,
                DueDate = dueDate,
                Note = note,
                Tag = tag,
                Date = DateTime.Now,
                CreatedBy = username
            };

            // Load transactions and user
            var transactions = await GetAll(username);
            var users = LoadUsers();
            var user = users.FirstOrDefault(u => u.Username == username);

            if (user == null)
                throw new Exception("User not found.");

            // Check if the user has enough balance for a debit transaction
            if (type == "Debit" && user.Balance < amount)
            {
                throw new Exception("Insufficient balance for the debit transaction.");
            }

            // Update balance based on transaction type
            if (type == "Credit")
                user.Balance += amount;
            else if (type == "Debit")
                user.Balance -= amount;

            transactions.Add(transaction);

            // Save updated transactions and users
            await SaveTransactions(username, transactions);
            SaveUsers(users); // Ensure updated balance is persisted

            return true;
        }



    }
}
