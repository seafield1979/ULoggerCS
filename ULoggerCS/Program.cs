using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using ULoggerCS.Utility;

namespace ULoggerCS
{
    class Program
    {
        static void Main(string[] args)
        {
            //TestWrite1();
            TestRead1();

            // 入力待ち 
            string input1 = Console.ReadLine();

        }

        public static void TestWrite1()
        {
            // Loggerを作成
            Logger logger = new Logger();
#if true
            logger.FileType = LogFileType.Text;       // バイナリ形式で保存
            logger.LogFilePath = @".\default.log";
#else
            logger.FileType = LogFileType.Binary;       // バイナリ形式で保存
            logger.LogFilePath = @".\default_log.dat";
#endif

            // ヘッダー情報を追加
            logger.AddLogID(1, "id1", UColor.Black);
            logger.AddLogID(2, "id2", UColor.Black);
            logger.AddLogID(3, "id3", UColor.Black);
            logger.AddLogID(4, "id4", UColor.Black);

            logger.AddLane(1, "lane1", UColor.Black);
            logger.AddLane(2, "lane2", UColor.Black);
            logger.AddLane(3, "lane3", UColor.Black);
            logger.AddLane(4, "lane4", UColor.Black);

            // エリアを追加
            logger.AddArea("area1", null);

            // ログを追加
            for (int i = 0; i < 10; i++)
            {
                logger.AddLogData(1, LogType.Single, 1, "test1");
            }

            System.Threading.Thread.Sleep(500);

            // 詳細ありのログを追加
            for (int i = 0; i < 10; i++)
            {
                LogDetailTest1 detail = new LogDetailTest1();
                detail.DetailText = "hoge123";
                logger.AddLogData(1, LogType.Single, 1, "test2", detail);
            }
            System.Threading.Thread.Sleep(500);

            // 詳細ありのログ２を追加
            LogDetailTest2 detail2 = new LogDetailTest2();
            detail2.Init();
            detail2.setArray1(0, 10);
            logger.AddLogData(1, LogType.Single, 1, "test3", detail2);

            System.Threading.Thread.Sleep(500);

            // ログを表示
            logger.DebugPrint();

            // ファイルに書き込み
            logger.WriteHeader();
            logger.WriteBody();
        }

        public static void TestRead1()
        {
            // Loggerを作成
            LogReader reader = new LogReader();

            reader.ReadLogFile(@".\default.log", LogFileType.Text);

            Console.WriteLine("TestRead1 finished!!");
        }
    }

    
}
