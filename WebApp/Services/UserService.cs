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
                return false; // Invalid input
            }

            var hashedPassword = HashPassword(user.Password);
            User fullUser = GetUserByUsername(user.Username);

            // Validate login and set the current user
            if (users.Any(u => u.Username == user.Username && u.Password == hashedPassword))
            {
                _currentUser = fullUser; // Set the logged-in user
                return true;
            }

            return false;
        }


        public bool Register(User user)
        {
            // Load the existing users from the JSON file
            var users = LoadUsers();

            // Check if the username or email already exists
            if (users.Any(u => u.Username == user.Username || u.Email == user.Email))
                return false; // Registration failed: username or email already exists

            // Hash the password before storing (for better security)
            user.Password = HashPassword(user.Password);

            // Add the new user to the list
            users.Add(user);

            // Save the updated list of users
            SaveUsers(users);

            return true; // Registration successful
        }

        // Method to hash the password (simple example with SHA256)
        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                byte[] hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(hashBytes); // Return hashed password as base64 string
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

            if (user != null && !user.CustomTags.Contains(tag) && !user.DefaultTags.Contains(tag))  // Ensure tag is not a default tag
            {
                user.CustomTags.Add(tag);  // Add the custom tag
                SaveUsers(users);

                // Update the current user after modifying the tags
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
