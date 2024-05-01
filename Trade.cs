using cAlgo.API;

namespace Trade
{
    public enum Switcher
    {
        Activated,
        Deactivated
    }
    public enum TypeRemoval
    {
        OnlyOrders,
        OnlyPositions,
        Both
    }
    public class PlaceOrders
    {
        private readonly Robot _cbot;
        public double LotSize { get; private set; }
        public double StopLoss { get; private set; }
        public double TakeProfit { get; private set; }
        public string LabelIdentifier { get; private set; }
        public PlaceOrders(Robot _robot, double lotSize, double stopLoss, double takeProfit, string labelIdentifier)
        {
            _cbot = _robot;
            LotSize = lotSize * _cbot.Symbol.LotSize;
            StopLoss = stopLoss * _cbot.Symbol.PipSize;
            TakeProfit = takeProfit * _cbot.Symbol.PipSize;
            LabelIdentifier = labelIdentifier;
        }

        protected bool BuyComposite(double targetPrice)
        {
            if (_cbot.Ask < targetPrice)
            {
                TradeResult tradeResult = _cbot.PlaceStopOrder(TradeType.Buy, _cbot.Symbol.Name, LotSize, targetPrice, LabelIdentifier, StopLoss, TakeProfit);

                if (tradeResult.IsSuccessful) { return true; }
            }
            else
            {
                TradeResult tradeResult = _cbot.PlaceLimitOrder(TradeType.Buy, _cbot.Symbol.Name, LotSize, targetPrice, LabelIdentifier, StopLoss, TakeProfit);

                if (tradeResult.IsSuccessful) { return true; }
            }
            return false;
        }
        protected bool SellComposite(double targetPrice)
        {
            if (_cbot.Bid > targetPrice)
            {
                TradeResult tradeResult = _cbot.PlaceStopOrder(TradeType.Sell, _cbot.Symbol.Name, LotSize, targetPrice, LabelIdentifier, StopLoss, TakeProfit);

                if (tradeResult.IsSuccessful) { return true; }
            }
            else
            {
                TradeResult tradeResult = _cbot.PlaceLimitOrder(TradeType.Sell, _cbot.Symbol.Name, LotSize, targetPrice, LabelIdentifier, StopLoss, TakeProfit);

                if (tradeResult.IsSuccessful) { return true; }
            }
            return false;
        }

        public void Send(TradeType tradeType, double targetPrice)
        {
            if (tradeType == TradeType.Buy) { BuyComposite(targetPrice); }
            else { SellComposite(targetPrice); }
        }
    }
    public class Removal
    {
        protected readonly Robot _cbot;
        public List<PendingOrder> OrdersBuy { get; private set; }
        public List<PendingOrder> OrdersSell { get; private set; }
        public List<PendingOrder> OrdersUpper { get; private set; }
        public List<PendingOrder> OrdersLower { get; private set; }

        public string LabelIdentifier { get; private set; }
        public Removal(Robot robot, string labelIdentifier)
        {
            _cbot = robot;
            OrdersBuy = new List<PendingOrder>();
            OrdersSell = new List<PendingOrder>();
            OrdersUpper = new List<PendingOrder>();
            OrdersLower = new List<PendingOrder>();
            LabelIdentifier = labelIdentifier;
        }

        private void RemoveFromList(List<PendingOrder> pendingOrders)
        {
            foreach (var pendingOrder in pendingOrders) { _cbot.CancelPendingOrder(pendingOrder); }
        }

        public static bool IsIn(PendingOrder order, List<PendingOrder> toSearchIn)
        {
            if (toSearchIn.Contains(order)) return true;
            return false;
        }
        public void UpdateOrders()
        {
            OrdersBuy.Clear();
            OrdersSell.Clear();
            foreach (var pendingOrder in _cbot.PendingOrders)
            {
                if (pendingOrder.Symbol.Name != _cbot.Symbol.Name && pendingOrder.Label != LabelIdentifier) { continue; }

                if (pendingOrder.TradeType == TradeType.Buy) { OrdersBuy.Add(pendingOrder); }
                else { OrdersSell.Add(pendingOrder); }
            }
        }
        public void UpdateOrders(List<double> limitsToAverage)
        {
            OrdersUpper.Clear();
            OrdersLower.Clear();
            double average = limitsToAverage.Average();

            foreach (var pendingOrder in _cbot.PendingOrders)
            {
                if (pendingOrder.Symbol.Name != _cbot.Symbol.Name && pendingOrder.Label != LabelIdentifier) { continue; }

                if (pendingOrder.TargetPrice > average) { OrdersUpper.Add(pendingOrder); }
                else { OrdersLower.Add(pendingOrder); }
            }
        }

        public void ByTradeType(TradeType tradeTypeToRemove = TradeType.Buy)
        {
            if (tradeTypeToRemove == TradeType.Buy) { RemoveFromList(OrdersBuy); }
            else { RemoveFromList(OrdersSell); }
        }

        public void BySymbol(string? symbol = null, IEnumerable<object>? ordersOrPositions = null)
        {
            ordersOrPositions ??= _cbot.PendingOrders.Cast<object>().Concat(_cbot.Positions.Cast<object>());

            if (!ordersOrPositions.Any()) { return; }

            symbol ??= _cbot.Symbol.Name;

            foreach (var orderOrPosition in ordersOrPositions)
            {
                if (orderOrPosition is PendingOrder pendingOrder && pendingOrder.Symbol.Name == symbol) { _cbot.CancelPendingOrder(pendingOrder); }
                else if (orderOrPosition is Position position && position.Symbol.Name == symbol) { _cbot.ClosePosition(position); }
            }
        }

        public void ByLabelIdentifier(string? labelIdentifier = null, IEnumerable<object>? ordersOrPositions = null)
        {
            ordersOrPositions ??= _cbot.PendingOrders.Cast<object>().Concat(_cbot.Positions.Cast<object>());

            if (!ordersOrPositions.Any()) { return; }

            labelIdentifier ??= LabelIdentifier;

            foreach (var orderOrPosition in ordersOrPositions)
            {
                if (orderOrPosition is PendingOrder pendingOrder && pendingOrder.Label == labelIdentifier) { _cbot.CancelPendingOrder(pendingOrder); }
                else if (orderOrPosition is Position position && position.Label == labelIdentifier) { _cbot.ClosePosition(position); }
            }
        }
    }

}
