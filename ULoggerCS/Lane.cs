﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using ULoggerCS.Utility;

namespace ULoggerCS
{
    /**
     * レーン情報
     * 各ログのレーンIDに一致したレーンにログが表示される
     */
    class Lane : Log
    {
        //
        // Properties
        //
        private UInt32 id;          // Lane ID
        private string  name;       // Lane name
        private UInt32 color;       // Lane (Background) color

        public UInt32 Color
        {
            get { return color; }
            set { color = value; }
        }

        public string  Name
        {
            get { return name; }
            set { name = value; }
        }


        public UInt32 ID
        {
            get { return id; }
            set { id = value; }
        }

        //
        // Constructor
        //
        public Lane()
        {
            id = 0;
            name = null;
            color = 0;
        }

        public Lane(UInt32 id, string name, UInt32 color)
        {
            this.id = id;
            this.name = name;
            this.color = color; 
        }

        //
        // Methods
        //
        override public string ToString()
        {
            return string.Format(@"id:{0},name:""{1}"",color:{2:X8}", id, name, color );
        }

        override public byte[] ToBinary()
        {
            List<byte> data = new List<byte>(1000);

            // ID
            data.AddRange(BitConverter.GetBytes(id));

            // Length of name
            byte[] nameData = Encoding.UTF8.GetBytes(name);
            data.AddRange(BitConverter.GetBytes(nameData.Length));

            // Name
            data.AddRange(nameData);

            // Color
            data.AddRange(BitConverter.GetBytes(color));

            return data.ToArray();
        }

        /**
         * バイナリ形式のログファイルにログを出力する
         * 
         * @input fs: 書き込み先のファイルオブジェクト
         * @input encoding: 文字列を出力する場合のエンコードタイプ
         */
        public override void WriteToBinFile(UFileStream fs, Encoding encoding)
        {
            // ID
            fs.WriteUInt32(id);

            // Name
            fs.WriteSizeString(name, encoding);

            // Color
            fs.WriteUInt32(color);

        }
    }


    /**
     * Lanes 
     * レーン全体を管理するクラス
     */
    class Lanes
    {
        //
        // Properties
        //
        List<Lane> lanes;

        //
        // Constructor
        //
        public Lanes()
        {
            lanes = new List<Lane>();
        }

        //
        // Methods
        //
        /**
         * レーンを１件追加する(パラメータ)
         * 
         * @input id
         * @input name
         * @input color
         */
        public void Add(UInt32 id, string name, UInt32 color)
        {
            Lane lane = new Lane(id, name, color);
            lanes.Add(lane);
        }

        /**
         * レーンを１件追加する(オブジェクト)
         * 
         * @input lane: 外部で作成したレーンオブジェクト
         */ 
        public void Add(Lane lane)
        {
            lanes.Add(lane);
        }

        /**
         * ファイルに書き込む
         * 
         * @input sw: 書き込み先のファイルオブジェクト
         */
        public void WriteToFile(StreamWriter sw)
        {
            sw.WriteLine("<lane>");

            foreach (Lane lane in lanes)
            {
                sw.WriteLine("\t" + lane.ToString());
            }

            sw.WriteLine("</lane>");
        }

        /**
         * テキスト形式のログファイルに書き込む用の文字列に変換する
         */
        override public string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("<lane>");

            foreach (Lane lane in lanes)
            {
                sb.AppendLine("\t" + lane.ToString());
            }

            sb.AppendLine("</lane>");

            return sb.ToString();
        }

        /**
         * バイナリ形式のログファイルに書き込む用の文字列に変換する
         */
        public byte[] ToBinary()
        {
            List<byte> data = new List<byte>(1000);

            data.AddRange(BitConverter.GetBytes(lanes.Count));

            foreach(Lane lane in lanes)
            {
                data.AddRange(lane.ToBinary());
            }

            return data.ToArray();
        }

        /**
         * テキスト形式のログファイルに書き込む
         */
        public void WriteToTextFile(StreamWriter sw)
        {
            sw.WriteLine("<lane>");

            foreach (Lane lane in lanes)
            {
                sw.WriteLine("\t" + lane.ToString());
            }

            sw.WriteLine("</lane>");
        }

        /**
         * バイナリ形式のログファイルに書き込む
         */
        public void WriteToBinFile(UFileStream fs)
        {
            fs.WriteInt32(lanes.Count);

            foreach (Lane lane in lanes)
            {
                fs.WriteBytes(lane.ToBinary());
            }
        }
    }
}
