﻿using System;

namespace SUS
{
    public class Timer
    {
        public enum Formats
        {
            Milliseconds,
            Seconds,
            Minutes
        }

        public Timer()
        {
            Limit = 0;
        }

        public Timer(int timeout, Formats format)
        {
            Limit = Parser(timeout, format);
        }

        private DateTime StartTime { get; set; }
        private DateTime EndTime { get; set; }
        private bool Running { get; set; }
        private bool Started { get; set; }
        public int Limit { get; private set; } = 1000;

        public int ElapsedTime
        {
            get
            {
                if (!Started) return 0;

                if (Started && !Running) return (int) (EndTime - StartTime).TotalMilliseconds;

                return (int) (DateTime.Now - StartTime).TotalMilliseconds;
            }
        }

        public int Ticks => Limit >= 1000 ? ElapsedTime / Limit : 0;

        public bool Completed => Limit >= 1000 && ElapsedTime >= Limit;

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

        public void AddLimit(int timeout, Formats format)
        {
            Limit = Parser(timeout, format);
        }

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

        public void Stop()
        {
            // It is not running and it has not ever started.
            if (!Running || !Started) return;

            EndTime = DateTime.Now;
            Running = false;
        }
    }
}