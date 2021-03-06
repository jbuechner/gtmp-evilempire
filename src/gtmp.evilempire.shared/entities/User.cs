﻿using System;

namespace gtmp.evilempire.entities
{
    public class User
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1726:UsePreferredTerms", MessageId = "Login")]
        public string Login { get; set; }
        public string Password { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1726:UsePreferredTerms", MessageId = "Login")]
        public DateTime? LastLogin { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1726:UsePreferredTerms", MessageId = "Login")]
        public DateTime? FirstLogin { get; set; }
        public DateTime? LastSuccessfulLogin { get; set; }
        public int NumberOfInvalidLoginAttempts { get; set; }

        public AuthUserGroup UserGroup { get; set; }
    }
}
