# OBSAutoReplayBuffer

指定したアプリケーションの起動、停止に合わせて[OBS Studio](https://github.com/obsproject/obs-studio)のリプレイバッファを開始、終了するアプリケーションです。

GeForce Experienceのインスタントリプレイの代わりにOBSのリプレイバッファを用いる場合、スタートアップに`obs64.exe --startreplaybuffer --minimize-to-tray`を登録する方法が主流ですが、この方法では録画対象のウィンドウがないときにもエンコード、記録が行われてしまい、余計な負荷がかかります(「真っ黒な画面」のエンコード負荷はなぜかゲーム画面のそれより倍以上に高い)。OBSAutoReplayBufferは「必要な時だけリプレイバッファ」を実現します。

[OBSNotification](https://gist.github.com/OER1057/86fe13dc46704fd940ac14ebd5ceaf95#file-obsnotification-md)を組み合わせて使用するとよいでしょう。

## 機能

実行時にOBSが起動していない場合は起動します。

指定したプロセスのうち1つが起動すると、リプレイバッファを開始します。OBSが起動していない場合は起動します。

指定したプロセスが1つも起動していない状態になると、リプレイバッファを停止します。OBSは起動したままです。

## 使用法

### obs-websocket有効化

WebSocket経由でOBSの操作を行うので、obs-websocketを有効にしてください。標準で搭載されています。

### コマンド

```
OBSAutoReplayBuffer.exe <プロセス名> [設定]
```

#### プロセス名

連動させるアプリケーションのプロセス名を指定します。半角スペース区切りで複数指定できます。

実行ファイル名と違うのかどうかよく分かりませんが、PowerShellを用いて以下のようにウィンドウタイトルからプロセス名を取得できます。

```
PS > Get-Process | Where-Object {$_.MainWindowTitle -like "*War Thunder*"} | Select-Object ProcessName,MainWindowTitle

ProcessName MainWindowTitle
----------- ---------------
aces        War Thunder
```

#### オプション

- `--obs ファイルパス` : OBSの実行ファイルのパスを指定します。指定しなかった場合は`C:\Program Files\obs-studio\bin\64bit\obs64.exe`とします。
- `--port ポート番号` : obs-websocketのサーバーポートを指定します。指定しなかった場合は`4455`とします。
- `--password パスワード` : obs-websocketのサーバーパスワードを指定します。指定しなかった場合は認証なしとします。
- `--help` : ヘルプを表示します。処理は実行されません。

### スタートアップ登録

目的の設定で正しく動作することを確認したら、以下の手順でログインと同時に実行するようにします。一瞬だけウィンドウが表示されますが、バックグラウンドで動作します。

1. `ファイル名を指定して実行`かエクスプローラのアドレスバーに`shell:startup`と入力し、スタートアップフォルダを開く
2. 右クリック→新規作成→ショートカット
3. 項目の場所に`powershell.exe -Command "Start-Process -WindowStyle Hidden 'OBSAutoReplayBuffer.exeの絶対パス' '<プロセス名> [設定]'"`を入力し進める

UIがないので、終了するにはタスクマネージャで探し出して終了するなり`taskkill /im obsautoreplaybuffer.exe /f`するなりしてください。