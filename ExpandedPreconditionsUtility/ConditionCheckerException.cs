using System;
using System.Runtime.Serialization;

namespace ExpandedPreconditionsUtility
{
    [Serializable]
    public class ConditionCheckerException : Exception
    {
        internal ConditionCheckerException()
        {
        }

        internal ConditionCheckerException(string message) : base(message)
        {
        }

        internal ConditionCheckerException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}