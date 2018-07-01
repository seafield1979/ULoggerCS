using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ULoggerCS.Utility;

/**
 * ログデータ(1件１件の表示されるログ情報)
 */
namespace ULoggerCS
{
    /**
     * ULogViewで画面に表示されるログデータ
     */
    class LogData : Log
    {
        //
        // Properties
        //
        private UInt32 logId;           // log id
        private LogDataType logType;    // type of log data
        private UInt32 laneId;          // lane id
        private string text;            // text
        private double time;            // time of log
        private LogDetail detail;		// detail of log

        // Constructor
        public LogData()
        {
        }

        public LogData(double time, UInt32 logId, UInt32 laneId, LogDataType logType, string text, LogDetail detail)
        {
            this.time = time;
            this.logId = logId;
            this.laneId = laneId;
            this.logType = logType;
            if (this.text != null)
            {
                // コピーコンストラクタでコピーを作成
                // ※ログをファイルに出力するまでにコピー元が存在しているとは限らないため
                this.text = String.Copy(text);
            }
            if (detail != null)
            {
                this.detail = detail.CreateCopy();
            }
        }

        /**
         * テキスト形式のログファイルに出力する文字列
         */
        public override string ToString()
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
                sb.Append( String.Format( @",detail_type:{0},detail:""{1}""", detail.dataTypeString(), detail.ToString()));
            }

            return sb.ToString();
        }

        /**
         * バイナリ形式のデータを取得する
         */
        public override byte[] ToBinary()
        {
            List<byte> data = new List<byte>(1000);

            // データログ
            data.Add((byte)LogType.Data);
            // ログID
            data.AddRange(BitConverter.GetBytes(logId));
            // ログタイプ
            data.Add((byte)logType);
            // 表示レーンID
            data.AddRange(BitConverter.GetBytes(laneId));

            if (text == null)
            {
                //タイトルの長さ
                data.AddRange(BitConverter.GetBytes((UInt32)0));
            }
            else {
                //タイトルの長さ
                byte[] textData = System.Text.Encoding.UTF8.GetBytes(text);
                data.AddRange(BitConverter.GetBytes(textData.Length));
                //タイトル
                data.AddRange(textData);
            }
            //時間
            data.AddRange(BitConverter.GetBytes(time));
            if (detail == null)
            {
                // ログデータ(詳細)の種類
                data.Add((byte)DetailDataType.None);
            }
            else
            {
                // ログデータ(詳細)の種類
                data.Add(detail.dataTypeByte());
                // ログデータ(詳細)のサイズ
                byte[] detailData = detail.ToBinary();
                data.AddRange(BitConverter.GetBytes(detailData.Length));
                // ログデータ(詳細)
                data.AddRange(detailData);
            }

            return data.ToArray();
        }

        /**
         * バイナリ形式のログファイルにログを出力する
         * 
         * @output fs: 書き込み先のファイルオブジェクト
         * @output encoding: 文字列のエンコードタイプ
         */
        public override void WriteToBinFile(UFileStream fs, Encoding encoding)
        {
            // データログ
            fs.WriteByte((byte)LogType.Data);
            // ログID
            fs.WriteUInt32(logId);
            // ログタイプ
            fs.WriteByte((byte)logType);
            // 表示レーンID
            fs.WriteUInt32(laneId);

            if (text == null)
            {
                //タイトルの長さ
                fs.WriteUInt32((UInt32)0);
            }
            else
            {
                //タイトル
                fs.WriteSizeString(text, encoding);
            }
            //時間
            fs.WriteDouble(time);
            if (detail == null)
            {
                // ログデータ(詳細)の種類
                fs.WriteByte((byte)DetailDataType.None);
            }
            else
            {
                // ログデータ(詳細)の種類
                fs.WriteByte(detail.dataTypeByte());
                // ログデータ(詳細)
                fs.WriteSizeString(detail.ToString(), encoding);
            }

        }
    }
}
