using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebApp.Model;

namespace WebApp.Services.Interface
{
    public interface ITransactionService
    {


        Task<bool> Create(string username, string title, decimal amount, string type, string debtSource = null, DateTime? dueDate = null, string note = null, string tag = null);
        Task<List<Transaction>> GetAll(string username);
        Task<bool> DeleteTransaction(string username, Guid transactionId);
        Task<bool> ClearDebt(string username, Guid transactionId);

    }
}
