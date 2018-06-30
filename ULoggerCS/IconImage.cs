using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Drawing;

namespace ULoggerCS
{
    class IconImage : Log
    {
        //
        // Properties
        //
        // 画像名(拡張子込み)
        private string name;

        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        //画像ファイルパス
        private string imagePath;

        public string ImagePath
        {
            get { return imagePath; }
            set { imagePath = value; }
        }


        // Constructor
        public IconImage()
        {
            name = null;
            imagePath = null;
        }

        public IconImage(string name, string imagePath)
        {
            this.name = name;
            this.imagePath = imagePath;
        }

        // Methods
        override public string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat(@"name:""{0}""", name);

            try
            {
                // 画像ファイルを開き、base64変換する
                if (imagePath != null && File.Exists(imagePath))
                {
                    byte[] image = File.ReadAllBytes(imagePath);
                    sb.AppendFormat(@",image:""{0}""", Convert.ToBase64String(image));
                    return sb.ToString();
                }
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
            }
            return "";
        }

        override public byte[] ToBinary()
        {
            List<byte> data = new List<byte>(1000);

            // 名前の長さ
            byte[] nameData = Encoding.UTF8.GetBytes(name);
            data.AddRange(BitConverter.GetBytes(nameData.Length));

            // 名前
            data.AddRange(nameData);

            // 画像データ
            // 指定の画像ファイルをメモリに展開し書き込む
            try
            {
                // 画像ファイルから画像のbyte配列を取得する
                if (imagePath != null && File.Exists(imagePath))
                {
                    byte[] image = File.ReadAllBytes(imagePath);
                    // 画像サイズ
                    data.AddRange(BitConverter.GetBytes(image.Length));
                    // 画像
                    data.AddRange(image);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            return data.ToArray();
        }

    }


    class IconImages
    {
        // Variables
        List<IconImage> list;

        // Constructor
        public IconImages()
        {
            list = new List<IconImage>();
        }

        // Methods
        public void Add(string name, string imagePath)
        {
            IconImage image = new IconImage(name, imagePath);
            list.Add(image);
        }

        public void Add(IconImage image)
        {
            list.Add(image);
        }

        public void WriteToFile(StreamWriter sw)
        {
            sw.WriteLine("<image>");

            foreach (IconImage image in list)
            {
                sw.WriteLine("\t" + image.ToString());
            }

            sw.WriteLine("</image>");
        }

        override public string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("<image>");

            foreach (IconImage image in list)
            {
                sb.AppendLine("\t" + image.ToString());
            }

            sb.AppendLine("</image>");

            return sb.ToString();
        }

        public byte[] ToBinary()
        {
            List<byte> data = new List<byte>(1000);

            data.AddRange(BitConverter.GetBytes(list.Count));

            foreach (IconImage image in list)
            {
                data.AddRange(image.ToBinary());
            }

            return data.ToArray();
        }

    }
}
