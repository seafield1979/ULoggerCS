﻿using System;
using System.Text;
using ULoggerCS.Utility;
using System.IO;

namespace ULoggerCS
{
    // ログファイルの種類
    enum LogFileType
    {
        Text = 0,
        Binary
    }

    class Logs
    {
        public const int LOG_BLOCK_SIZE = 100;

        public Log[] logs = new Log[LOG_BLOCK_SIZE];

        // Constructor
        public Logs()
        {
            for (int i = 0; i < LOG_BLOCK_SIZE; i++)
            {
                logs[i] = null;
            }
        }
    }

    class LoggerOption
    {
    
    }

    class Logger
    {
        // Consts
        public const int LOG_BUF_SIZE = 2;
        public const int LOG_BLOCK_MAX = 10;

        // Properties
        private LogIDs logIDs;
        private Lanes lanes;
        private IconImages images;

        private int writeBufferNo;      // 書き込みバッファ番号
        private int currentBufferNo;    // 現在のバッファ番号
        private int currentBlockNo;     // 現在のブロック番号
        private int currentPos;         // 現在のブロック内での位置
        private int allocatedBlockNum;  // 確保済みのブロック数
        private LogFileType fileType;   // ログファイルの種類
  
        public LogFileType FileType
        {
            get { return fileType; }
            set { fileType = value; }
        }


        // ダブルバッファなので２つ
        private Logs[,] blocks = new Logs[LOG_BUF_SIZE, LOG_BLOCK_MAX];

        // for Timer
        TimeCountByPerformanceCounter pc;

        // 保存先のログファイルのパス
        private string logFilePath;

        public string LogFilePath
        {
            get { return logFilePath; }
            set { logFilePath = value; }
        }


        // Constructor
        public Logger()
        {
            for (int i = 0; i < LOG_BUF_SIZE; i++)
            {
                for (int j = 0; j < LOG_BLOCK_MAX; j++)
                {
                    this.blocks[i,j] = null;
                }
            }
            // 変数の初期化
            currentBufferNo = 0;
            currentBlockNo = 0;
            currentPos = 0;
            allocatedBlockNum = 0;
            fileType = LogFileType.Text;

            // 最初に２ブロック確保
            for (int i = 0; i < 2; i++)
            {
                AllocateBlock();
            }

            // Init timer
            InitTimer();

            // デフォルトのファイル名
            logFilePath = @".\default.log";

            // レーン情報を登録
            lanes = new Lanes();

            // ログID情報を登録
            logIDs = new LogIDs();
        }

        ~Logger()
        {
            // メモリ開放
            for (int i = 0; i < LOG_BUF_SIZE; i++)
            {
                ClearBuffer(i);
            }
        }

        public bool InitTimer()
        {
            pc = new TimeCountByPerformanceCounter();
            pc.Start();

            return true;
        }

        //
        // 現在の時間を取得する
        // 
        // @output : 現在の時間(s)
        public double GetTime()
        {
            return pc.GetSecTime();
        }

        // レーン情報を追加
        public void AddLane(int id, string name, UInt32 color)
        {
            lanes.Add(id, name, color);
        }

        // ログID情報を追加
        public void AddLogID(int id, string name, UInt32 color, UInt32 frameColor = 0xFF000000)
        {
            logIDs.Add(id, name, color, frameColor);
        }

        /**
         * ログバッファにログを追加する
         * @input log:  追加するLogの子クラスのオブジェクト
         */
        public bool AddLog(Log log)
        {
            // ログを追加先のバッファを探す
            if (blocks[currentBufferNo, currentBlockNo] != null)
            {

                // 現在のブロックに空きがないなら次のブロックへ
                if (currentPos >= Logs.LOG_BLOCK_SIZE)
                {
                    if (currentBlockNo < LOG_BLOCK_MAX)
                    {
                        currentBlockNo++;
                        currentPos = 0;
                    }
                    else
                    {
                        // これ以上のブロックは増やせない
                        return false;
                    }
                    if (allocatedBlockNum < LOG_BLOCK_MAX)
                    {
                        AllocateBlock();
                    }
                }

                // ログを追加
                Logs block = blocks[currentBufferNo, currentBlockNo];
                block.logs[currentPos] = log;

                currentPos++;
            }
            return true;
        }

        // ログID, ログ名, ログ種別, レーンID(or レーン名), ログ詳細(文字列))
        public bool AddLogData(int logId, LogType logType, int laneId, string title)
        {
            string text = null;
            if (title != null)
            {
                text = String.Copy(title);
            }
            return AddLogData(logId, logType, laneId, text, null);
        }

        public bool AddLogData(int logId, LogType logType, int laneId, string title, LogDetail logDetail)
        {
            // ログを追加
            LogData data = new LogData(GetTime(), logId, laneId, logType, title, logDetail);

            AddLog(data);

            return true;
        }

