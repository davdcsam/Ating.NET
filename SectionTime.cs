using cAlgo.API;

namespace SectionTime
{
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
