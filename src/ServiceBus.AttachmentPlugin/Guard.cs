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
        public static void AgainstNull(string argumentName, object value)
        {
            if (value == null)
            {
                throw new ArgumentNullException(argumentName);
            }
        }

        public static void AgainstNegative(string argumentName, long value)
        {
            if (value < 0)
            {
                throw new ArgumentException(argumentName);
            }
        }
    }
}