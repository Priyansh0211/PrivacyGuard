using Microsoft.AspNetCore.Mvc;

namespace PrivacyGuard.Filters
{
    public class HasPermissionAttribute : TypeFilterAttribute
    {
        public HasPermissionAttribute(string permission) : base(typeof(PermissionFilter))
        {
            Arguments = new object[] { permission };
        }
    }
}