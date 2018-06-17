using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace ULoggerCS
{
    class Lane : Log
    {
        // Variables
        private int id;
        private string  name;
        private UInt32 color;

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


        public int Id
        {
            get { return id; }
            set { id = value; }
        }



        // Constructor
        public Lane()
        {
            id = 0;
            name = null;
            color = 0;
        }

        public Lane(int id, string name, UInt32 color)
        {
            this.id = id;
            this.name = name;
            this.color = color; 
        }

        // Methods
        override public string ToString()
        {
            return string.Format(@"id:{0},name:""{1}"",color:#{2:X8}", id, name, color );
        }

        override public byte[] ToBinary()
        {
            List<byte> data = new List<byte>(1000);

            // レーン名の長さ
            byte[] nameData = Encoding.UTF8.GetBytes(name);
            data.AddRange(BitConverter.GetBytes(nameData.Length));

            // レーン名
            data.AddRange(nameData);

            // 背景色
            data.AddRange(BitConverter.GetBytes(color));

            return data.ToArray();
        }
    }




    class Lanes
    {
        // Variables
        List<Lane> list;

        // Constructor
        public Lanes()
        {
            list = new List<Lane>();
        }

        // Methods
        public bool Add(int id, string name, UInt32 color)
        {
            Lane lane = new Lane(id, name, color);
            list.Add(lane);
            return true;
        }

        public void WriteToFile(StreamWriter sw)
        {
            sw.WriteLine("<lane>");

            foreach (Lane lane in list)
            {
                sw.WriteLine("\t" + lane.ToString());
            }

            sw.WriteLine("</lane>");
        }

        override public string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("<lane>");

            foreach (Lane lane in list)
            {
                sb.AppendLine("\t" + lane.ToString());
            }

            sb.AppendLine("</lane>");

            return sb.ToString();
        }

        public byte[] ToBinary()
        {
            List<byte> data = new List<byte>(1000);

            data.AddRange(BitConverter.GetBytes(list.Count));

            foreach(Lane lane in list)
            {
                data.AddRange(lane.ToBinary());
            }

            return data.ToArray();
        }
    }
}
