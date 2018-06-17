using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualBasic.FileIO;

/**
 * ULogView用のログファイルを読み込んでメモリに展開するクラス
 */
namespace ULoggerCS
{
    

    class LogReader
    {
        public Lanes lanes = null;
        public LogIDs logIDs = null;
        public IconImages images;
        public List<LogData> logs = null;
        public Encoding encoding;

        //public string        

        public LogReader()
        {
            encoding = Encoding.UTF8;
        }

        /**
         * ULoggerで作成したログファイルを読み込んでULogViewで使用できる種類のデータとしてメモリ展開する
         * 
         * @input inputFilePath: ログファイルのパス
         * @input fileType: ログファイルの種類(テキスト、バイナリ)
         * @output : true:成功 / false:失敗
         */
        public bool ReadLogFile(string inputFilePath, LogFileType fileType)
        {
            if (fileType == LogFileType.Text)
            {
                ReadLogFileText(inputFilePath);
            }
            else
            {
                ReadLogFileBin(inputFilePath);
            }
            return true;
        }

        /**
         * テキスト形式のログファイルを読み込んでメモリに展開する
         * @input inputFilePath: ログファイルのパス
         * @output : true:成功 / false:失敗
         */
        private bool ReadLogFileText(string inputFilePath)
        {
            bool isHeader = false;
            
            // まずはヘッダ部分からエンコードタイプを取得する
            encoding = GetEncoding(inputFilePath);

            using (TextFieldParser txtParser = new TextFieldParser(inputFilePath, encoding))
            {
                // 区切り文字形式
                txtParser.TextFieldType = FieldType.Delimited;
                // デリミタはカンマ
                txtParser.SetDelimiters(",");

                // ヘッダ部分を読み込む <head>～</head>
                while (!txtParser.EndOfData)
                {
                    // ファイルを 1 行ずつ読み込む
                    string line = txtParser.ReadLine().Trim();

                    // <head>の行が見つかるまではスキップ
                    if (isHeader == false)
                    {
                        if (line.Equals("<head>"))
                        {
                            isHeader = true;
                        }
                    }
                    else
                    {
                        if (line.Equals("</head>"))
                        {
                            break;
                        }

                        // ヘッダーの読み込みメイン
                        switch (line)
                        {
                            case "<lane>":
                                lanes = GetLanes(txtParser);
                                break;
                            case "<logid>":
                                logIDs = GetLogIDs(txtParser);
                                break;
                            case "<images>":
                                break;
                        }
                    }
                }

                // 本体部分を読み込む
                bool isBody = false;
                while (!txtParser.EndOfData)
                {
                    // ファイルを 1 行ずつ読み込む
                    string line = txtParser.ReadLine().Trim();

                    // <body>を見つけたら本体の取得モードに入る
                    if (!isBody)
                    {
                        if (line.Equals("<body>"))
                        {
                            isBody = true;
                        }
                    }
                    else
                    {
                        //logs = GetLogData(txtParser);
                    }
                }
            }
            return true;
        }

        /**
         * バイナリ形式のログファイルを読み込んでメモリに展開する
         * @input inputFilePath: ログファイルのパス
         * @output : true:成功 / false:失敗
         */
        private bool ReadLogFileBin(string inputFilePath)
        {
            return true;
        }

        /**
         * LogID情報を取得する
         */
        private LogIDs GetLogIDs(TextFieldParser parser)
        {
            LogIDs _logIDs = new LogIDs();

            while (!parser.EndOfData)
            {
                string[] fields = parser.ReadFields();
                
                if (fields.Length == 0)
                {
                    continue;
                }

                // 終了判定
                if (fields[0].Trim().Equals("</logids>"))
                {
                    break;
                }

                // レーン情報を取得
                LogID logID = new LogID();

                foreach (string field in fields)
                {
                    // keyとvalueに分割
                    string[] splitted = field.Split(':');
                    if (splitted.Length >= 2)
                    {
                        switch (splitted[0])
                        {
                            case "id":
                                UInt32 id;
                                if (UInt32.TryParse(splitted[1], out id))
                                {
                                    logID.ID = id;
                                }
                                break;
                            case "name":
                                logID.Name = splitted[1];
                                break;
                            case "color":
                                // FF001122 のような16進数文字列を整数値に変換
                                logID.Color = Convert.ToUInt32(splitted[1], 16);
                                break;
                            case "image":
                                break;
                        }
                    }
                }
                _logIDs.Add(logID);
            }
            return _logIDs;
        }

