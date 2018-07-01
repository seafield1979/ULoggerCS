using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ULoggerCS
{
    /**
     * LogDetailを継承したクラスのサンプル２
     * 詳細データで配列を扱う
     */
    class LogDetailTest2 : LogDetail
    {
        // Consts
        public const int ARRAY1_SIZE = 10;
        // Properties
        protected int[] array1 = new int[ARRAY1_SIZE];

        // Accessor
        public void setArray1(int index, int value)
        {
            if (index < ARRAY1_SIZE)
            {
                array1[index] = value;
            }
        }

        // Constructor
        public LogDetailTest2()
        {

        }

        // 初期値を設定する
        public void Init()
        {
            for (int i = 0; i < ARRAY1_SIZE; i++)
            {
                array1[i] = i + 1;
            }
        }

        // Methods
        override public LogDetail CreateCopy()
        {
            LogDetailTest2 copy1 = new LogDetailTest2();
            for (int i = 0; i < ARRAY1_SIZE; i++)
            {
                copy1.array1[i] = array1[i];
            }

            return copy1;
        }

        override public string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("[");
            for (int i = 0; i < ARRAY1_SIZE; i++)
            {
                if (i > 0)
                {
                    sb.Append(",");
                }
                sb.Append(array1[i]);
            }
            sb.Append("]");
            return sb.ToString();
        }

        public override string dataTypeString()
        {
            return "array";
        }

        public override byte dataTypeByte()
        {
            return (byte)DetailDataType.Array;
        }

        public override byte[] ToBinary()
        {
            return Encoding.UTF8.GetBytes(this.ToString());
        }
    }
}
