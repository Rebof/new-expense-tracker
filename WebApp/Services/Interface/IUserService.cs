using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebApp.Model;

namespace WebApp.Services.Interface
{
    public interface IUserService
    {
        bool Register(User user);

        bool Login(User user);

        User GetUserByUsername(string username);

        // Property or method to get the current logged-in user
        User CurrentUser { get; }

        bool AddCustomTagToUser(string username, string newTag);

        void Logout();

    }
}
