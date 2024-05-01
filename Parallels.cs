using cAlgo.API;

namespace Parallels
{
    public class ParallelGenerator
    {
        private readonly Robot _cbot;
        public double Start { get; private set; }
        public double End { get; private set; }
        public double Step { get; private set; }
        public double Add { get; private set; }
        public double Current { get; private set; }
        public bool IsAdd { get; private set; }

        public List<double> Generated { get; private set; }
        public double UpperBuyPrice { get; private set; }
        public double UpperSellPrice { get; private set; }
        public double LowerBuyPrice { get; private set; }
        public double LowerSellPrice { get; private set; }

        public ParallelGenerator(Robot _robot, double start, double end, double step, double add)
        {
            _cbot = _robot;
            Start = Math.Min(start, end);
            End = Math.Max(start, end);
            Step = step;
            Add = add;
            Current = Start;
            IsAdd = false;
            Generated = new List<double>();
        }

        public void GenerateInternal()
        {
            Generated.Clear();
            double stepAdd = Step + Add;
            double stepSubtract = Step - Add;

            while (Current <= End)
            {
                Generated.Add(Current);

                if (!IsAdd)
                {
                    Current += stepSubtract;
                }
                else
                {
                    Current += stepAdd;
                }

                IsAdd = !IsAdd;
            }
        }

        public void Generate()
        {
            double closePrice = (_cbot.Symbol.Bid + _cbot.Symbol.Ask) / 2;
            int TotalSizeGenerated = Generated.Count;
            if (TotalSizeGenerated <= 0)
            {
                return;
            }

            UpperBuyPrice = double.MinValue;
            UpperSellPrice = double.MinValue;
            LowerBuyPrice = double.MinValue;
            LowerSellPrice = double.MinValue;

            for (int i = 0; i < Generated.Count - 1; i++)
            {
                if (closePrice > Generated[i] && closePrice < Generated[i + 1])
                {
                    if (closePrice < Generated[i] + Add)
                    {
                        _cbot.Print("Inside a Parallel");

                        UpperBuyPrice = Generated[i + 1];

                        UpperSellPrice = Generated[i] + Add;
                        LowerBuyPrice = Generated[i];

                        LowerSellPrice = Generated[i - 1] + Add;

                        break;
                    }

                    _cbot.Print("Between Parallels");

                    UpperSellPrice = Generated[i + 1] + Add;
                    UpperBuyPrice = Generated[i + 1];

                    LowerSellPrice = Generated[i] + Add;
                    LowerBuyPrice = Generated[i];

                    break;
                }
            }

            _cbot.Print($"UpperBuy {UpperBuyPrice} UpperSell {UpperSellPrice} LowerBuy {LowerBuyPrice} LowerSell {LowerSellPrice}");
        }
    }

}
