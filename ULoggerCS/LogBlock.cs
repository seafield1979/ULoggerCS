using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


/**
 * ブロック
 * ブロックはバイナリモード時のメモリ展開の単位。
 * 大量のログがあった場合、ブロック単位でメモリに展開することで使用メモリを抑えることができる。
 * block,name:"ブロック1"
 * block,name:"ブロック2"
 */
namespace ULoggerCS
{
    class LogBlock : Log
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

        //
        // Constructor
        //
        public LogBlock(string name)
        {
            this.name = name;
        }

        //
        // Methods
        //
        public override string ToString()
        {
            return String.Format("block,name:{0}", name);
        }

        public override byte[] ToBinary()
        {
            return null;
        }
    }
}
