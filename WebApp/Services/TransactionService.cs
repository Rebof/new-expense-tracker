using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using WebApp.Model;
using WebApp.Services.Interface;
using WebApp.Abstraction;
using System.Diagnostics;

namespace WebApp.Services
{
    public class TransactionService : UserBase, ITransactionService
    {
        private static string GetAppDirectoryPath()
        {
            string appDirectoryPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "Islington-Transactions"
            );

            if (!Directory.Exists(appDirectoryPath))
            {
                Directory.CreateDirectory(appDirectoryPath);
            }

            return appDirectoryPath;
        }

        private static string GetAppUsersFilePath()
        {
            return Path.Combine(GetAppDirectoryPath(), "users.json");
        }

        private static string GetTransactionsFilePath(string username)
        {
            return Path.Combine(GetAppDirectoryPath(), $"{username}_transactions.json");
        }

        public async Task<List<Transaction>> GetAll(string username)
        {
            string filePath = GetTransactionsFilePath(username);

            if (!File.Exists(filePath))
            {
                return new List<Transaction> { new Transaction { Title = $"File path not found: {filePath}" } };
            }

            try
            {
                var json = await File.ReadAllTextAsync(filePath);
                return JsonSerializer.Deserialize<List<Transaction>>(json) ?? new List<Transaction>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deserializing transactions for {username}: {ex.Message}");
                return new List<Transaction>();
            }
        }

        private async Task SaveTransactions(string username, List<Transaction> transactions)
        {
            string filePath = GetTransactionsFilePath(username);
            var json = JsonSerializer.Serialize(transactions);
            await File.WriteAllTextAsync(filePath, json);
        }

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

            if (type == "Debt")
            {
                transaction.DebtCleared = false;
            }
            else
            {
                transaction.DebtCleared = null;
            }

            var transactions = await GetAll(username);
            var users = LoadUsers();
            var user = users.FirstOrDefault(u => u.Username == username);

            if (user == null)
                throw new Exception("User not found.");

            if (type == "Debit" && user.Balance < amount)
            {
                throw new Exception("Insufficient balance for the debit transaction.");
            }

            if (type == "Credit")
                user.Balance += amount;
            else if (type == "Debit")
                user.Balance -= amount;

            transactions.Add(transaction);
            await SaveTransactions(username, transactions);
            SaveUsers(users);

            return true;
        }

        public async Task<bool> ClearDebt(string username, int transactionId)
        {
            var transactions = await GetAll(username);
            var debtTransaction = transactions.FirstOrDefault(t => t.Id == transactionId && t.Type == "Debt" && t.DebtCleared == false);

            if (debtTransaction == null)
            {
                return false;
            }

            var users = LoadUsers();
            var user = users.FirstOrDefault(u => u.Username == username);

            if (user == null)
                throw new Exception("User not found.");

            if (user.Balance < debtTransaction.Amount)
            {
                return false;
            }

            Debug.WriteLine($"User {username}: Balance before deduction: {user.Balance}");
            Console.WriteLine($"User {username}: Balance before deduction: {user.Balance}");

            user.Balance -= debtTransaction.Amount;

            Console.WriteLine($"User {username}: Balance after deduction: {user.Balance}");

            debtTransaction.DebtCleared = true;
            debtTransaction.Date = DateTime.Now;

            await SaveTransactions(username, transactions);
            SaveUsers(users);

            return true;
        }
    }
}
