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
            // MemJsonDataテスト用

            string[] jsonStrs = new string[]
            {
                "{ \"key1\" : { \"key1-1\" : \"value1\", \"key1-2\" : \"value2\" }, \"key2\" : { \"key1-1\" : \"value3\", \"key1-2\" : \"value4\" } }",
                "[1,{\"hoge\":1234, \"hoge2\":\"abc\"},3,4,5]",
                "[[[1]]]",
                "12345",
                "\"string1\"",
                "{ \"key\" : \"value\" }",
                "{ \"key\" : value   }",
                "{ \"key1\" : \"value1\", \"key2\" : \"value2\"}",
                "[ 1 , 2 , 3 , 4 , 5 , 6 , 7 , 8 , 9 , 10 ]"
            };

            for (int i = 0; i < jsonStrs.Length; i++)  
            {
                string str = jsonStrs[i];

                Console.WriteLine("*** test" + i + " ***");
                MemJsonData json = MemJsonData.Deserialize(str);
                Console.WriteLine(json.ToString());
            }

            Console.ReadLine();
        }

        static void Main2(string[] args)
        {
            MyArgs myArgs = MyArgs.GetMyArgs(args);
            Console.WriteLine(myArgs);

#if DEBUG
            myArgs.FilePath = @"C:\work\Github\ULoggerCS\Test\InputData\sample04";
            myArgs.FileType = LogFileType.Binary;
            myArgs.IsReadMode = true;
            if (myArgs.FileType == LogFileType.Binary)
            {
                myArgs.FilePath += ".ulgb";
            }
            else
            {
                myArgs.FilePath += ".ulog"; 
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
            Logger logger = new Logger(1000, 100);

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

            //----------------------------------------
            // 詳細なしログを追加
            //----------------------------------------
            // エリアを追加
            logger.AddArea("えりあ１", null);

            // ログを追加
            for (int i = 0; i < 1000; i++)
            {
                logger.AddLogData(1, LogDataType.Single, 1, "てすと１");
            }

            //----------------------------------------
            // 詳細ありのログを追加
            //----------------------------------------
            // エリアを追加
            logger.AddArea("えりあ１－２", "えりあ１");

            for (int i = 0; i < 1000; i++)
            {
                LogDetailTest1 detail = new LogDetailTest1();
                detail.DetailText = "ほげ１２３";
                logger.AddLogData(1, LogDataType.Single, 1, "てすと２", detail);
            }

            //----------------------------------------
            // 詳細ありのログ２を追加
            //----------------------------------------
            logger.AddArea("えりあ１－３", "えりあ１");
            LogDetailTest2 detail2 = new LogDetailTest2();
            detail2.Init();
            
            for (int i = 0; i < 1000; i++)
            {
                logger.AddLogData(1, LogDataType.Single, 1, "てすと３", detail2);
            }

            // バッファに残ったログを書き込み
            logger.WriteBody(true);

            Console.WriteLine("finished!!");
            // ログを表示
            //logger.DebugPrint();
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
