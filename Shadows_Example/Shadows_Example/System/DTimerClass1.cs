using System.Diagnostics;

namespace DSharpDXRastertek.Tut40.System
{
    public class DTimer                 // 48 lines
    {
        // Variables
        private Stopwatch _StopWatch;
        private float m_ticksPerMs;
        private long m_LastFrameTime = 0;

        // Properties
        public float FrameTime { get; private set; }
        public float CumulativeFrameTime { get; private set; }

        // Public Methods
        public bool Initialize()
        {
            // Check to see if this system supports high performance timers.
            if (!Stopwatch.IsHighResolution)
                return false;
            if (Stopwatch.Frequency == 0)
                return false;

            _StopWatch = Stopwatch.StartNew();

            // Find out how many times the frequency counter ticks every millisecond.
            m_ticksPerMs = (float)(Stopwatch.Frequency / 1000.0f);

            return true;
        }
        public void Frame2()
        {
            // Query the current time.
            long currentTime = _StopWatch.ElapsedTicks;

            // Calculate the difference in time since the last time we queried for the current time.
            float timeDifference = currentTime - m_LastFrameTime;

            // Calculate the frame time by the time difference over the timer speed resolution.
            FrameTime = timeDifference / m_ticksPerMs;
            CumulativeFrameTime += FrameTime;

            // Restart the timer.
            m_LastFrameTime = currentTime;
        }
    }
}