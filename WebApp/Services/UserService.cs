using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using WebApp.Abstraction;
using WebApp.Model;
using WebApp.Services.Interface;

namespace WebApp.Services
{
    public class UserService : UserBase, IUserService
    {
        private User _currentUser;

        public User CurrentUser => _currentUser;

        public bool Login(User user)
        {
            var users = LoadUsers();

            if (string.IsNullOrEmpty(user.Username) || string.IsNullOrEmpty(user.Password))
            {
                return false; 
            }

            var hashedPassword = HashPassword(user.Password);
            User fullUser = GetUserByUsername(user.Username);

            
            if (users.Any(u => u.Username == user.Username && u.Password == hashedPassword))
            {
                _currentUser = fullUser; 
                return true;
            }

            return false;
        }


        public bool Register(User user)
        {
            // Load the existing users
            var users = LoadUsers();

            if (users.Any(u => u.Username == user.Username || u.Email == user.Email))
                return false; 

            
            user.Password = HashPassword(user.Password);

            
            users.Add(user);

          
            SaveUsers(users);

            return true; 
        }

        
        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                byte[] hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(hashBytes); 
            }
        }

        public User GetUserByUsername(string username)
        {
            var users = LoadUsers();
            return users.FirstOrDefault(u => u.Username == username);
        }


        public bool AddCustomTagToUser(string username, string tag)
        {
            var users = LoadUsers();
            var user = users.FirstOrDefault(u => u.Username == username);

            if (user != null && !user.CustomTags.Contains(tag) && !user.DefaultTags.Contains(tag))  
            {
                user.CustomTags.Add(tag);  
                SaveUsers(users);

                
                if (_currentUser != null && _currentUser.Username == username)
                {
                    _currentUser = user;  // Refresh the current user with the updated custom tags
                }

                return true;
            }

            return false;
        }

        public void Logout()
        {
            _currentUser = null; // Clear the current user
        }
    }

}
