using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ULoggerCS
{
    class LogData : Log
    {
        //
        // Properties
        //
        private double time;            // time of log
        private string text;            // text
        private int logId;              // log id
        private int laneId;             // lane id
        private LogType logType;     // type of log
        private LogDetail detail;		// detail of log

        // Constructor
        public LogData()
        {

        }
        public LogData(double _time, int _logId, int _laneId, LogType _logType, string _text, LogDetail _detail)
        {
            time = _time;
            logId = _logId;
            laneId = _laneId;
            logType = _logType;
            if (_text != null)
            {
                // コピーコンストラクタでコピーを作成
                // ※ログをファイルに出力するまでにコピー元が存在しているとは限らないため
                text = String.Copy(_text);
            }
            if (detail != null)
            {
                detail = _detail.CreateCopy();
            }
        }

        // Finalizer
        ~LogData()
        {
            text = null;
            detail = null;
        }

        public override string toString()
        {
            StringBuilder sb = new StringBuilder();

            // log, type:1, id : 1, lane : 1, text : "hoge1", time : 123.456, data_type : str, data : "テキスト"
            sb.Append(String.Format("log,type:{0},id:{1},lane:{2},time:{3:f10}", logType, logId, laneId, time));
            
            if (text != null)
            {
                sb.Append( String.Format(@",text:""{0}""", text));
            }
            if (detail != null)
            {
                sb.Append( String.Format( ",{0},{{1}}", detail.dataTypeString(), detail.toString()));
            }

            return sb.ToString();
        }
        public override byte[] toBinary()
        {
            return null;
        }
    }
}
