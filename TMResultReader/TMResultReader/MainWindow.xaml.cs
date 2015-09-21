using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Diagnostics.Contracts;
using TMResult;
using BouyomiChan;

namespace TMResultReader
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly Bouyomi bouyomi = new Bouyomi();
        private bool reading = false;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void latestReadMenuItem_Click(object sender, RoutedEventArgs e)
        {
            readAsync();
        }

        private void readMenuItem_Click(object sender, RoutedEventArgs e)
        {
            gameIDTextBox.Text = gameIDTextBox.Text.Trim();

            if (gameIDTextBox.Text.Length == 0) {
                setStatusMessage("ゲームIDが入力されていません");
                return;
            }
            
            int gameID;
            if (!int.TryParse(gameIDTextBox.Text, out gameID) || gameID < 0) {
                setStatusMessage("入力されたゲームIDは不正です");
                return;
            }
            
            readAsync(gameID);
        }

        /// <summary>
        /// 最新のゲームIDを取得してそのリザルトを読む
        /// </summary>
        private async void readAsync()
        {
            if (reading) {
                return;
            }
            
            TMResult.Infomation info = new Infomation();
            var IDs = await info.GetLatestGameIDAsync();
            if (IDs == null || !IDs.Any()) {
                setStatusMessage("ゲームIDの自動取得に失敗しました");
                return;
            }
            int latestID = IDs.First();
            readAsync(latestID);
        }

        /// <summary>
        /// 指定されたゲームIDのリザルトを読む
        /// </summary>
        /// <param name="gameID">ゲームID</param>
        private async void readAsync(int gameID)
        {
            Contract.Requires<ArgumentOutOfRangeException>(gameID >= 0);

            if (reading) {
                return;
            }
            reading = true;

            gameIDTextBox.Text = gameID.ToString();
            
            setStatusMessage(string.Format("リザルトを読み込み中です。(ID:{0})", gameID.ToString()));

            GameResult result = new GameResult();
            var players = await result.LoadAsync(gameID);
            
            if (players == null) {
                setStatusMessage(string.Format("リザルトの読み込みに失敗しました。(ID:{0})", gameID.ToString()));
                reading = false;
                return;
            }

            var resultText = await createResultStringAsync(players);
            resultTextBox.Text = resultText;
            
            bool success = await bouyomi.SpeakAsync(resultText);
            if (success) {
                setStatusMessage(string.Format("リザルトの読み込みが完了しました。(ID:{0})", gameID.ToString()));
            } else {
                setStatusMessage(string.Format("棒読みちゃんに接続できません。(ID:{0})", gameID.ToString()));
            }

            reading = false;
        }
        
        private async Task<string> createResultStringAsync(IEnumerable<IPlayerResult> players)
        {
            Contract.Requires<ArgumentNullException>(players != null);
            Contract.Ensures(Contract.Result<string>() != null);

            return await Task.Run(() =>
            {
                var resultText = new StringBuilder();
                createResultLine("ヒット数 第1位 {0}ヒット ", resultText, players, IPlayerResultExtensions.HitCount);
                createResultLine("ショット数 第1位 {0}ショット ", resultText, players, IPlayerResultExtensions.ShotCount);
                createResultLine("ヒット率 第1位 {0}パーセント ", resultText, players, IPlayerResultExtensions.HitRate);
                createResultLine("キル数 第1位 {0}キル ", resultText, players, IPlayerResultExtensions.KillCount);
                createResultLine("ダイ数 第1位 {0}ダイ ", resultText, players, IPlayerResultExtensions.DieCount);
                createResultLine("フラッグ数 第1位 {0}フラッグ ", resultText, players, IPlayerResultExtensions.HomeCount);
                createResultLine("トライ数 第1位 {0}トライ ", resultText, players, IPlayerResultExtensions.TryCount);
                createResultLine("セーブ数 第1位 {0}セーブ ", resultText, players, IPlayerResultExtensions.SaveCount);
                createResultLine("ドラ 第1位 {0}ドラ ", resultText, players, IPlayerResultExtensions.Dora);
                return resultText.ToString();
            });
        }

        private void createResultLine<T>(string format, StringBuilder resultText, IEnumerable<IPlayerResult> players, Func<IPlayerResult, T> selector)
            where T : IComparable<T>
        {
            Contract.Requires<ArgumentNullException>(format != null);
            Contract.Requires<ArgumentNullException>(resultText != null);
            Contract.Requires<ArgumentNullException>(players != null);
            Contract.Requires<ArgumentNullException>(selector != null);

            IEnumerable<IPlayerResult> selectedPlayers = players.MaxElements(selector);
            if (selectedPlayers.Count() == 0) {
                return;
            }

            resultText.Append(string.Format(format, selector(selectedPlayers.First())));
            foreach (var player in selectedPlayers) {
                resultText.Append(player.RawName());
                resultText.Append(" ");
            }
            resultText.Append(Environment.NewLine);
        }

        private void setStatusMessage(string message)
        {
            Contract.Requires<ArgumentNullException>(message != null);
            statusBarText.Text = message;
        }
    }
}
