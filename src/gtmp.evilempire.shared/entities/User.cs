using System;

namespace gtmp.evilempire.entities
{
    [EntityStorage("user", nameof(Login))]
    public class User
    {
        public string Login { get; set; }
        public string Password { get; set; }
        public DateTime? LastLogin { get; set; }
        public DateTime? FirstLogin { get; set; }
    }
}
