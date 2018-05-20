using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ULoggerCS
{
    // Type of Log
    // ログの種別
    public enum LogType
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

        public abstract string toString();
        public abstract byte[] toBinary();
    }

}
