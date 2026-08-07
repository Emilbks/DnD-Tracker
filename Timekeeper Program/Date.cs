using System.Text.Json.Serialization;
namespace TimekeeperProgram
{
    public class Date
    {
		public const int WEEKDAYS = 7;
		public const int MONTHDAYS = 28;
		public const int YEARDAYS = 364;

        [JsonInclude]
        public int day { get; private set; }

        public int CalandarDate { get { return day + 1; } }
        public Weekdays weekday { get { return (Weekdays)(day % 7 + 1); } }
        public int month { get { return day / MONTHDAYS % 13; } }
        public int ClandarMonth { get { return month + 1; } }
        public int monthDay { get { return day % MONTHDAYS + 1; } }
        public Months monthName { get { return (Months)(day / MONTHDAYS % 13 + 1); } }
        public int year { get { return day / YEARDAYS; } }

        public Date(int day)
        {
            this.day = day;
        }

		public static string WrittenDate(Date date, bool shorthand = true, bool monthName = false)
		{
			if (shorthand && !monthName) return $"{date.year}/{date.month + 1}/{date.monthDay}";
			if (shorthand && monthName) return $"{date.year}/{date.monthName} {date.monthDay}";
			return $"{date.weekday}, {date.ClandarMonth}/{date.monthDay}, Year {date.year}";
		}

        public void SetDate(int newDate) { this.day = newDate; }

        public void ProgressDate(int days = 1) { this.day += days; }
    }

    public enum Weekdays
    {
        Clearwake = 1,
        Cinderdawn = 2,
        Fogdeep = 3,
        Godwatch = 4,
        Waybound = 5,
        Grimfall = 6,
        Starrest = 7
    }

    public enum Months
    {
        Frostwane = 1,
        Bloomreach = 2,
        Greenspire = 3,
        Suncrest = 4,
        Goldtide = 5,
        Midnightsun = 6,
        Stormcall = 7,
        Mistveil = 8,
        Harvestfall = 9,
        Redleaf = 10,
        Snowfall = 11,
        Evernight = 12,
        Crystalgrowth = 13
    }
}