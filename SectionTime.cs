using cAlgo.API;

namespace SectionTime
{
    public class UTCZones
    {
        public enum CustomTimeZones
        {
            AppTimeOffset,
            ServerTime,
            Minus12h00m,
            Minus11h30m,
            Minus1100m,
            Minus10h30m,
            Minus1000m,
            Minus9h30m,
            Minus9h00m,
            Minus8h30m,
            Minus8h00m,
            Minus7h30m,
            Minus7h00m,
            Minus6h30m,
            Minus6h00m,
            Minus5h30m,
            Minus5h00m,
            Minus4h30m,
            Minus4h00m,
            Minus3h30m,
            Minus3h00m,
            Minus2h30m,
            Minus2h00m,
            Minus1h30m,
            Minus1h00m,
            Minus0h30m,
            UTC,
            Plus0h30m,
            Plus1h00m,
            Plus1h30m,
            Plus2h00m,
            Plus2h30m,
            Plus3h00m,
            Plus3h30m,
            Plus4h00m,
            Plus4h30m,
            Plus5h00m,
            Plus5h30m,
            Plus6h00m,
            Plus6h30m,
            Plus7h00m,
            Plus7h30m,
            Plus8h00m,
            Plus8h30m,
            Plus9h00m,
            Plus9h30m,
            Plus10h00m,
            Plus10h30m,
            Plus11h00m,
            Plus11h30m,
            Plus12h00m,
            Plus12h30m,
            Plus13h00m,
            Plus13h30m,
            Plus14h00m
        }

        public static TimeSpan ToTimeSpan(CustomTimeZones timeZone, Robot _cbot)
        {
            switch (timeZone)
            {
                case CustomTimeZones.AppTimeOffset:
                    return _cbot.Application.UserTimeOffset;
                case CustomTimeZones.ServerTime:
                    return _cbot.TimeZone.GetUtcOffset(_cbot.Server.TimeInUtc);
                case CustomTimeZones.UTC:
                    return TimeSpan.Zero;
                default:
                    string timeZoneString = timeZone.ToString();

                    string numberPart = timeZoneString.Replace("Minus", "").Replace("Plus", "");

                    string hours = numberPart.Substring(0, numberPart.IndexOf("h"));
                    string minutes = numberPart.Substring(numberPart.IndexOf("h") + 1, numberPart.IndexOf("m") - numberPart.IndexOf("h") - 1);

                    if (int.TryParse(hours, out int h) && int.TryParse(minutes, out int m))
                    {
                        TimeSpan result = new TimeSpan(h, m, 0);

                        if (timeZoneString.StartsWith("Minus"))
                        {
                            result = result.Negate();
                        }

                        return result;
                    }
                    else
                    {
                        throw new ArgumentException("TimeZone invalid.");
                    }
            }
        }
    }
    public class RangeTime
    {
        private readonly Robot _cbot;
        public DateTime StartDateTime { get; private set; }
        public DateTime EndDateTime { get; private set; }
        public DateTime CurrentDatetime { get; private set; }
        public TimeSpan CurrentTimeSpan { get; private set; }
        public int StartHour { get; private set; }
        public int StartMinute { get; private set; }
        public int StartSecond { get; private set; }
        public int EndHour { get; private set; }
        public int EndMinute { get; private set; }
        public int EndSecond { get; private set; }

        public RangeTime(Robot robot, int startHour, int startMinute, int startSecond, int endHour, int endMinute, int endSecond)
        {
            _cbot = robot;
            StartHour = startHour;
            StartMinute = startMinute;
            StartSecond = startSecond;
            EndHour = endHour;
            EndMinute = endMinute;
            EndSecond = endSecond;
            CurrentTimeSpan = TimeSpan.Zero;
        }

        public void AppUserTimeOffsetChanged(UserTimeOffsetChangedEventArgs args)
        {
            if (args == null) { _cbot.Print("UserTimeOffsetChangedEventArgs is null"); return; }
            _cbot.Print("User timezone changed to " + args.UserTimeOffset.ToString());
            UpdateUserTimeOffset();
        }

        public void UpdateUserTimeOffset()
        {
            CurrentTimeSpan = _cbot.Application.UserTimeOffset;
        }

        public void UpdateUserTimeOffset(TimeSpan timeSpan)
        {
            CurrentTimeSpan = timeSpan;
        }

        public void UpdateUserTimeOffset(UTCZones.CustomTimeZones timeZone)
        {
            TimeSpan timeSpan = UTCZones.ToTimeSpan(timeZone, _cbot);
            CurrentTimeSpan = timeSpan;
            _cbot.Print($"User select {timeZone} converted to {timeSpan} with respect to UTC");
        }
        public bool VerifyFormattingTime()
        {
            if (
                (StartHour < 0 && StartHour >= 24) &&
                (StartMinute < 0 && StartMinute >= 60) &&
                (StartSecond < 0 && StartSecond >= 60) &&
                (EndHour < 0 && EndHour >= 24) &&
                (EndMinute < 0 && EndMinute >= 60) &&
                (EndSecond < 0 && EndSecond >= 60)
                ) { return (false); }

            return true;
        }
        public bool VerifyInsideInterval()
        {
            if (CurrentDatetime > StartDateTime && CurrentDatetime < EndDateTime)
            {
                return true;
            }
            return false;
        }

        public bool VerifyInsideInterval(DateTime currentDatetime)
        {
            if (currentDatetime > StartDateTime && currentDatetime < EndDateTime) { return true; }
            return false;
        }
        public bool UpdateDatetimeInterval()
        {
            CurrentDatetime = _cbot.Server.TimeInUtc + CurrentTimeSpan;

            if (!VerifyFormattingTime() || CurrentDatetime == DateTime.MinValue)
            {
                _cbot.Stop();
                return false;
            }

            StartDateTime = new DateTime(
                CurrentDatetime.Year,
                CurrentDatetime.Month,
                CurrentDatetime.Day,
                StartHour,
                StartMinute,
                StartSecond,
                DateTimeKind.Utc
                );

            EndDateTime = new DateTime(
                CurrentDatetime.Year,
                CurrentDatetime.Month,
                CurrentDatetime.Day,
                EndHour,
                EndMinute,
                EndSecond,
                DateTimeKind.Utc
                );

            return true;
        }
    }
}
