using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.Diagnostics.Contracts;

namespace TMResult
{
    public class GameResult
    {
        /// <summary>
        /// 指定されたIDのゲーム結果を非同期に読み込む
        /// </summary>
        /// <param name="gameID">ゲームID</param>
        /// <returns>成功したときはnull、失敗時はエラーメッセージ</returns>
        public async Task<IEnumerable<IPlayerResult>> LoadAsync(int gameID)
        {
            Contract.Requires<ArgumentOutOfRangeException>(gameID >= 0);
            Contract.Ensures(Contract.Result<Task<IEnumerable<IPlayerResult>>>() != null);

            HttpClient client = new HttpClient();

            string gameResultString;
            try {
                gameResultString = await client.GetStringAsync(
                    string.Format("http://tankmatch.net/result/{0}/{1}.txt", gameID / 1000, gameID));
            } catch (HttpRequestException) {
                return null;
            } catch (TaskCanceledException) {
                return null;
            }

            loadingPlayer = null;
            
            List<PlayerResult> playerResults = new List<PlayerResult>();
            foreach (string line in gameResultString.Split('\n')) {
                var player = loadLine(line);
                if (player != null) {
                    playerResults.Add(player);
                }
            }

            return playerResults;
        }   

        private PlayerResult loadingPlayer;
        private PlayerResult loadLine(string line)
        {
            Contract.Requires<ArgumentNullException>(line != null);

            List<string> columns = new List<string>(line.Split(','));

            if (columns.Count == 0) {
                return null;
            }

            PlayerResult player = null;
            switch (columns[0]) {
                case "UN":
                    if (loadingPlayer != null) {
                        player = loadingPlayer;
                    }
                    loadingPlayer = new PlayerResult();
                    int id;
                    int.TryParse(columns[1], out id);
                    loadingPlayer.ID = id;
                    break;
                case "EF":
                    if (loadingPlayer != null) {
                        player = loadingPlayer;
                    }
                    loadingPlayer = null;
                    break;
                case "NA":
                    loadingPlayer.Name = columns[1];
                    break;
                case "UT":
                    Team team;
                    Enum.TryParse<Team>(columns[1], out team);
                    loadingPlayer.Team = team;
                    break;
                case "LI":
                    loadingPlayer.Leave = columns[1].Equals("2");
                    break;
                case "JT":
                    int joinTime;
                    int.TryParse(columns[1], out joinTime);
                    loadingPlayer.JoinTime = joinTime;
                    break;
                case "LT":
                    int leaveTime;
                    int.TryParse(columns[1], out leaveTime);
                    loadingPlayer.LeaveTime = leaveTime;
                    break;
                case "SH":
                    Contract.Assume(loadingPlayer != null);
                    loadingPlayer.ShotTimes = parseIntegerColumns(columns);
                    break;
                case "HI":
                    Contract.Assume(loadingPlayer != null);
                    loadingPlayer.HitTimes = parseIntegerColumns(columns);
                    break;
                case "KI":
                    Contract.Assume(loadingPlayer != null);
                    loadingPlayer.KillTimes = parseIntegerColumns(columns);
                    break;
                case "DI":
                    Contract.Assume(loadingPlayer != null);
                    loadingPlayer.DieTimes = parseIntegerColumns(columns);
                    break;
                case "SU":
                    Contract.Assume(loadingPlayer != null);
                    loadingPlayer.SuicideTimes = parseIntegerColumns(columns);
                    break;
                case "SA":
                    Contract.Assume(loadingPlayer != null);
                    loadingPlayer.SaveTimes = parseIntegerColumns(columns);
                    break;
                case "TR":
                    Contract.Assume(loadingPlayer != null);
                    loadingPlayer.TryTimes = parseIntegerColumns(columns);
                    break;
                case "HO":
                    Contract.Assume(loadingPlayer != null);
                    loadingPlayer.HomeTimes = parseIntegerColumns(columns);
                    break;
                default:
                    break;
            }
            return player;
        }

        private int[] parseIntegerColumns(IEnumerable<string> columns)
        {
            Contract.Requires<ArgumentNullException>(columns != null);
            Contract.Ensures(Contract.Result<int[]>() != null);
            
            return columns
                .Skip(1)
                .Where(c => c.Length != 0)
                .Select(int.Parse)
                .ToArray();
        }
    }
}
