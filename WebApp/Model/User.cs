using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebApp.Model
{
    public class User
    {
        public string FullName { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string Currency { get; set; }

        public decimal Balance { get; set; }

        public List<string> DefaultTags { get; set; }
        public List<string> CustomTags { get; set; }

        public User()
        {
            // Initialize with some default tags
            DefaultTags = new List<string> { "groceries", "salary", "rent", "work", "travel" };
            CustomTags = new List<string>(); 
            Balance = 0;
        }
    }
}
