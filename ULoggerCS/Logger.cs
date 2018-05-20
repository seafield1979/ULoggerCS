using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ULoggerCS.Utility;

namespace ULoggerCS
{

    class Logs
    {
        public const int LOG_BLOCK_SIZE = 10;

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

    class Logger
    {
        // Consts
        public const int LOG_BUF_SIZE = 2;
        public const int LOG_BLOCK_MAX = 10;

        // Properties
        private LogIDs logIDs;
        private Lanes lanes;
        private IconImages images;

        private int currentBuffer;      // 現在のバッファ番号
        private int currentBlock;       // 現在のブロック番号
        private int currentPos;         // 現在のブロック内での位置
        private int allocatedBlockNum;  // 確保済みのブロック数

        // ダブルバッファなので２つ
        private Logs[,] blocks = new Logs[LOG_BUF_SIZE, LOG_BLOCK_MAX];

        // for Timer
        TimeCountByPerformanceCounter pc;


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
            currentBuffer = 0;
            currentBlock = 0;
            currentPos = 0;
            allocatedBlockNum = 0;

            // 最初に２ブロック確保
            for (int i = 0; i < 2; i++)
            {
                AllocateBlock();
            }

            // Init timer
            InitTimer();
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



        // ログID, ログ名, ログ種別, レーンID(or レーン名), ログ詳細(文字列))
        public bool AddLog(int logId, LogType logType, int laneId, string title)
        {
            string text = null;
            if (title != null)
            {
                text = String.Copy(title);
            }
            return AddLog(logId, logType, laneId, text, null);
        }
        public bool AddLog(int logId, LogType logType, int laneId, string title, LogDetail logDetail)
        {
            // ログを追加先のバッファを探す
            if (blocks[currentBuffer,currentBlock] != null)
            {

                // 現在のブロックに空きがないなら次のブロックへ
                if (currentPos >= Logs.LOG_BLOCK_SIZE)
                {
                    if (currentBlock < LOG_BLOCK_MAX)
                    {
                        currentBlock++;
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
                LogData data = new LogData(GetTime(), logId, laneId, logType, title, logDetail);

                Logs block = blocks[currentBuffer, currentBlock];
                block.logs[currentPos] = data;

                currentPos++;
            }
            return true;
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
                                Console.WriteLine("[{0}][{1}][{2}] {3}", i, j, k, block.logs[k].toString());
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
                blocks[currentBuffer, allocatedBlockNum] = new Logs();

                allocatedBlockNum++;

                return true;
            }
            return false;
        }
    }
}
