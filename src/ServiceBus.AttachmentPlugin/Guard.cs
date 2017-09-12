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

        public static void AgainstNegativeTime(string argumentName, TimeSpan? value)
        {
            if (value?.Ticks <= 0)
            {
                throw new ArgumentException(argumentName);
            }
        }
    }
}