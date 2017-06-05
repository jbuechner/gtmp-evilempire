using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gtmp.evilempire.server
{
    static class ClientEvents
    {
        public static readonly string EnterFreeroam = "enterFreeroam";

        public static readonly string DisplayLoginScreen = "::display:login";
        public static readonly string DisplayCharacterCustomization = "::display:characterCustomization";

        public static readonly string RequestLogin = "req:login";
        public static readonly string RequestLoginResponse = "res:login";

        public static readonly string RequestCustomizeCharacter = "req:customizeChar";
        public static readonly string RequestCustomizeCharacterOk = "req:customizeChar:ok";
        public static readonly string RequestCustomizeCharacterCancel = "req:customizeChar:cancel";
        public static readonly string RequestCustomizeCharacterResponse = "res:customizeChar";
        public static readonly string RequestInteractWithEntity = "req:interactWithEntity";
        public static readonly string RequestInteractWithEntityResponse = "res:interactWithEntity";
        public static readonly string RequestTriggerEntityInteraction = "req:triggerEntityAction";
        public static readonly string RequestTriggerEntityInteractionResponse = "res:triggerEntityAction";

        public static readonly string MoneyChanged = "moneyChanged";
    }
}
