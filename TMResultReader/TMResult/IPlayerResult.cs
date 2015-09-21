using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Diagnostics.Contracts;

namespace TMResult
{
    public interface IPlayerResult
    {
        int ID { get; }
        string Name { get; }
        Team Team { get; }
        bool Leave { get; }
        int JoinTime { get; }
        int LeaveTime { get; }
        IEnumerable<int> ShotTimes { get; }
        IEnumerable<int> HitTimes { get; }
        IEnumerable<int> KillTimes { get; }
        IEnumerable<int> DieTimes { get; }
        IEnumerable<int> SuicideTimes { get; }
        IEnumerable<int> SaveTimes { get; }
        IEnumerable<int> TryTimes { get; }
        IEnumerable<int> HomeTimes { get; }
    }

    public static class IPlayerResultExtensions
    {
        private static Regex rawNameRegex = new Regex(@"^(.+) \([A-Z]{2}\)$", RegexOptions.Compiled);
        
        public static string RawName(this IPlayerResult player)
        {
            Contract.Requires<ArgumentNullException>(player != null);
            Contract.Ensures(Contract.Result<string>() != null);
            
            if (player.Name == null) {
                return "";
            }

            string name = player.Name;
            var match = rawNameRegex.Match(name);
            if (match.Success) {
                 name = match.Groups[1].ToString();
            }

            return name;
        }
        
        public static int ShotCount(this IPlayerResult player)
        {
            Contract.Requires<ArgumentNullException>(player != null);
            if (player.ShotTimes == null) {
                return 0;
            }
            return player.ShotTimes.Count();
        }

        public static int HitCount(this IPlayerResult player)
        {
            Contract.Requires<ArgumentNullException>(player != null);
            if (player.HitTimes == null) {
                return 0;
            }
            return player.HitTimes.Count();
        }

        public static int KillCount(this IPlayerResult player)
        {
            Contract.Requires<ArgumentNullException>(player != null);
            if (player.KillTimes == null) {
                return 0;
            }
            return player.KillTimes.Count();
        }

        public static int DieCount(this IPlayerResult player)
        {
            Contract.Requires<ArgumentNullException>(player != null);
            if (player.DieTimes == null) {
                return 0;
            }
            return player.DieTimes.Count();
        }

        public static int SuicideCount(this IPlayerResult player)
        {
            Contract.Requires<ArgumentNullException>(player != null);
            if (player.SuicideTimes == null) {
                return 0;
            }
            return player.SuicideTimes.Count();
        }

        public static int SaveCount(this IPlayerResult player)
        {
            Contract.Requires<ArgumentNullException>(player != null);
            if (player.SaveTimes == null) {
                return 0;
            }
            return player.SaveTimes.Count();
        }

        public static int TryCount(this IPlayerResult player)
        {
            Contract.Requires<ArgumentNullException>(player != null);
            if (player.TryTimes == null) {
                return 0;
            }
            return player.TryTimes.Count();
        }

        public static int HomeCount(this IPlayerResult player)
        {
            Contract.Requires<ArgumentNullException>(player != null);
            if (player.HomeTimes == null) {
                return 0;
            }
            return player.HomeTimes.Count();
        }

        public static double HitRate(this IPlayerResult player)
        {
            Contract.Requires<ArgumentNullException>(player != null);
            if (player.HitTimes == null || player.ShotTimes == null) {
                return 0;
            }
            return Math.Round(((double)player.HitTimes.Count() / (double)player.ShotTimes.Count() * 100), 1);
        }

        public static int Dora(this IPlayerResult player)
        {
            Contract.Requires<ArgumentNullException>(player != null);
            if (player.KillTimes == null || player.DieTimes == null) {
                return 0;
            }
            return player.KillTimes.Count() - player.DieTimes.Count();
        }
    }
}
