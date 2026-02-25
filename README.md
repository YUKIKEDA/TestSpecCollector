# TestSpecCollector

テストプロジェクトのDLLとXMLドキュメントから、テスト仕様をCSV形式で一覧化するコンソールツールです。  
NUnit / xUnit / MSTest のテストメソッドと、XMLコメント（`<preconditions>`, `<steps>`, `<expected>` など）を収集し、Excelなどで扱いやすいCSVファイルを出力します。

## 機能

- **テストメソッドの自動検出**: `[Test]`, `[Fact]`, `[Theory]`, `[TestMethod]` などの属性付きメソッドを収集
- **TestIDの自動付与**: `{クラス名}-{連番3桁}` 形式（例: `TestSample-001`）
- **XMLコメントの取り込み**: `<summary>`, `<preconditions>`, `<steps>`, `<expected>` をCSV列にマッピング
- **UTF-8 BOM付きCSV出力**: Excelでの文字化けを防止

## 必要な環境

- .NET 8.0
- テストプロジェクトのビルド済みDLLおよびXMLドキュメント（`GenerateDocumentationFile` 有効）

## プロジェクト構成

```
TestSpecCollector/
├── TestSpecCollector/          # コンソールアプリ（本ツール）
│   └── Program.cs
├── TestSpecCollectorTests/     # サンプルテストプロジェクト（xUnit）
│   └── TestSample.cs
├── TestSpecification.csv       # 出力先CSV（実行後に生成）
└── README.md
```

## 使い方

### 1. テストプロジェクトをビルドする

テストプロジェクトで **XMLドキュメントの出力** を有効にしておきます。

```xml
<!-- TestSpecCollectorTests.csproj など -->
<PropertyGroup>
  <GenerateDocumentationFile>true</GenerateDocumentationFile>
</PropertyGroup>
```

ビルド後、`bin/Debug/net8.0/` に `.dll` と `.xml` が出力されていることを確認してください。

### 2. パスを設定する

`TestSpecCollector/Program.cs` の先頭にある設定を、環境に合わせて変更します。

```csharp
// 1. テストプロジェクトのDLLのパス
string targetDllPath = @"（あなたのパス）\TestSpecCollectorTests\bin\Debug\net8.0\TestSpecCollectorTests.dll";

// 2. 生成されたXMLドキュメントのパス
string targetXmlPath = @"（あなたのパス）\TestSpecCollectorTests\bin\Debug\net8.0\TestSpecCollectorTests.xml";

// 3. 出力するCSVのパス
string outputCsvPath = @"（あなたのパス）\TestSpecification.csv";
```

### 3. 実行する

```bash
cd TestSpecCollector
dotnet run
```

コンソールに「完了しました。合計 N 件のテストを出力しました。」と表示され、指定したパスにCSVが保存されます。

## 出力CSVの形式

| 列名 | 説明 |
|------|------|
| TestID | `{クラス名}-{連番3桁}` |
| Title(MethodName) | テストメソッド名 |
| Preconditions | XMLの `<preconditions>` の内容 |
| Steps | XMLの `<steps>` の内容 |
| ExpectedResult | XMLの `<expected>` の内容 |
| Summary(Optional) | XMLの `<summary>` の内容 |

## テストコードでのXMLコメント例

ツールは以下のようなカスタムタグを認識します。

```csharp
/// <summary>
/// (省略可能) メソッドの概要
/// </summary>
/// <preconditions>
/// - 前提条件1
/// - 前提条件2
/// </preconditions>
/// <steps>
/// 1. 手順1
/// 2. 手順2
/// </steps>
/// <expected>
/// - 期待結果1
/// - 期待結果2
/// </expected>
[Fact]
public void 正常系_有効なユーザーでログインできる()
{
    // テストコード
}
```

- `preconditions`, `steps`, `expected` は標準のXMLドキュメントタグではないため、プロジェクトで「仕様用」として利用する想定です。
- これらのタグがなくても、メソッド名とTestIDだけでもCSVに出力されます。

## 対応テストフレームワーク

- **xUnit**: `[Fact]`, `[Theory]`
- **NUnit**: `[Test]`, `[TestCase]` など
- **MSTest**: `[TestMethod]` など

属性名が `Test`, `Fact`, `Theory` で始まるものをテストメソッドとして扱います。

## ライセンス

このリポジトリのライセンスに従います。
