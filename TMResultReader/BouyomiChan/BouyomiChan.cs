using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net.Sockets;
using System.Diagnostics.Contracts;

namespace BouyomiChan
{
    public class Bouyomi
    {
        private int _port;
        public int Port
        {
            get { return _port; }
            set
            {
                Contract.Requires<ArgumentOutOfRangeException>(value >= 0);
                Contract.Requires<ArgumentOutOfRangeException>(value <= 65535);
                _port = value;
            }
        }

        private string previousMessage;
        public bool SameMessageReadOnce { get; set; }

        public Bouyomi()
        {
            Port = 50001;
            SameMessageReadOnce = false;
        }

        public Bouyomi(int port)
        {
            Contract.Requires<ArgumentOutOfRangeException>(port >= 0);
            Contract.Requires<ArgumentOutOfRangeException>(port <= 65535);
            Port = port;
            SameMessageReadOnce = false;
        }

        /// <summary>
        /// 指定されたメッセージを棒読みちゃんに送る
        /// </summary>
        /// <param name="message">棒読みちゃんに読ませるテキスト</param>
        /// <returns>成功したらtrue/returns>
        public async Task<bool> SpeakAsync(string message)
        {
            Contract.Requires<ArgumentNullException>(message != null);
            Contract.Ensures(Contract.Result<Task<bool>>() != null);
            
            if (SameMessageReadOnce && previousMessage == message) {
                return false;
            }
            previousMessage = message;
            
            return await Task<bool>.Run(() =>
            {
                const short iVoice = 1;
                const short iVolume = -1;
                const short iSpeed = -1;
                const short iTone = -1;
                const short iCommand = 0x0001;
                const byte bCode = 0;
                try {
                    using (var tcpClient = new TcpClient("localhost", Port)) {
                        using (var ns = tcpClient.GetStream()) {
                            using (var bw = new BinaryWriter(ns)) {
                                var bMessage = Encoding.UTF8.GetBytes(message);
                                bw.Write(iCommand); //コマンド（ 0:メッセージ読み上げ）
                                bw.Write(iSpeed); //速度    （-1:棒読みちゃん画面上の設定）
                                bw.Write(iTone); //音程    （-1:棒読みちゃん画面上の設定）
                                bw.Write(iVolume); //音量    （-1:棒読みちゃん画面上の設定）
                                bw.Write(iVoice); //声質    （ 0:棒読みちゃん画面上の設定、1:女性1、2:女性2、3:男性1、4:男性2、5:中性、6:ロボット、7:機械1、8:機械2、10001～:SAPI5）
                                bw.Write(bCode); //文字列のbyte配列の文字コード(0:UTF-8, 1:Unicode, 2:Shift-JIS)
                                bw.Write(bMessage.Length); //文字列のbyte配列の長さ
                                bw.Write(bMessage); //文字列のbyte配列
                            }
                        }
                    }
                } catch (SocketException) {
                    return false;
                }
                return true;
            });
        }

        [ContractInvariantMethod]
        private void ContractInvariant()
        {
            Contract.Invariant(_port >= 0);
            Contract.Invariant(_port <= 65535);
        }
    }
}
