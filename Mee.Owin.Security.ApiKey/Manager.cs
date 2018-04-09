//using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Mee.Owin.Security.ApiKey
{
    public class Manager
    {
        private static Manager _instance = null;
        private List<User> _manager = null ;

        public Manager()
        {
            _manager = new List<User>();
        }

        public Boolean Load()
        {
            var config = Config.ApiKeySection.Load();

            if (config != null)
            {
                _manager.Clear();

                config.Users
                    .OfType<Config.User>()
                    .ToList()
                    .ForEach(u => _manager.Add(
                        new User() {
                            ApiKey = u.ApiKey,
                            Name = u.Name,
                            Roles = u.Roles.OfType<Config.Role>().Select(r => r.Name).ToList()

                        })
                    );
            }

            return true;
        }

        public void LoadSample()
        {
            _manager.Clear();

            _manager.Add(new User()
            {
                ApiKey = "123",
                Name = "seb",
                Roles = new List<string> { "admins", "users" }
            });
            _manager.Add(new User()
            {
                ApiKey = "456",
                Name = "seb4",
                Roles = new List<string> { "admins" }
            });
            _manager.Add(new User()
            {
                ApiKey = "789",
                Name = "seb7",
                Roles = new List<string> { "users" }
            });

        }

        public static Manager GetInstance ()
        {
            if (_instance == null)
            {
                _instance = new Manager();
            }

            return _instance;
        }

        public User GetUser(string ApiKey)
        {
            User user = _manager.Find(u => u.ApiKey == ApiKey);
            return user;
        }

    }

}