using System;
using System.Security.Cryptography;
using System.Text;

namespace gtmp.evilempire.entities.processors
{
    public class UserPasswordHashProcessor : IEntityProcessor
    {
        public void Process(object entity)
        {
            var user = entity as User;
            if (user != null)
            {
                Process(user);
            }
        }

        public void Process(User user)
        {
            if (user == null)
            {
                return;
            }

            user.Password = Hash(user.Password);
        }

        public string Hash(string password)
        {
            if (password != null && password.StartsWith("!", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("Unable to hash raw password. Expect a cshash.");
            }

            if (password != null && password.StartsWith("::", StringComparison.OrdinalIgnoreCase))
            {
                using (var sha = SHA512.Create())
                {
                    var a = string.Concat("Xc_#", password, "__<0");
                    for (var i = 0; i < 10; i++)
                    {
                        var buffer = Encoding.UTF8.GetBytes(a);
                        a = Convert.ToBase64String(sha.ComputeHash(buffer, 0, buffer.Length));
                    }
                    password = string.Concat("#", a);
                }
            }

            return password;
        }
    }
}
