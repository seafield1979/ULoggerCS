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
            MyArgs myArgs = MyArgs.GetMyArgs(args);
            Console.WriteLine(myArgs);

#if DEBUG
            myArgs.FilePath = @"C:\work\Github\ULoggerCS\Test\InputData\sample04";
            myArgs.FileType = LogFileType.Binary;
            myArgs.IsReadMode = true;
            if (myArgs.FileType == LogFileType.Text)
            {
                myArgs.FilePath += ".ulog";
            }
            else
            {
                myArgs.FilePath += ".ulgb";
            }
#endif

            if (myArgs.IsReadMode)
            {
                TestRead1(myArgs.FilePath, myArgs.FileType);
            }
            else { 
                TestWrite1(myArgs.FilePath, myArgs.FileType);
            }
            
            // 入力待ち 
            string input1 = Console.ReadLine();

        }


        /**
         * ログファイルに書き込む
         * 
         * @input outputFilePath: 書き込み先のファイルパス
         * @input fileType: 書き込むファイルの種類(テキスト or バイナリ)
         */
        public static void TestWrite1(string outputFilePath, LogFileType fileType)
        {
            // Loggerを作成
            Logger logger = new Logger(100, 100);

            logger.FileType = fileType;
            logger.LogFilePath = outputFilePath;
            logger.Encoding = Encoding.UTF8;

            //-----------------------------------
            // ヘッダー情報を追加
            //-----------------------------------
            logger.AddLogID(1, "id1", UColor.Black);
            logger.AddLogID(2, "id2", UColor.Black);
            logger.AddLogID(3, "id3", UColor.Black);
            logger.AddLogID(4, "id4", UColor.Black);

            logger.AddLane(1, "れーん１", UColor.Black);
            logger.AddLane(2, "れーん２", UColor.Black);
            logger.AddLane(3, "れーん３", UColor.Black);
            logger.AddLane(4, "れーん４", UColor.Black);

            logger.AddImage("あいこん１", @"C:\work\Github\ULoggerCS\Test\Image\icon1.bmp");
            logger.AddImage("あいこん２", @"C:\work\Github\ULoggerCS\Test\Image\icon2.png");

            // ファイルに書き込み
            // ヘッダ書き込みはプログラム開始時のみ、ログ本体書き込みはバッファがいっぱいになるたびに何度も行われる。
            logger.WriteHeader();

            //-----------------------------------
            // 本体部分を追加
            //-----------------------------------
            // エリアを追加
            logger.AddArea("えりあ１", null);

            // ログを追加
            for (int i = 0; i < 100; i++)
            {
                logger.AddLogData(1, LogDataType.Single, 1, "てすと１");
            }

            System.Threading.Thread.Sleep(500);

            // エリアを追加
            logger.AddArea("えりあ１－２", "えりあ１");

            // 詳細ありのログを追加
            for (int i = 0; i < 100; i++)
            {
                LogDetailTest1 detail = new LogDetailTest1();
                detail.DetailText = "ほげ１２３";
                logger.AddLogData(1, LogDataType.Single, 1, "てすと２", detail);
            }
            System.Threading.Thread.Sleep(500);

            // 詳細ありのログ２を追加
            logger.AddArea("えりあ１－３", "えりあ１");
            LogDetailTest2 detail2 = new LogDetailTest2();
            detail2.Init();
            //detail2.setArray1(0, 10);
            logger.AddLogData(1, LogDataType.Single, 1, "てすと３", detail2);

            System.Threading.Thread.Sleep(500);

            // ログを表示
            logger.DebugPrint();

            // バッファに残ったログを書き込み
            logger.WriteBody(true);
        }

        /**
         * ログファイルを読み込む
         * 
         * @input inputFilePath: 入力ログファイルのパス
         */
        public static void TestRead1(string inputFilePath, LogFileType fileType)
        {
            // Loggerを作成
            LogReader reader = new LogReader();

            // ログファイルをメモリに読み込む
            reader.ReadLogFile(inputFilePath, fileType);

            // ファイルに書き出す
            reader.WriteToFile(@"C:\work\Github\ULoggerCS\Test\OutputData\memdata.txt");

            Console.WriteLine("TestRead1 finished!!");
        }
    }

    
}
