using System;

namespace SUS.Server
{
    public class Timer
    {
        public enum Formats
        {
            Milliseconds,
            Seconds,
            Minutes
        }

        /// <summary>
        ///     Limitless timer that runs forever.
        /// </summary>
        public Timer()
        {
            Limit = 0;
        }

        /// <summary>
        ///     A timer where the intervals are determined, allows the use of 'Ticks'
        /// </summary>
        /// <param name="timeout">Time desired.</param>
        /// <param name="format">Interval the time is.</param>
        public Timer(int timeout, Formats format)
        {
            Limit = Parser(timeout, format);
        }

        private DateTime StartTime { get; set; }
        private DateTime EndTime { get; set; }
        public bool Running { get; private set; }
        private bool Started { get; set; }
        public int Limit { get; private set; }

        public int ElapsedTime
        {
            get
            {
                if (!Started) return 0;

                if (Started && !Running) return (int) (EndTime - StartTime).TotalMilliseconds;

                return (int) (DateTime.Now - StartTime).TotalMilliseconds;
            }
        }

        /// <summary>
        ///     Amount fo times that the limit has been exceeded in the current run.
        /// </summary>
        public int Ticks => Limit >= 1000 ? ElapsedTime / Limit : 0;

        public bool Completed => Limit >= 1000 && ElapsedTime >= Limit;

        /// <summary>
        ///     Parses a number and determines what the limit will be in milliseconds.
        ///     By default it will always be a minimum of 1000 milliseconds (1 second).
        /// </summary>
        /// <param name="timeout">Raw time.</param>
        /// <param name="format">Format the raw time is in.</param>
        /// <returns>Timeout converted to milliseconds.</returns>
        private static int Parser(int timeout, Formats format)
        {
            var limit = 0;
            switch (format)
            {
                case Formats.Milliseconds:
                    limit = timeout;
                    break;
                case Formats.Seconds:
                    limit = timeout * 1000;
                    break;
                case Formats.Minutes:
                    limit = timeout * 60000;
                    break;
            }

            return limit > 1000 ? limit : 1000;
        }

        /// <summary>
        ///     Edits the limit based on a new raw timeout and format.
        /// </summary>
        /// <param name="timeout">Raw integer for the new timeout.</param>
        /// <param name="format">Format of the raw integer.</param>
        public void AddLimit(int timeout, Formats format)
        {
            Limit = Parser(timeout, format);
        }

        /// <summary>
        ///     Restarts the timer by setting the "start time" to the current time. Continues to run.
        /// </summary>
        public void Restart()
        {
            if (!Started)
            {
                // It has never been started.
                Start();
                return;
            }

            StartTime = DateTime.Now;
            Running = true;
        }

        /// <summary>
        ///     If the timer is not running. Otherwise, it will return early.
        /// </summary>
        public void Start()
        {
            if (Running) return;

            if (!Started)
            {
                StartTime = DateTime.Now;
                Started = true;
            }

            Running = true;
        }

        /// <summary>
        ///     Stops the timer and sets the EndTime.
        /// </summary>
        public void Stop()
        {
            // It is not running and it has not ever started.
            if (!Running || !Started) return;

            EndTime = DateTime.Now;
            Running = false;
        }

        /// <summary>
        ///     Stops the timer and sets the ticks to 0 by assigning
        ///     the End Time to match the Start Time.
        /// </summary>
        public void StopAndClear()
        {
            Stop();
            StartTime = EndTime;
        }
    }
}