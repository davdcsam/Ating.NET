using cAlgo.API;


namespace Limits
{
    public class Limits
    {
        private readonly Robot _cbot;
        public double Highest { get; private set; }
        public double Lowest { get; private set; }
        public TimeFrame TimeFrame { get; private set; }

        public Limits(Robot robot, TimeFrame timeFrame)
        {
            _cbot = robot;
            TimeFrame = timeFrame ?? TimeFrame.Minute;
            Highest = double.MinValue;
            Lowest = double.MinValue;
        }

        public void Get(int periods)
        {
            if (periods < 1) { _cbot.Print($"Periods arg '{periods}' under 1"); return; }

            var bars = _cbot.MarketData.GetBars(TimeFrame);

            Highest = bars.HighPrices.Maximum(periods);

            Lowest = bars.LowPrices.Minimum(periods);
        }

        public void Get(DateTime start, DateTime end)
        {
            if (start >= end) { _cbot.Print($"Time inverted, start {start} over end {end}"); return; }

            var bars = _cbot.MarketData.GetBars(TimeFrame);

            var filteredBars = bars.Where(bar => bar.OpenTime >= start && bar.OpenTime <= end);

            Highest = filteredBars.Max(bar => bar.High);

            Lowest = filteredBars.Min(bar => bar.Low);
        }
    }

}
