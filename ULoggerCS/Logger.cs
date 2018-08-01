using System;
using System.Text;
using ULoggerCS.Utility;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace ULoggerCS
{
    // ログファイルの種類
    enum LogFileType
    {
        Text = 0,
        Binary
    }

    /**
     * １回のメモリ確保で生成されるログのブロック
     */
    class LogBlock
    {
        //public const int LOG_BLOCK_SIZE = 100;

        public Log[] logs;

        // Constructor
        public LogBlock(int blockSize)
        {
            logs = new Log[blockSize];

            Clear();
        }

        // Clear
        public void Clear()
        {
            for (int i = 0; i < logs.Length; i++)
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
        //
        // Consts
        //
        public const int LOG_BUF_SIZE = 2;      // Dobule buffer
        //public const int LOG_BLOCK_MAX = 10;    // 確保可能なブロック数
        public const string IdentText = "text";    // ファイルの種別判定用文字列(テキスト)
        public const string IdentBin = "data";     // ファイルの種別判定用文字列(バイナリ)

        //
        // Properties
        //
        private LogIDs logIDs;
        private Lanes lanes;
        private IconImages images;
        private int logCnt;

        private int blockSize;          // １ブロックのログ数
        private int blockMax;           // 確保できるブロック最大数

        private int writeBufferNo;      // 書き込みバッファ番号
        private int currentBufferNo;    // 現在のバッファ番号
        private int currentBlockNo;     // 現在のブロック番号
        private int currentPos;         // 現在のブロック内での位置
        private int[] allocatedBlockNum;  // 確保済みのブロック数
        private LogFileType fileType;   // ログファイルの種類
        private Encoding encoding;      // 文字列のエンコーディングタイプ

        private bool isWriting = false;         // ファイルに書き込み中にtrueになる
        private bool isFirstBody = true;       // 最初に本体部分を書き込んだら false になる
        
        public LogFileType FileType
        {
            get { return fileType; }
            set { fileType = value; }
        }

        public Encoding Encoding
        {
            get { return encoding; }
            set { encoding = value; }
        }

        // ダブルバッファなので２つ
        private LogBlock[,] blocks;

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
        public Logger(int blockSize = 100, int blockMax = 100)
        {
            this.blockSize = blockSize;
            this.blockMax = blockMax;

            blocks = new LogBlock[LOG_BUF_SIZE, blockMax];

            for (int i = 0; i < LOG_BUF_SIZE; i++)
            {
                for (int j = 0; j < blockMax; j++)
                {
                    this.blocks[i,j] = null;
                }
            }

            // 変数の初期化
            logCnt = 0;
            currentBufferNo = 0;
            currentBlockNo = 0;
            currentPos = 0;
            allocatedBlockNum = new int[LOG_BUF_SIZE];
            fileType = LogFileType.Text;
            encoding = Encoding.UTF8;       // デフォルトはUTF8

            // 最初に２ブロック確保
            for (int i = 0; i < 2; i++)
            {
                AllocateBlock();
            }

            // Init timer
            InitTimer();

            // デフォルトのファイル名
            logFilePath = @".\default.ulog";

            // レーン情報を登録
            lanes = new Lanes();

            // ログID情報を登録
            logIDs = new LogIDs();

            images = new IconImages();
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
        public void AddLane(UInt32 id, string name, UInt32 color)
        {
            lanes.Add(id, name, color);
        }

        // ログID情報を追加
        public void AddLogID(UInt32 id, string name, UInt32 color, UInt32 frameColor = 0xFF000000)
        {
            logIDs.Add(id, name, color, frameColor);
        }

        // アイコン画像を追加
        public void AddImage(string name, string imagePath)
        {
            images.Add(name, imagePath);
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
                if (currentPos >= blockSize)
                {
                    if (currentBlockNo < blockMax - 1)
                    {
                        currentBlockNo++;
                        currentPos = 0;
                        if (allocatedBlockNum[currentBufferNo] < blockMax)
                        {
                            AllocateBlock();
                        }
                    }
                    else
                    {
                        // これ以上のブロックは増やせないのでファイルに書き込み
                        WriteBody();
                    }
                }
                // ログを追加
                LogBlock block = blocks[currentBufferNo, currentBlockNo];
                block.logs[currentPos] = log;

                logCnt++;
                currentPos++;
            }
            return true;
        }

        // ログID, ログ名, ログ種別, レーンID(or レーン名), ログ詳細(文字列))
        public bool AddLogData(UInt32 logId, LogDataType logType, UInt32 laneId, string title)
        {
            string text = null;
            if (title != null)
            {
                text = String.Copy(title);
            }
            return AddLogData(logId, logType, laneId, text, null);
        }

        public bool AddLogData(UInt32 logId, LogDataType logType, UInt32 laneId, string title, LogDetail logDetail)
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
        public void AddArea(string name, string parentName, UInt32 color = 0xFF000000)
        {
            LogArea log = new LogArea(name, parentName, color);
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
            using (FileStream fs = new FileStream(logFilePath, FileMode.Create))
            {
                // ファイルの種別
                byte[] buf = encoding.GetBytes(IdentText);
                fs.Write(buf, 0, buf.Length);
            }

            using (StreamWriter sw = new StreamWriter(logFilePath, true, encoding))
            {
                sw.WriteLine("");
                sw.WriteLine("<head>");
                sw.WriteLine("encoding:{0}", UUtility.GetEncodingStr(encoding));

                // レーン
                lanes.WriteToTextFile(sw);

                // ログID
                logIDs.WriteToTextFile(sw);

                // アイコンイメージ
                images.WriteToTextFile(sw);

                sw.WriteLine("</head>");
            }
        }

        /**
         * バイナリ形式のヘッダーを書き込む
         */
        public void WriteHeaderBin()
        {
            // 新規
            using (UFileStream fs = new UFileStream(logFilePath, FileMode.Create, FileAccess.Write))
            {
                // ファイルの種別
                fs.WriteString(IdentBin);

                // エンコードの長さ
                // エンコード
                fs.WriteSizeString(UUtility.GetEncodingStr(encoding), Encoding.UTF8);

                // ID情報
                logIDs.WriteToBinFile(fs);

                // レーン情報
                lanes.WriteToBinFile(fs);

                // Icon image
                images.WriteToBinFile(fs);
            }
        }

        /**
         * 本体部分をファイルに書き込む
         */
        public async void WriteBody(bool isLast = false)
        {
#if DEBUG
            //DebugPrint();
#endif
            // 前回の書き込み完了を待つ
            if (isWriting) {
                Console.WriteLine("waiting for file writing.");
                if (isWriting)
                {
                    Thread.Sleep(500);
                    Console.Write(".");
                }
            }

            isWriting = true;

            // バッファを切り替え
            writeBufferNo = currentBufferNo;
            currentBufferNo = (currentBufferNo + 1) & 0x1;
            currentPos = 0;
            currentBlockNo = 0;

            // 切り替え先のバッファにブロックを確保する
            if (allocatedBlockNum[currentBufferNo] == 0)
            {
                AllocateBlock();
            }

            // バックグラウンドで処理する
            Task<int> task = Task.Run<int>(() => {
                if (fileType == LogFileType.Text)
                {
                    WriteBodyText(isLast);
                }
                else
                {
                    WriteBodyBin();
                }
                // 書き込み完了したのでバッファをクリアする
                // ただし、メモリに確保した Logs は残しておく。
                for (int i = 0; i < blockMax; i++)
                {
                    LogBlock _block = blocks[writeBufferNo, i];
                    if (_block != null)
                    {
                        _block.Clear();
                    }
                }
                isWriting = false;
                return 0;    
            });
            int result = await task;
        }

        /**
         * 本体部分をテキスト形式で書き込む
         */
            public void WriteBodyText(bool isLast)
        {
            using (StreamWriter sw = new StreamWriter(logFilePath, true, Encoding.UTF8))
            {
                if (isFirstBody)
                {
                    sw.WriteLine("<body>");
                    isFirstBody = false;
                }

                for (int i = 0; i < blockMax; i++)
                {
                    LogBlock block = blocks[writeBufferNo, i];

                    if (block != null)
                    {
                        for (int j = 0; j < blockSize; j++)
                        {
                            if (block.logs[j] != null)
                            {
                                sw.WriteLine(block.logs[j].ToString());
                            }
                        }
                    }
                }
                if (isLast)
                {
                    sw.WriteLine("</body>");
                }
            }
        }

        /**
         * 本体部分をバイナリ形式で書き込む
         */
        public void WriteBodyBin()
        {
            // 追加
            using (UFileStream fs = new UFileStream(logFilePath, FileMode.Append, FileAccess.Write))
            {
                // ログ件数
                fs.WriteInt32(GetLogNum());
                
                for (int i = 0; i < blockMax; i++)
                {
                    LogBlock block = blocks[writeBufferNo, i];

                    if (block != null)
                    {
                        for (int j = 0; j < blockSize; j++)
                        {
                            if (block.logs[j] != null)
                            {
                                block.logs[j].WriteToBinFile(fs, Encoding.UTF8);
                            }
                        }
                    }
                }
            }
        }

        /**
         * バッファに積まれたログの件数を取得する
         * @output : バッファに積まれたログ件数
         */
        public int GetLogNum()
        {
            int num = 0;

            for (int i = 0; i < blockMax; i++)
            {
                LogBlock block = blocks[writeBufferNo, i];

                if (block != null)
                {
                    for (int j = 0; j < blockSize; j++)
                    {
                        if (block.logs[j] != null)
                        {
                            num++;
                        }
                    }
                }
            }

            return num;
        }
        
        // for Debug
        public void DebugPrint()
        {
            if (blocks == null)
            {
                return;
            }

            for (int j = 0; j < blockMax; j++)
            {
                LogBlock block = blocks[currentBufferNo,j];

                if (block != null)
                {
                    for (int k = 0; k < blockSize; k++)
                    {
                        if (block.logs[k] != null)
                        {
                            Console.WriteLine("[{0}][{1}][{2}] {3}", currentBufferNo, j, k, block.logs[k].ToString());
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
            if (allocatedBlockNum[currentBufferNo] < blockMax)
            {
                // メモリ確保
                blocks[currentBufferNo, allocatedBlockNum[currentBufferNo]] = new LogBlock(blockSize);

                allocatedBlockNum[currentBufferNo]++;

                return true;
            }
            return false;
        }
    }
}
