using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ULoggerCS
{
    /*
     * area,name:"エリア1",parent:"root",type:"dir"
     * area,name:"エリア2",parent:"エリア1",type:"data",color:#00ff00
     */
    enum LogAreaType
    {
        Dir,        // 配下に子エリアを持てる。DataやDirタイプの親になる。
        Data,       // 配下にログを持てるエリア
    }

    class LogArea : Log
    {
        //
        // Properties
        //
        private string name;

        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        private string parentName;

        public string ParentName
        {
            get { return parentName; }
            set { parentName = value; }
        }

        private LogAreaType type;

        public LogAreaType Type
        {
            get { return type; }
            set { type = value; }
        }

        private UInt32 color;

        public UInt32 Color
        {
            get { return color; }
            set { color = value; }
        }


        //
        // Constructor
        //
        public LogArea(string name, string parentName, LogAreaType type, UInt32 color = 0xFF000000)
        {
            this.name = name;
            if (parentName == null || parentName.Length == 0)
            {
                parentName = "root";
            }
            this.parentName = parentName;
            this.type = type;
            this.color = color;
        }

        override public string ToString()
        {
            // area,name: "エリア1",parent: "root",type: "dir"
            string typeStr = (type == LogAreaType.Dir) ? "dir" : "data";
            return String.Format("area,name:\"{0}\",parent:\"{1}\",type:\"{2}\",color={3:X8}", name, parentName, typeStr, color);
        }


        override public byte[] ToBinary()
        {
            List<byte> data = new List<byte>(1000);

            // エリア名の長さ
            byte[] nameData = Encoding.UTF8.GetBytes(name);
            data.AddRange(BitConverter.GetBytes(nameData.Length));

            // エリア名
            data.AddRange(nameData);

            // 親のエリア名の長さ
            byte[] parentNameData = Encoding.UTF8.GetBytes(parentName);
            data.AddRange(BitConverter.GetBytes(parentNameData.Length));

            // 親のエリア名
            data.AddRange(parentNameData);

            // 種別
            data.Add((byte)type);

            // 色
            data.AddRange(BitConverter.GetBytes(color));

            return data.ToArray();
            return null;
        }
    }
}
