using System;

namespace Stagger
{
    public class FormatException : Exception
    {
        public FormatException(string message) : base(message)
        {
        }
    }
}