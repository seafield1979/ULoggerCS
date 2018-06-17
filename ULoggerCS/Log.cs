using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ULoggerCS
{
    // 色の定義
    // ログやエリアの色指定に使用する
    class UColor
    {
        public const UInt32 Black = 0xFF000000;
        public const UInt32 Red = 0xFFFF0000;
        public const UInt32 Green = 0xFF00FF00;
        public const UInt32 Blue = 0xFF0000FF;
        public const UInt32 Yellow = 0xFFFFFF00;
        public const UInt32 Gray = 0xFF808080;
    }

    // Type of Log
    // ログの種別
    public enum LogType : byte
    {
        Single = 0, //E_LOG_SINGLE = 0,               // 単体ログ
        AreaStart,  //E_LOG_AREA_ST,                  // 範囲開始
        AreaEnd,    //E_LOG_AREA_END,                 // 範囲終了
        Value       // E_LOG_VALUE                     // 値
    }

    // 抽象クラス
    abstract class Log
    {
        // Constructor
        // 抽象クラスなのでインスタンスは生成できない
        public Log()
        {
        }

        //public abstract string ToString();
        public abstract byte[] ToBinary();
    }

}
