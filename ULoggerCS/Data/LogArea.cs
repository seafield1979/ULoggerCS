using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ULoggerCS.Utility;

namespace ULoggerCS
{
    /*
     * エリアタイプのログ
     * エリアログに続くログは、エリアの中に入る
     * area,name:"エリア1",parent:"root"
     * area,name:"エリア2",parent:"エリア1",color:#00ff00
     */
    class LogArea : Log
    {
        //
        // Properties
        //
        private string name;

        // Area name
        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        // Parent area name
        private string parentName;

        public string ParentName
        {
            get { return parentName; }
            set { parentName = value; }
        }

        // Area color
        private UInt32 color;

        public UInt32 Color
        {
            get { return color; }
            set { color = value; }
        }


        //
        // Constructor
        //
        public LogArea(string name, string parentName, UInt32 color = 0xFF000000)
        {
            this.name = name;
            if (parentName == null || parentName.Length == 0)
            {
                parentName = "root";
            }
            this.parentName = parentName;
            this.color = color;
        }

        /**
         * テキスト形式のログファイルに書き込む文字列
         * 
         * 例: area,name: "エリア1",parent: "root",color:FF112233
         */
        override public string ToString()
        {
            return String.Format("area,name:\"{0}\",parent:\"{1}\",color:{2:X8}", name, parentName, color);
        }

        /**
         * バイナリ形式のログファイルに保存する形式のbyte配列を返す
         */
        override public byte[] ToBinary()
        {
            List<byte> data = new List<byte>(1000);

            // Type of log (Area)
            data.Add((byte)LogType.Area);

            // Length of area name
            byte[] nameData = Encoding.UTF8.GetBytes(name);
            data.AddRange(BitConverter.GetBytes(nameData.Length));

            // Area name
            data.AddRange(nameData);

            // length of parent area name
            byte[] parentNameData = Encoding.UTF8.GetBytes(parentName);
            data.AddRange(BitConverter.GetBytes(parentNameData.Length));

            // parent area name
            data.AddRange(parentNameData);

            // color
            data.AddRange(BitConverter.GetBytes(color));

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
            // Type of log (Area)
            fs.WriteByte((byte)LogType.Area);

            // Length of area name
            // Area name
            fs.WriteSizeString(name, encoding);
            
            // parent area name

            fs.WriteSizeString(parentName, encoding);
            
            // color
            fs.WriteUInt32(color);
        }

    }
}
