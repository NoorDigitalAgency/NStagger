using System;

namespace Stagger
{
    public class TagNameException:Exception
    {
        public TagNameException(string message):base(message)
        {
        }
    }
}