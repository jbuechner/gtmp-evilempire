using gtmp.evilempire.entities;

namespace gtmp.evilempire
{
    public static class AuthExtensions
    {
        public static bool IsAuthGroupOrHigher(this AuthUserGroup checkedUserGroup, AuthUserGroup requiredUserGroup)
        {
            return checkedUserGroup >= requiredUserGroup;
        }
    }
}