        /**
         * Lane情報を取得する
         * <lane>～</lane> の中を行をすべて取得する
         */
        private Lanes GetLanes(TextFieldParser parser)
        {
            Lanes _lanes = new Lanes();

            while(!parser.EndOfData)
            {
                string[] fields = parser.ReadFields();
                
                if (fields.Length == 0)
                {
                    continue;
                }
                // 終了判定
                if (fields[0].Trim().Equals("</lane>"))
                {
                    break;
                }

                // レーン情報を取得
                Lane lane = new Lane();

                foreach (string field in fields)
                {
                    // keyとvalueに分割
                    string[] splitted = field.Split(':');
                    if (splitted.Length >= 2)
                    {
                        switch (splitted[0])
                        {
                            case "id":
                                UInt32 id;
                                if (UInt32.TryParse(splitted[1], out id))
                                {
                                    lane.Id = id;
                                }
                                break;
                            case "name":
                                lane.Name = splitted[1];
                                break;
                            case "color":
                                try {
                                    // FF001122 のような16進数文字列を整数値に変換
                                    lane.Color = Convert.ToUInt32(splitted[1], 16);
                                }
                                catch (Exception e)
                                {
                                    lane.Color = 0;
                                }
                                break;
                        }
                    }
                }
                _lanes.Add(lane);
            }
            return _lanes;
        }

        /**
         * １行分のIconImage情報を取得する
         */
        private IconImages GetIconImages(TextFieldParser parser)
        {
            return null;
        }

        /**
         * ログの本体部分を取得
         * <body>～</body> の中の行をすべて取得する
         * 
         */
        private Logs GetLogData(TextFieldParser parser)
        {
            Logs _logs = new Logs();


            return null;
        }

        /**
         * ヘッダ部分からファイルのエンコードタイプを取得する
         * 
         * @input inputFilePath:  LogViewのファイルパス
         * @output エンコードの種類
         */
        public static Encoding GetEncoding(string inputFilePath)
        {
            bool isHeader = false;
            int count = 0;
            Encoding encoding = Encoding.UTF8;

            using (StreamReader sr = new StreamReader(inputFilePath))
            {
                // ヘッダ部分を読み込む <head>～</head>
                while (sr.Peek() >= 0)
                {
                    // ファイルを 1 行ずつ読み込む
                    string line = sr.ReadLine().Trim();

                    // <head>の行が見つかるまではスキップ
                    if (isHeader == false)
                    {
                        if (line.Equals("<head>"))
                        {
                            isHeader = true;
                        }
                    }
                    else
                    {
                        if (line.Equals("</head>"))
                        {
                            break;
                        }
                        // エンコード種別
                        if (line.Contains("encoding:"))
                        {
                            string[] splitted = line.Split(':');
                            if (splitted.Length >= 2)
                            {
                                switch (splitted[1].ToLower())
                                {
                                    case "ascii":
                                        encoding = Encoding.ASCII;
                                        break;
                                    case "utf7":
                                    case "utf-7":
                                        encoding = Encoding.UTF7;
                                        break;
                                    case "utf8":
                                    case "utf-8":
                                        encoding = Encoding.UTF8;
                                        break;
                                    case "utf32":
                                    case "utf-32":
                                        encoding = Encoding.UTF32;
                                        break;
                                    case "unicode":
                                        encoding = Encoding.Unicode;
                                        break;
                                    default:
                                        encoding = Encoding.GetEncoding(splitted[1]);
                                        break;
                                }
                            }
                        }
                    }
                    count++;
                    if (count > 1000)
                    {
                        // ファイルの先頭部分に見つからなかったので打ち切り
                        break;
                    }
                }
                return encoding;
            }
        }  // GetEncoding()

    }
}
