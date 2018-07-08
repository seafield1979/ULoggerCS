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
            // JsonDataテスト用
#if true
            // JsonDataにJson出力用のデータを作成する
            //--------------------------
            // string
            //--------------------------
            Console.WriteLine("*** test1 ***");
            JsonData2 json1 = new JsonData2();
            json1.Add("hoge");
            Console.WriteLine(json1.ToString());

            //--------------------------
            // array
            //--------------------------
            Console.WriteLine("*** test2 ***");
            JsonData2 json2 = new JsonData2();
            object[] array1 = new object[] { 1, 2, 3, 4, 5 };
            json2.Add(array1);
            Console.WriteLine(json2.ToString());

            //--------------------------
            // dictionary
            //--------------------------
            Console.WriteLine("*** test3 ***");
            JsonData2 json3 = new JsonData2();
            var dic1 = new Dictionary<string, object>();
            dic1["key1"] = 1;
            dic1["key2"] = "test2";
            dic1["key3"] = 3;
            json3.Add(dic1);
            Console.WriteLine(json3);


            Console.WriteLine("*** test4 ***");
            JsonData2 json4 = new JsonData2();
            JsonData2 json42 = new JsonData2();
            JsonData2 json43 = new JsonData2();

            var dic2 = new Dictionary<string, object>();
            dic2["key1"] = "hoge";
            dic2["key2"] = "hoge2";
            json42.Add(dic2);
            json43.Add(array1);

            var dic3 = new Dictionary<string, object>();
            dic3["key1"] = 1;
            dic3["key2"] = "test2";
            dic3["key3"] = 3;
            dic3["key4"] = json42;
            dic3["key5"] = json43;
            json3.Add(dic3);
            Console.WriteLine(json3);


            Console.WriteLine("*** test5 ***");
            JsonData2 json5 = new JsonData2();
            json5.Add(dic1);
            array1[2] = json5;
            Console.WriteLine(json2);

#endif


            // MemJsonDataテスト用
#if false
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
                MemJsonData2 json = MemJsonData2.Deserialize(str);
                Console.WriteLine(json.ToString());
            }
#endif
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
