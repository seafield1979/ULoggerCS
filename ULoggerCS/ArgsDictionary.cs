using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ULoggerCS
{
    /**
     * プログラムの引数を辞書型で取得する
     * 
     * プログラムの引数が
     *   key1 key2 key3=100 key4="abcde" 
     * の場合
     * {"key1"="", "key2"="", "key3"="100", "key4"="abcde"}
     * のような辞書型データを作成する
     */
    class ArgsDictionary
    {
        public static Dictionary<string, string> ArgsToDictionary(string[] args)
        {
            var dic1 = new Dictionary<string, string>();

            foreach (string arg in args)
            {
                string[] splitted = arg.Split('=');

                if (splitted.Length == 1)
                {
                    // key only
                    dic1[splitted[0]] = "";
                }
                if (splitted.Length >= 2)
                {
                    // key and value
                    dic1[splitted[0]] = splitted[1];
                }
            }

            return dic1;
        }
    }

    /**
     * ULogger用のプログラム引数を取得する
     * 
     * sample: rw_mode=read file_type=text file_path="C:\hoge\default.ulog" 
     */
    class MyArgs
    {
        private LogFileType fileType;
        private bool isReadMode;
        private string filePath;

        public LogFileType FileType
        {
            get { return fileType; }
            set { fileType = value; }
        }

        public bool IsReadMode
        {
            get { return isReadMode; }
            set { isReadMode = value; }
        }


        public string FilePath
        {
            get { return filePath; }
            set { filePath = value; }
        }

        // Constructor
        public MyArgs()
        {
            fileType = LogFileType.Text;
            isReadMode = true;
        }

        /**
         * Get MyArgs from args
         * 
         * @input args: プログラム引数
         * @output : 引数から取得したパラメータ
         */
        public static MyArgs GetMyArgs(string[] args)
        {
            var argDic = ArgsDictionary.ArgsToDictionary(args);
            var myArgs = new MyArgs();

            foreach (var kvp in argDic)
            {
                switch(kvp.Key)
                {
                    case "file_path":
                        myArgs.FilePath = kvp.Value;
                        break;
                    case "file_type":
                        switch (kvp.Value)
                        {
                            case "text":
                                myArgs.FileType = LogFileType.Text;
                                break;
                            case "bin":
                            case "binary":
                                myArgs.FileType = LogFileType.Binary;
                                break;
                        }
                        break;
                    case "rw_mode":
                        switch(kvp.Value)
                        {
                            case "read":
                            case "r":
                                myArgs.IsReadMode = true;
                                break;
                            case "write":
                            case "w":
                                myArgs.IsReadMode = false;
                                break;
                        }
                        break;
                }
            }

            return myArgs;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine(String.Format("FileType:{0}", fileType));
            sb.AppendLine(String.Format("ReadWriteMode:{0}", (isReadMode) ? "read" : "write"));
            sb.AppendLine(String.Format("FilePath:{0}", filePath));

            return sb.ToString();
        }

    }
}
