using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics.Contracts;

namespace TMResult
{
    internal class PlayerResult : IPlayerResult
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public Team Team { get; set; }
        public bool Leave { get; set; }
        public int JoinTime { get; set; }
        public int LeaveTime { get; set; }
        public IEnumerable<int> ShotTimes { get; set; }
        public IEnumerable<int> HitTimes { get; set; }
        public IEnumerable<int> KillTimes { get; set; }
        public IEnumerable<int> DieTimes { get; set; }
        public IEnumerable<int> SuicideTimes { get; set; }
        public IEnumerable<int> SaveTimes { get; set; }
        public IEnumerable<int> TryTimes { get; set; }
        public IEnumerable<int> HomeTimes { get; set; }

        public override string ToString()
        {
            Contract.Ensures(Contract.Result<string>() != null);
            return Name ?? "(NO_NAME)";
        }
    }
}
