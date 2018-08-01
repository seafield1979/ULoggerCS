﻿using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace ULoggerCS
{
    class MemLogID
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

        private Image image;

        public Image Image
        {
            get { return image; }
            set { image = value; }
        }

        //
        // Constructor
        //
        public MemLogID()
        {
            id = 0;
            name = null;
            color = 0;
            frameColor = 0;
            image = null;
        }

        public MemLogID(UInt32 id, string name, UInt32 color, UInt32 frameColor = 0xFF000000, Image image = null)
        {
            this.id = id;
            this.name = name;
            this.color = color;
            this.frameColor = color;
            this.image = image;
        }

        //
        // Methods
        //
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendFormat("id:{0}", id);
            sb.AppendFormat(",name:{0}", name);
            sb.AppendFormat(",color:{0:X8}", color);
            sb.AppendFormat(",frameColor:{0:X8}", frameColor);
            if (image != null)
            {
                sb.AppendFormat(",image:{0}byte", image.Size);
            }
            return sb.ToString();
        }
    }

    class MemLogIDs
    {
        // Variables
        private List<MemLogID> logIDs;

        // Constructor
        public MemLogIDs()
        {
            logIDs = new List<MemLogID>();
        }

        // Methods
        public bool Add(UInt32 id, string name, UInt32 color, UInt32 frameColor = 0xFF000000)
        {
            MemLogID logId = new MemLogID(id, name, color, frameColor);
            logIDs.Add(logId);
            return true;
        }

        public void Add(MemLogID logId)
        {
            logIDs.Add(logId);
        }

        public IEnumerator<MemLogID> GetEnumerator()
        {
            foreach(MemLogID logID in logIDs)
            {
                yield return logID;
            }
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("<logID>");
            foreach (MemLogID logID in logIDs)
            {
                sb.AppendFormat("\t{0}\r\n", logID.ToString());
            }
            sb.AppendLine("</logID>");

            return sb.ToString();
        }
    }
}
