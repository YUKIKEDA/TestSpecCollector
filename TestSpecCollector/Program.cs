using System.Reflection;
using System.Text;
using System.Xml.Linq;

namespace TestSpecCollector
{
    internal class Program
    {
        static void Main(string[] args)
        {
            // ==========================================
            // 設定エリア (環境に合わせて変更してください)
            // ==========================================

            // 1. テストプロジェクトのDLLのパス (ビルド済みのもの)
            string targetDllPath = @"D:\home\Programs\CSharpProjects\TestSpecCollector\TestSpecCollectorTests\bin\Debug\net8.0\TestSpecCollectorTests.dll";

            // 2. 生成されたXMLドキュメントのパス
            string targetXmlPath = @"D:\home\Programs\CSharpProjects\TestSpecCollector\TestSpecCollectorTests\bin\Debug\net8.0\TestSpecCollectorTests.xml";

            // 3. 出力するCSVのパス
            string outputCsvPath = @"D:\home\Programs\CSharpProjects\TestSpecCollector\TestSpecification.csv";

            // ==========================================
            // 実行ロジック
            // ==========================================

            try
            {
                Console.WriteLine("DLLを読み込んでいます...");
                if (!File.Exists(targetDllPath)) throw new FileNotFoundException("DLLが見つかりません", targetDllPath);
                if (!File.Exists(targetXmlPath)) throw new FileNotFoundException("XMLが見つかりません", targetXmlPath);

                // アセンブリ(DLL)とXMLをロード
                var assembly = Assembly.LoadFrom(targetDllPath);
                var xmlDoc = XDocument.Load(targetXmlPath);

                // CSV作成用バッファ
                var csvBuilder = new StringBuilder();

                // CSVヘッダー (Excelで開きやすい構成)
                csvBuilder.AppendLine("TestID,ClassName,Title(MethodName),Preconditions,Steps,ExpectedResult,Summary(Optional)");

                int count = 0;

                // すべてのクラスを走査
                foreach (var type in assembly.GetTypes())
                {
                    // すべてのメソッドを走査 (PublicかつInstanceメソッド)
                    foreach (var method in type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly))
                    {
                        // テストメソッド判定
                        if (!IsTestMethod(method)) continue;

                        // 1. TestIDの取得 (属性から)
                        string testId = GetTestId(method);

                        // 2. テストタイトルの決定 (メソッド名を優先)
                        string title = method.Name;

                        // 3. XMLドキュメント情報の取得
                        var xmlInfo = GetXmlComments(method, xmlDoc);

                        // もしXMLにSummaryがあり、かつタイトル(メソッド名)が英語っぽい(日本語を含まない)場合、
                        // Summaryをタイトルとして採用するロジックを入れても良いが、
                        // 今回は「メソッド名＝タイトル」を基本とするため、Summaryは別カラムへ。

                        // CSV行を追加 (IDの次にClassName)
                        csvBuilder.AppendLine(
                            $"\"{testId}\"," +
                            $"\"{type.Name}\"," +
                            $"\"{title}\"," +
                            $"\"{xmlInfo.Preconditions}\"," +
                            $"\"{xmlInfo.Steps}\"," +
                            $"\"{xmlInfo.Expected}\"," +
                            $"\"{xmlInfo.Summary}\""
                        );

                        count++;
                    }
                }

                // UTF-8 (BOM付き) で保存 (Excelでの文字化け防止)
                File.WriteAllText(outputCsvPath, csvBuilder.ToString(), Encoding.UTF8);

                Console.WriteLine($"--------------------------------------------------");
                Console.WriteLine($"完了しました。合計 {count} 件のテストを出力しました。");
                Console.WriteLine($"保存先: {outputCsvPath}");
                Console.WriteLine($"--------------------------------------------------");
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("エラーが発生しました:");
                Console.Error.WriteLine(ex.Message);
                Console.Error.WriteLine(ex.StackTrace);
            }
        }

        // --- ヘルパーメソッド ---

        // テストメソッドかどうかを判定
        // NUnit([Test]), xUnit([Fact]), MSTest([TestMethod]) などを名前で緩く判定
        static bool IsTestMethod(MethodInfo m)
        {
            var attributes = m.GetCustomAttributes().Select(a => a.GetType().Name);
            return attributes.Any(name =>
                name.StartsWith("Test") ||  // Test, TestMethod, TestCase
                name.StartsWith("Fact") ||  // Fact
                name.StartsWith("Theory")   // Theory
            );
        }

        // [TestId] 属性からIDを取得
        static string GetTestId(MethodInfo m)
        {
            // 参照設定せずに動的に属性値を取得する (依存関係を減らすため)
            var attr = m.GetCustomAttributes().FirstOrDefault(a => a.GetType().Name == "TestIdAttribute");
            if (attr == null) return "";

            // "Id" プロパティの値を取得
            var prop = attr.GetType().GetProperty("Id");
            return prop?.GetValue(attr)?.ToString() ?? "";
        }

        // XMLコメント情報を取得するためのコンテナ
        class XmlCommentInfo
        {
            public string Summary { get; set; } = "";
            public string Preconditions { get; set; } = "";
            public string Steps { get; set; } = "";
            public string Expected { get; set; } = "";
        }

        // メソッドに対応するXML要素を取得・解析
        static XmlCommentInfo GetXmlComments(MethodInfo m, XDocument doc)
        {
            var info = new XmlCommentInfo();

            // XMLドキュメントIDの生成 (形式: M:Namespace.Class.Method)
            // ※引数がある場合などは複雑になりますが、テストメソッドは引数なしか単純なケースが多いため簡易実装です
            string docId = $"M:{m.DeclaringType?.FullName}.{m.Name}";

            var memberNode = doc.Descendants("member").FirstOrDefault(x => x.Attribute("name")?.Value == docId);
            if (memberNode == null) return info;

            // 各タグの中身を取得して整形
            info.Summary = CleanText(memberNode.Element("summary")?.Value);
            info.Preconditions = CleanText(memberNode.Element("preconditions")?.Value); // カスタムタグ
            info.Steps = CleanText(memberNode.Element("steps")?.Value);                 // カスタムタグ
            info.Expected = CleanText(memberNode.Element("expected")?.Value);           // カスタムタグ

            return info;
        }

        // テキスト整形処理 (余分な空白削除、改行統一、CSVエスケープ)
        static string CleanText(string? input)
        {
            if (string.IsNullOrWhiteSpace(input)) return "";

            // 前後の空白削除
            var text = input.Trim();

            // XML内のインデント空白を行ごとに削除
            var lines = text.Split(["\r\n", "\r", "\n"], StringSplitOptions.None)
                            .Select(line => line.Trim())
                            .Where(line => !string.IsNullOrWhiteSpace(line)); // 完全な空行は詰める場合

            text = string.Join("\n", lines);

            // CSV用にダブルクォートをエスケープ ( " -> "" )
            return text.Replace("\"", "\"\"");
        }
    }
}
