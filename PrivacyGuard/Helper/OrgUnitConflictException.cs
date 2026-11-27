using System;

namespace PrivacyGuard.Helpers
{
    // 409 Conflict ke liye (duplicate code, delete/deactivate blocked, etc.)
    public class OrgUnitConflictException : InvalidOperationException
    {
        public OrgUnitConflictException(string message) : base(message) { }
    }

    // 403 Forbidden ke liye (branch ke bahar ki unit)
    public class OrgUnitForbiddenException : InvalidOperationException
    {
        public OrgUnitForbiddenException(string message) : base(message) { }
    }
}