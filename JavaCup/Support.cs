namespace JavaCup
{
    using System;

    public class Support
    {
        private static DateTime Epoch = new (0x7b2, 1, 1, 0, 0, 0, 0);

        public static long CurrentTimeMillis()
        {
            TimeSpan span = (TimeSpan) (DateTime.Now - Epoch);
            return (long) span.TotalMilliseconds;
        }
    }
}

