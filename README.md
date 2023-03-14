# OBSAutoReplayBuffer

特定のアプリケーションの起動、停止と[OBS Studio](https://github.com/obsproject/obs-studio)のリプレイバッファを連動させるアプリケーションです。

GeForce Experienceのインスタントリプレイの代わりにOBSのリプレイバッファを用いる場合、スタートアップに`obs64.exe --startreplaybuffer --minimize-to-tray`を登録する方法が主流ですが、この方法では録画対象のウィンドウがないときにもエンコード、記録が行われてしまい、余計な負荷がかかります(「真っ黒な画面」のエンコード負荷はなぜかゲーム画面のそれより倍以上に高い)。OBSAutoReplayBufferは「必要な時だけリプレイバッファ」を実現します。

## スタートアップ登録

以下の手順でログインと同時に実行できます。一瞬だけウィンドウが表示されますが、バックグラウンドで動作します。

1. `ファイル名を指定して実行`かエクスプローラのアドレスバーに`shell:startup`と入力し、スタートアップフォルダを開く
2. 右クリック→新規作成→ショートカット
3. 項目の場所に`powershell.exe -Command "Start-Process -WindowStyle Hidden 'OBSAutoReplayBuffer.exeの絶対パス' '<プロセス名> [オプション]'"`を入力し進める

UIがないので、終了するにはタスクマネージャで探し出して終了するなり`taskkill /im obsautoreplaybuffer.exe /f`するなりしてください。

## コマンド

```
OBSAutoReplayBuffer.exe <プロセス名> [オプション]
```

### 引数

#### プロセス名

連動させるアプリケーションのプロセス名を指定します。半角スペース区切りで複数指定できます。

実行ファイル名と違うのかどうかよく分かりませんが、PowerShellを用いて以下のようにウィンドウタイトルからプロセス名を取得できます。

```
PS > Get-Process | Where-Object {$_.MainWindowTitle -like "*War Thunder*"} | Select-Object ProcessName,MainWindowTitle

ProcessName MainWindowTitle
----------- ---------------
aces        War Thunder
```

### オプション

#### --obs ファイルパス

OBSの実行ファイルのパスを指定します。指定しなかった場合は`C:\Program Files\obs-studio\bin\64bit\obs64.exe`とします。

#### --port ポート番号

obs-websocketのサーバーポートを指定します。指定しなかった場合は`4455`とします。

#### --password パスワード

obs-websocketのサーバーパスワードを指定します。認証がオフの場合は指定しなくてよいです。

#### --help

ヘルプを表示します。処理は実行されません。

## 機能

実行時にOBSが起動していない場合は起動します。

指定したプロセスのうち1つが起動すると、リプレイバッファを開始します。OBSが起動していない場合は起動します。

指定したプロセスが1つも起動していない状態になると、リプレイバッファを停止します。OBSは起動したままです。
