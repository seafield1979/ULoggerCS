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
            // Loggerを作成
            Logger logger = new Logger();

            // ログを追加
            logger.AddLog(1, LogType.Single, 1, "test1");

            System.Threading.Thread.Sleep(500);

            // 詳細ありのログを追加
            LogDetailTest1 detail = new LogDetailTest1();
            detail.DetailText = "hoge123";
            logger.AddLog(1, LogType.Single, 1, "test2", detail);

            System.Threading.Thread.Sleep(500);

            // 詳細ありのログ２を追加
            LogDetailTest2 detail2 = new LogDetailTest2();
            detail2.Init();
            detail2.setArray1(0, 10);
            logger.AddLog(1, LogType.Single, 1, "test3", detail2);

            System.Threading.Thread.Sleep(500);

            // ログを表示
            logger.DebugPrint();

            // 入力待ち 
            string input1 = Console.ReadLine();

        }
    }

    
}
