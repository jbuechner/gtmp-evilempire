using gtmp.evilempire.entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gtmp.evilempire.services
{
    public class LoginResponse
    {
        public string Login { get; set; }
        public AuthUserGroup UserGroup { get; set; }
        public bool HasBeenThroughInitialCustomization { get; set; }
        public string FreeroamCustomizationData { get; set; }
        public string CharacterCustomizationData { get; set; }
    }
}
