namespace SUS.Server
{
    internal class Regenerator
    {
        public enum Speeds
        {
            Decay = -1,
            None = 0,
            Slow = 1,
            Medium = 2,
            Fast = 3,
            Super = 4,

            Normal = Slow
        }

        private readonly int _interval; // Multiplier for how much each tick is worth.

        private readonly Timer _timer;

        #region Constructors

        public Regenerator(Speeds speed)
        {
            _timer = new Timer(6, Timer.Formats.Seconds);
            _interval = (int) speed;
        }

        #endregion

        public bool Running => _timer.Running;

        /// <summary>
        ///     Get the amount of regeneration that has occured since this was last called.
        ///     Resets the counter.
        /// </summary>
        /// <returns>Amount of banked regeneration.</returns>
        public int RetrieveTicks()
        {
            var banked = _interval * _timer.Ticks;
            if (Running)
                _timer.Restart(); // Restart the internal timer, setting to 0.
            else
                _timer.StopAndClear();

            return banked;
        }

        /// <summary>
        ///     Stops the internal timer and resets the ticks to 0.
        /// </summary>
        public void Stop()
        {
            _timer.StopAndClear();
        }

        /// <summary>
        ///     Restarts the internal timer, sets the ticks to 0 and continues to run.
        /// </summary>
        public void Restart()
        {
            _timer.Restart();
        }
    }
}