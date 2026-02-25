namespace TestSpecCollectorTests
{
    /// <summary>
    /// テストサンプルクラス
    /// </summary>
    public class TestSample
    {
        // パターン1: 完全な仕様記述
        // メソッド名がそのまま「テストタイトル」になります。
        /// <summary>
        /// (省略可能) メソッド名で十分な場合はsummaryを書かなくてOK
        /// </summary>
        /// <preconditions>
        /// - 登録済みの有効なユーザーが存在する
        /// - ログイン画面を開いている
        /// </preconditions>
        /// <steps>
        /// 1. ユーザーID "testuser" を入力
        /// 2. パスワード "password123" を入力
        /// 3. 「ログイン」ボタンを押下
        /// </steps>
        /// <expected>
        /// - ダッシュボード画面へ遷移すること
        /// - ヘッダーに「ようこそ」と表示されること
        /// </expected>
        [Fact]
        public void 正常系_有効なユーザーでログインできる()
        {
            // Test Code...
        }

        // パターン2: 最小限の記述
        // XMLコメントがなくても、メソッド名だけで一覧化可能です。
        [Fact]
        public void 異常系_無効なパスワードでエラーになる()
        {
            // Test Code...
        }

        // パターン3: 属性なし（IDなし）
        /// <summary>
        /// 管理者権限の確認テスト
        /// </summary>
        /// <expected>設定画面が開くこと</expected>
        [Fact]
        public void 管理者メニューが表示される()
        {
        }
    }
}
