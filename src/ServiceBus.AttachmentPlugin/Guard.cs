using System;

namespace ServiceBus.AttachmentPlugin
{
    static class Guard
    {
        public static void AgainstEmpty(string argumentName, string value)
        {
            if (value != null && string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentNullException(argumentName);
            }
        }
    }
}