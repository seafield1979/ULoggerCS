using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualBasic.FileIO;
using ULoggerCS.Utility;

/**
 * ULogView用のログファイルを読み込んでメモリに展開するクラス
 */
namespace ULoggerCS
{
    

    class LogReader
    {
        //
        // Consts
        //
        private const int ID_NAME_MAX = 64;         // ID名のByte数
        private const int LANE_NAME_MAX = 64;       // レーン名のByte数
        private const int IMAGE_NAME_MAX = 64;      // 画像名のByte数

        public Lanes lanes = null;
        public LogIDs logIDs = null;
        public MemIconImages images = null;
        public MemLogAreaManager areaManager = new MemLogAreaManager();
        public Encoding encoding;

        public MemLogAreaManager AreaManager
        {
            get { return areaManager; }
            set { areaManager = value; }
        }


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
            areaManager.Print();
            
            return true;
        }

        #region Text

        /**
         * テキスト形式のログファイルを読み込んでメモリに展開する
         * @input inputFilePath: ログファイルのパス
         * @output : true:成功 / false:失敗
         */
        private bool ReadLogFileText(string inputFilePath)
        {
            bool isHeader = false;
            
            // まずはヘッダ部分からエンコードタイプを取得する
            encoding = GetEncodingText(inputFilePath);

            using (TextFieldParser txtParser = new TextFieldParser(inputFilePath, encoding))
            {
                // 区切り文字形式
                txtParser.TextFieldType = FieldType.Delimited;
                // デリミタはカンマ
                txtParser.SetDelimiters(",");
                // フィールドが引用符で囲まれているか
                // txtParser.HasFieldsEnclosedInQuotes = true;

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
                                lanes = GetLanesText(txtParser);
                                break;
                            case "<logid>":
                                logIDs = GetLogIDsText(txtParser);
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
                        areaManager = GetLogDataText(txtParser);
                    }
                }
            }
            return true;
        }

        /**
         * LogID情報を取得する
         */
        private LogIDs GetLogIDsText(TextFieldParser parser)
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
                if (fields[0].Trim().Equals("</logid>"))
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
        private Lanes GetLanesText(TextFieldParser parser)
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
                                    lane.ID = id;
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
        private IconImages GetIconImagesText(TextFieldParser parser)
        {
            while (!parser.EndOfData)
            {
                string[] fields = parser.ReadFields();

                if (fields.Length == 0)
                {
                    continue;
                }
                // 終了判定
                if (fields[0].Trim().Equals("</images>"))
                {
                    break;
                }

                // 画像情報を取得
                MemIconImage image = new MemIconImage();

                foreach (string field in fields)
                {
                    // keyとvalueに分割
                    string[] splitted = field.Split(':');
                    if (splitted.Length >= 2)
                    {
                        switch (splitted[0])
                        {
                            case "name":
                                image.Name = splitted[1];
                                break;
                            case "image":
                                try
                                {
                                    // Base64文字列を出コード
                                    byte[] byteImage = Convert.FromBase64String(splitted[1]);
                                    image.SetByteImage(byteImage);
                                }
                                catch (Exception e)
                                {
                                    Console.WriteLine("base64のデコードに失敗しました。");
                                }
                                break;
                        }
                    }
                }
            }
            return null;
        }

        /**
         * ログの本体部分を取得
         * <body>～</body> の中の行をすべて取得する
         * 
         */
        private MemLogAreaManager GetLogDataText(TextFieldParser parser)
        {
            MemLogAreaManager manager = new MemLogAreaManager();

            while (!parser.EndOfData)
            {
                string[] fields = parser.ReadFields();

                if (fields.Length == 0)
                {
                    continue;
                }
                string firstField = fields[0].Trim();
                // 終了判定
                switch (firstField)
                {
                    case "</body>":
                        return manager;
                    case "area":
                        MemLogArea area = GetMemAreaText(fields, manager);
                        manager.AddArea(area);
                        break;
                    case "log":
                        MemLogData log = GetMemLogText(fields);
                        manager.AddLogData(log);
                        break;
                }
            }

            return null;
        }

        /**
         * 1行分のフィールドからエリアデータを取得する
         * @input fields:
         * @output  取得したエリアデータ
         */
        private static MemLogArea GetMemAreaText(string[] fields, MemLogAreaManager manager)
        {
            MemLogArea area = new MemLogArea();

            foreach (string field in fields)
            {
                string[] splitted = field.Split(':');
                if (splitted.Length == 2)
                {
                    switch (splitted[0].ToLower())
                    {
                        case "name":
                            area.Name = splitted[1];
                            break;
                        case "parent":
                            area.ParentArea = manager.searchArea(splitted[1]);
                            break;
                        case "color":
                            area.Color = Convert.ToUInt32(splitted[1], 16);
                            break;
                        case "image":       // todo
                            break;
                    }
                    // area,name:"area1",parent:"root",color=FF000000
                }
            }
            return area;
        }

        /**
         * 1行分のフィールドからログデータを取得する
         * @input fields:
         * @output  取得したログデータ
         *  Sample
         *  log,type: Single,id: 1,lane: 1,time: 0.0026799596,text: "test1"
         */
        private static MemLogData GetMemLogText(string[] fields)
        {
            MemLogData log = new MemLogData();

            foreach (string field in fields)
            {
                string[] splitted = field.Split(':');
                if (splitted.Length == 2)
                {
                    switch (splitted[0].ToLower())
                    {
                        case "type":
                            switch (splitted[1].ToLower()) {
                                case "single":               // 単体ログ
                                    log.Type = MemLogType.Point;
                                    break;
                                case "areastart":            // 範囲開始
                                    log.Type = MemLogType.Range;
                                    break;
                                case "areaend":              // 範囲終了
                                    log.Type = MemLogType.Range;
                                    break;
                                case "value":                // 値
                                    log.Type = MemLogType.Value;
                                    break;
                            }
                            break;
                        case "id":
                            log.ID = UInt32.Parse(splitted[1]);
                            break;
                        case "lane":
                            log.LaneId = Byte.Parse(splitted[1]);
                            break;
                        case "time":
                            log.Time1 = Double.Parse(splitted[1]);
                            break;
                        case "text":
                            log.Text = splitted[1];
                            break;
                        case "color":
                            log.Color = Convert.ToUInt32(splitted[1], 16);
                            break;
                        case "detail_type":
                            switch(splitted[1].ToLower())
                            {
                                case "text":
                                    log.DetailType = DetailDataType.Text;
                                    break;
                                case "array":
                                    log.DetailType = DetailDataType.Array;
                                    break;
                                case "dictionary":
                                    log.DetailType = DetailDataType.Dictionary;
                                    break;
                                case "json":
                                    log.DetailType = DetailDataType.JSON;
                                    break;
                            }
                            break;
                        case "detail":
                            if (log.DetailType == DetailDataType.Array)
                            {
                                Console.WriteLine("");
                            }
                            log.Detail = splitted[1];
                            break;
                    }
                }
            }
            return log;
        }

        /**
         * ヘッダ部分からファイルのエンコードタイプを取得する
         * 
         * @input inputFilePath:  LogViewのファイルパス
         * @output エンコードの種類
         */
        public static Encoding GetEncodingText(string inputFilePath)
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
                                encoding = GetEncodingFromStr(splitted[1].ToLower());
                            }
                        }
                    }
                    count++;
                    if (count > 100)
                    {
                        // ファイルの先頭部分に見つからなかったので打ち切り
                        break;
                    }
                }
                return encoding;
            }
        }  // GetEncoding()

        /**
         * テキストからエンコーディングを取得する
         * @input encStr: エンコード名
         */
        private static Encoding GetEncodingFromStr(string encStr)
        {
            Encoding encoding = Encoding.UTF8;      // デフォルトのエンコード

            switch (encStr.ToLower())
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
                    encoding = Encoding.GetEncoding(encStr);
                    break;
            }
            return encoding;
        }

        #endregion

        #region Binary
        /**
         * バイナリ形式のログファイルを読み込んでメモリに展開する
         * @input inputFilePath: ログファイルのパス
         * @output : true:成功 / false:失敗
         */
        private bool ReadLogFileBin(string inputFilePath)
        {
            using (var fs = new UFileStream(inputFilePath, FileMode.Open, FileAccess.Read))
            {
                try
                {
                    // Header
                    ReadLogHeadBin(fs);

                    // Body
                    ReadLogBodyBin(fs);
                }
                catch (Exception e)
                {
                    Console.WriteLine("Error ReadLogFileBin " + e.Message);
                    throw;
                }
            }
            return true;
        }

        /**
         * バイナリログのヘッダ部分を取得
         * @input fs : ファイルオブジェクト
         * 
         */ 
        private void ReadLogHeadBin(UFileStream fs)
        {
            // 文字コードを取得
            string encStr = fs.GetSizeString();
            this.encoding = GetEncodingFromStr(encStr);
            fs.EncodingType = encoding;
            
            // ログID情報
            logIDs = ReadLogIDsBin(fs);

            // レーン情報
            lanes = ReadLogLanesBin(fs);

            // アイコン画像
            images = ReadLogImagesBin(fs);
        }

        /**
         * バイナリログのログID情報を読み込む
         * 
         */
        private LogIDs ReadLogIDsBin(UFileStream fs)
        {
            LogIDs _logIds = new LogIDs();

            // 件数取得
            int size = fs.GetInt32();

            for (int i = 0; i < size; i++)
            {
                // 1件分のログを取得
                LogID logId = new LogID();

                // ID
                logId.ID = fs.GetUInt32();

                // 名前
                logId.Name = fs.GetSizeString();

                // 色
                logId.Color = fs.GetUInt32();

                // アイコン画像名
                logId.ImageName = fs.GetSizeString();

                _logIds.Add(logId);
            }

            return _logIds;
        }

        /**
         * バイナリログのレーン情報を読み込む
         * @input fs: 読み込み元のファイル
         */
        private Lanes ReadLogLanesBin(UFileStream fs)
        {
            Lanes _lanes = new Lanes();

            // 件数取得
            int size = fs.GetInt32();

            for (int i = 0; i < size; i++)
            {
                // 1件分のログを取得
                Lane lane = new Lane();

                // ID
                lane.ID = fs.GetUInt32();

                // 名前
                lane.Name = fs.GetSizeString();

                // 色
                lane.Color = fs.GetUInt32();

                _lanes.Add(lane);
            }

            return _lanes;
        }

        /**
         * バイナリログの画像ID情報を読み込む
         */
        private MemIconImages ReadLogImagesBin(UFileStream fs)
        {
            MemIconImages _images = new MemIconImages();

            // 件数取得
            int size = fs.GetInt32();

            for (int i = 0; i < size; i++)
            {
                // １件分のログを取得
                MemIconImage image = new MemIconImage();

                // 名前
                image.Name = fs.GetSizeString();

                // 画像
                int imageSize = fs.GetInt32();
                if (imageSize > 0)
                {
                    byte[] byteImage = fs.GetBytes(imageSize);
                    image.SetByteImage(byteImage);
                }

                _images.Add(image);
            }

            return _images;
        }

        /**
         * バイナリログの本体部分を取得
         * @input fs : ファイルオブジェクト
         */
        private void ReadLogBodyBin(UFileStream fs)
        {
            // エリア、ログ

            // 件数取得
            int logNum = fs.GetInt32();

            for(int i=0; i< logNum; i++)
            {
                // 種類を取得
                LogType type = (LogType)fs.GetByte();

                if (type == LogType.Data)
                {
                    ReadLogDataBin(fs);
                }
                else
                {
                    ReadLogAreaBin(fs);
                }
            }
        }

        /**
         * バイナリログデータを読み込む
         */
        private void ReadLogDataBin(UFileStream fs)
        {
            MemLogData log = new MemLogData();

            // ログID
            log.ID = fs.GetUInt32();

            //ログタイプ
            bool isRangeEnd = false;
            LogDataType dataType = (LogDataType)fs.GetByte();
            
            switch (dataType) {
                case LogDataType.Single:
                    log.Type = MemLogType.Point;
                    break;
                case LogDataType.RangeStart:
                    log.Type = MemLogType.Range;
                    break;
                case LogDataType.RangeEnd:
                    // 同じレーンの Range タイプのログに結合する
                    // todo
                    isRangeEnd = true;
                    break;
                case LogDataType.Value:
                    log.Type = MemLogType.Value;
                    break;
            }

            //表示レーンID
            log.LaneId = fs.GetUInt32();

            //タイトルの長さ
            //タイトル
            log.Text = fs.GetSizeString();

            // 範囲ログの終了タイプの場合、結合する

            //時間
            Double time = fs.GetDouble();
            if (log.Type == MemLogType.Range && isRangeEnd == true)
            {
                // 1つ前の Rangeタイプの Time2 に時間を設定
                // todo
                return;
            }
            else
            {
                log.Time1 = time;
            }

            //ログデータ（詳細）の種類
            log.DetailType = (DetailDataType)fs.GetByte();

            if (log.DetailType == DetailDataType.Array)
            {
                Console.WriteLine("hoge");
            }
            //ログデータ(詳細)のサイズ
            //ログデータ(詳細)
            if (log.DetailType != DetailDataType.None)
            {
                log.Detail = fs.GetSizeString();
            }

            // ログを追加する
            areaManager.AddLogData(log);
        }

        /**
         * バイナリエリアデータを読み込む
         */
        private void ReadLogAreaBin(UFileStream fs)
        {
            MemLogArea area = new MemLogArea();

            // エリア名の長さ
            // エリア名
            area.Name = fs.GetSizeString();

            // 親のエリア名の長さ
            // 親のエリア名
            area.ParentArea = areaManager.searchArea(fs.GetSizeString());

            // 色
            area.Color = fs.GetUInt32();

            // エリアを追加
            areaManager.AddArea(area);
        }

        #endregion


        #region Debug

        /**
         * LogReaderに読み込んだ情報をファイルに保存する
         * 
         * @input filePath : 書き込み先のファイルパス
         */
        public void WriteToFile(string filePath)
        {
            using (StreamWriter sw = new StreamWriter(filePath, false, Encoding.UTF8))
            {
                if (logIDs != null)
                {
                    sw.Write(logIDs.ToString());
                }
                if (lanes != null)
                {
                    sw.Write(lanes.ToString());
                }
                if (images != null)
                {
                    sw.Write(images.ToString());
                }
                
                if (areaManager != null)
                {
                    areaManager.WriteToFile(sw);
                }
            }
        }
        #endregion
    }
}
