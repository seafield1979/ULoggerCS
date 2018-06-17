using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ULoggerCS
{
    /*
     * <logid>
	 	 id:1,name:"名前1",color:00ff00,blink:off
		 id:2,name:"名前2",color:rgb(100,100,20),blink:on
		 id:3,name:"名前2",image:"image1"
	   </logid>
    */
    class LogID : Log
    {
        private UInt32 id;

        public UInt32 ID
        {
            get { return id; }
            set { id = value; }
        }

        private string name;

        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        private UInt32 color;

        public UInt32 Color
        {
            get { return color; }
            set { color = value; }
        }

        private UInt32 frameColor;

        public UInt32 FrameColor
        {
            get { return frameColor; }
            set { frameColor = value; }
        }


        private string imageName;

        public string ImageName
        {
            get { return imageName; }
            set { imageName = value; }
        }

        //
        // Constructor
        //
        public LogID()
        {
            id = 0;
            name = null;
            color = 0;
            frameColor = 0;
            imageName = null;
        }

        public LogID(UInt32 id, string name, UInt32 color, UInt32 frameColor = 0xFF000000, string image = null)
        {
            this.id = id;
            this.name = name;
            this.color = color;
            this.frameColor = color;
            this.imageName = image;
        }

        //
        // Methods
        // 
        override public string ToString()
        {
            return string.Format(@"id:{0},name:""{1}"",color:{2:X8}", id, name, color);
        }

        override public byte[] ToBinary()
        {
            List<byte> data = new List<byte>(1000);

            // ID名の長さ
            byte[] nameData = Encoding.UTF8.GetBytes(name);
            data.AddRange(BitConverter.GetBytes(nameData.Length));

            // ID名
            data.AddRange(nameData);

            // 色
            data.AddRange(BitConverter.GetBytes(color));

            // アイコン画像名の長さ
            // アイコン画像名
            if (imageName == null)
            {
                data.AddRange(new byte[] { 0, 0 });
            }
            else
            {
                byte[] imageNameData = Encoding.UTF8.GetBytes(imageName);
                data.AddRange(BitConverter.GetBytes(imageNameData.Length));

                data.AddRange(imageNameData);
            }

            return data.ToArray();
        }
    }

    class LogIDs
    {
        // Variables
        private List<LogID> list;

        // Constructor
        public LogIDs()
        {
            list = new List<LogID>();
        }

        // Methods
        public bool Add(UInt32 id, string name, UInt32 color, UInt32 frameColor = 0xFF000000)
        {
            LogID logId = new LogID(id, name, color, frameColor);
            list.Add(logId);
            return true;
        }

        public void Add(LogID logId)
        {
            list.Add(logId);
        }

        /**
         * ファイル書き込み用の文字列を作成する
         */
        override public string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("<logid>");

            foreach (LogID logId in list)
            {
                sb.AppendLine("\t" + logId.ToString());
            }

            sb.AppendLine("</logid>");

            return sb.ToString();
        }

        /**
         * バイナリファイル書き込み用のバイト配列を作成する
         */
        public byte[] ToBinary()
        {
            List<byte> data = new List<byte>(1000);

            // ID情報の件数
            data.AddRange(BitConverter.GetBytes(list.Count));

            // ID情報
            foreach( LogID logId in list)
            {
                data.AddRange(logId.ToBinary());
            }

            return data.ToArray();
        }
    }
}