        /**
         * エリア情報を追加
         * @input name: エリア名
         * @input parentName: 親エリア名(このエリアの下に追加される)
         * @input type: エリアの種類
         * @input color: エリアの背景色
         */
        public void AddArea(string name, string parentName, LogAreaType type, UInt32 color = 0xFF000000)
        {
            LogArea log = new LogArea(name, parentName, type, color);
            AddLog(log);
        }

        /**
         * ブロックの追加
         * @input name: ブロック名
         */
        public void AddBlock(string name)
        {
            LogBlock log = new LogBlock(name);
            AddLog(log);
        }
        
        /**
         * ヘッダー部分をファイルに書き込む
         */
        public void WriteHeader()
        {
            if (fileType == LogFileType.Text)
            {
                WriteHeaderText();
            }
            else
            {
                WriteHeaderBin();
            }
        }

        /**
         * テキスト形式のヘッダーを書き込む
         */
        public void WriteHeaderText()
        {
            // ファイルを開く (新規)
            using (StreamWriter sw = new StreamWriter(logFilePath, false, Encoding.UTF8))
            {
                sw.WriteLine("<head>");

                // 書き込み処理
                sw.WriteLine(lanes.ToString());
                sw.WriteLine(logIDs.ToString());

                sw.WriteLine("</head>");
            }
        }

        /**
         * バイナリ形式のヘッダーを書き込む
         */
        public void WriteHeaderBin()
        {
            // 新規
            using (FileStream fs = new FileStream(logFilePath, FileMode.Create, FileAccess.Write))
            {
                // ID情報
                byte[] data = logIDs.ToBinary();
                fs.Write(data, 0, data.Length);

                // レーン情報
                data = lanes.ToBinary();
                fs.Write(data, 0, data.Length);
            }
        }

        /**
         * 本体部分をファイルに書き込む
         */
        public void WriteBody()
        {
            // バッファを切り替え
            writeBufferNo = currentBufferNo;
            currentBufferNo = (currentBufferNo + 1) & 0x1;

            // Todo バックグラウンドで処理する
            if (fileType == LogFileType.Text)
            {
                WriteBodyText();
            }
            else
            {
                WriteBodyBin();
            }
        }

        /**
         * 本体部分をテキスト形式で書き込む
         */
        public void WriteBodyText()
        {
            using (StreamWriter sw = new StreamWriter(logFilePath, true, Encoding.UTF8))
            {
                sw.WriteLine("<body>");

                for (int i = 0; i < LOG_BLOCK_MAX; i++)
                {
                    Logs block = blocks[writeBufferNo, i];

                    if (block != null)
                    {
                        for (int j = 0; j < Logs.LOG_BLOCK_SIZE; j++)
                        {
                            if (block.logs[j] != null)
                            {
                                sw.WriteLine(block.logs[j].ToString());
                            }
                        }
                    }
                }
                sw.WriteLine("</body>");
            }
        }

        /**
         * 本体部分をバイナリ形式で書き込む
         */
        public void WriteBodyBin()
        {
            // 追加
            using (FileStream fs = new FileStream(logFilePath, FileMode.Append, FileAccess.Write))
            {
                for (int i = 0; i < LOG_BLOCK_MAX; i++)
                {
                    Logs block = blocks[writeBufferNo, i];

                    if (block != null)
                    {
                        for (int j = 0; j < Logs.LOG_BLOCK_SIZE; j++)
                        {
                            if (block.logs[j] != null)
                            {
                                byte[] data = block.logs[j].ToBinary();
                                fs.Write(data, 0, data.Length);
                            }
                        }
                    }
                }
            }
        }
        
        // for Debug
        public void DebugPrint()
        {
            if (blocks == null)
            {
                return;
            }

            for (int i = 0; i < LOG_BUF_SIZE; i++)
            {
                for (int j = 0; j < LOG_BLOCK_MAX; j++)
                {
                    Logs block = blocks[i,j];

                    if (block != null)
                    {
                        for (int k = 0; k < Logs.LOG_BLOCK_SIZE; k++)
                        {
                            if (block.logs[k] != null)
                            {
                                Console.WriteLine("[{0}][{1}][{2}] {3}", i, j, k, block.logs[k].ToString());
                            }
                        }
                    }
                }
            }
        }

        // Private methods
        // バッファーをクリアする
        public void ClearBuffer(int bufNo)
        {
            // C#では自前でメモリ開放を行う必要がないため不要
		}

        // ブロックを確保する
        public bool AllocateBlock()
        {
            // 現在のバッファーに空きがあるかを確認
            if (allocatedBlockNum < LOG_BLOCK_MAX)
            {
                // メモリ確保
                blocks[currentBufferNo, allocatedBlockNum] = new Logs();

                allocatedBlockNum++;

                return true;
            }
            return false;
        }
    }
}
