using System;

namespace PrivacyGuard.Helper // Agar folder ka naam Helpers hai toh 'Helpers' kar dena
{
    public class OrgUnitRuleException : Exception
    {
        public OrgUnitRuleException(string message) : base(message) { }
    }
}