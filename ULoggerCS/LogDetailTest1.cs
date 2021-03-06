﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ULoggerCS
{
    /**
     * LogDetailを継承したクラスのサンプル１
     * 詳細データで文字列を扱う
     */
    class LogDetailTest1 : LogDetail
    {
        // Properties
        private string detailText;

        // Constructor
        public LogDetailTest1()
        {

        }

        // Accessor
        public string DetailText
        {
            set { this.detailText = value; }
            get { return detailText; }
        }
        
        // Methods
        override public LogDetail CreateCopy()
        {
            LogDetailTest1 copy1 = new LogDetailTest1();

            // ログに出力したい詳細情報をコピーする
            // ログに出力する必要がない情報はコピーしない
            if (detailText != null)
            {
                copy1.detailText = String.Copy(detailText);
                return copy1;
            }
            return copy1;
        }

        // 詳細情報をstringに変換して返す
        override public string ToString()
        {
            StringBuilder sb = new StringBuilder(detailText);

            if (detailText != null)
            {
                sb.Insert(0, @"\");
                sb.Append(@"\");
            }

            return detailText.ToString();
        }

        public override byte dataTypeByte()
        {
            return (byte)(DetailDataType.Text);
        }

        public override byte[] ToBinary()
        {
            return Encoding.UTF8.GetBytes(detailText);
        }
    }
}