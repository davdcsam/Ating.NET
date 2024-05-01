using cAlgo.API;

namespace ProfitProtection
{
    public enum TypeProfitProtection
    {
        BreakEven,
        TrailingStop
    }

    public class ProfitProtection
    {

        protected readonly Robot _cbot;
        protected double PercentActivation;
        protected double PercentDeviation;
        protected string LabelIdentifier;
        public ProfitProtection(Robot robot, double percentActivation, double percentDeviation, string labelIdentifier)
        {
            _cbot = robot;
            PercentActivation = percentActivation;
            PercentDeviation = percentDeviation;
            LabelIdentifier = labelIdentifier;
        }

        protected bool IsValid(Position position) { return (position.Symbol.Name == _cbot.Symbol.Name && position.Label == LabelIdentifier); }
    }
    public class BreakEven : ProfitProtection
    {
        public BreakEven(Robot robot, double percentActivation, double percentDeviation, string labelIdentifier) : base(robot, percentActivation, percentDeviation, labelIdentifier) { }

        public void Run()
        {
            Positions Positions = _cbot.Positions;

            if (!Positions.Any()) return;

            foreach (Position position in Positions)
            {
                if (!position.TakeProfit.HasValue || !IsValid(position)) continue;

                double priceActivation = position.EntryPrice + ((position.TakeProfit.Value - position.EntryPrice) * PercentActivation / 100);
                double newStopLoss = position.EntryPrice - (position.EntryPrice - position.TakeProfit.Value) * PercentDeviation / 100;

                if (position.TradeType == TradeType.Buy && position.CurrentPrice >= priceActivation) { _cbot.ModifyPosition(position, newStopLoss, position.TakeProfit); }

                if (position.TradeType == TradeType.Sell && position.CurrentPrice <= priceActivation) { _cbot.ModifyPosition(position, newStopLoss, position.TakeProfit); }
            }
        }
    }
    public class TrailingStop : ProfitProtection
    {
        public TrailingStop(Robot robot, double percentActivation, double percentDeviation, string labelIdentifier) : base(robot, percentActivation, percentDeviation, labelIdentifier) { }

        public void Run()
        {
            if (!_cbot.Positions.Any()) return;

            foreach (Position position in _cbot.Positions)
            {
                if (!position.TakeProfit.HasValue || !IsValid(position)) continue;

                double priceActivation = position.EntryPrice + ((position.TakeProfit.Value - position.EntryPrice) * PercentActivation / 100);
                double newStopLoss = position.CurrentPrice + (position.EntryPrice - position.TakeProfit.Value) * PercentDeviation / 100;

                if (position.TradeType == TradeType.Buy && position.CurrentPrice >= priceActivation) { _cbot.ModifyPosition(position, newStopLoss, position.TakeProfit); }

                if (position.TradeType == TradeType.Sell && position.CurrentPrice <= priceActivation) { _cbot.ModifyPosition(position, newStopLoss, position.TakeProfit); }
            }
        }
    }
}
