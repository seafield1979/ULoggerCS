using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Drawing;
using ULoggerCS.Utility;

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

        /**
         * バイナリ形式のログをファイルに書き込む
         * 
         * @input fs : 書き込み先のファイルオブジェクト
         * @input encoding : 文字列のエンコードタイプ
         */
        public override void WriteToBinFile(UFileStream fs, Encoding encoding)
        {
            // 名前
            fs.WriteSizeString(name, encoding);

            // 画像データ
            // 指定の画像ファイルをメモリに展開し書き込む
            try
            {
                // 画像ファイルから画像のbyte配列を取得する
                if (imagePath != null && File.Exists(imagePath))
                {
                    fs.WriteSizeBytes(File.ReadAllBytes(imagePath));
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

    }


    class IconImages
    {
        // Variables
        List<IconImage> images;

        // Constructor
        public IconImages()
        {
            images = new List<IconImage>();
        }

        // Methods
        public void Add(string name, string imagePath)
        {
            IconImage image = new IconImage(name, imagePath);
            images.Add(image);
        }

        public void Add(IconImage image)
        {
            images.Add(image);
        }

        /** 
         * テキスト形式のログファイルに書き込む用の文字列に変換する
         */
        override public string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("<image>");

            foreach (IconImage image in images)
            {
                sb.AppendLine("\t" + image.ToString());
            }

            sb.AppendLine("</image>");

            return sb.ToString();
        }

        /**
         * バイナリ形式のログファイルに書き込むためのバイト配列に変換する
         */
        public byte[] ToBinary()
        {
            List<byte> data = new List<byte>(1000);

            data.AddRange(BitConverter.GetBytes(images.Count));

            foreach (IconImage image in images)
            {
                data.AddRange(image.ToBinary());
            }

            return data.ToArray();
        }

        /**
         * テキスト形式のログファイルに書き込む
         * 
         * @input sw: 書き込み先のファイルオブジェクト
         */
        public void WriteToTextFile(StreamWriter sw)
        {
            sw.WriteLine("<image>");

            foreach (IconImage image in images)
            {
                sw.WriteLine("\t" + image.ToString());
            }

            sw.WriteLine("</image>");
        }

        /**
         * バイナリ形式のログファイルに書き込む
         * 
         * @input fs: 書き込み先のファイルオブジェクト
         */
        public void WriteToBinFile(UFileStream fs)
        {
            fs.WriteInt32(images.Count);

            foreach (IconImage image in images)
            {
                fs.WriteBytes(image.ToBinary());
            }
        }

    }
}
