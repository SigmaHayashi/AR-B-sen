# AR B-sen


# 概要
ROS-TMSのデータベースに蓄えられた情報をAR Coreを用いて実空間にグラフィカルに重畳表示することで，見ただけではわからない情報を可視化するAndroidアプリケーション


# 現在可視化されるデータ
* 知的冷蔵庫の中の物品の種類，位置，関連する日時
* 心拍センサをつけている人の心拍波形，心拍数，体表温
* SmartPalのバッテリー残量


# 必要な環境
PC1 : Windows10 64bit（アプリケーションビルド用）  
PC2 : Ubuntu 16（Smart Previewed Reality実行用）  
※PC1とPC2は同時に起動する必要なし，デュアルブートでOK

スマートフォン : ARCore対応Android端末（Pixel 4 XL推奨）  
参考：https://developers.google.com/ar/discover/supported-devices?hl=ja

ROS kinetic (Ubuntuにインストールしておく)


# 開発環境
PC : Windows 10 64bit  
* Unity 2018.4.1f1  
* Visual Studio 2017  
* Android Studio 3.5.1  

Android（動作確認済み） : Pixel 3 XL, Pixel 4 XL


# アプリケーションをビルドするためのPCの準備
1. Unityのインストール  
    URL : https://unity3d.com/jp/get-unity/download

1. Visual Studioのインストール  
    ※VS Codeではない  
    ※Unityのインストール中にインストールされるものでOK  
    URL : https://visualstudio.microsoft.com/ja/downloads/

1. Android Studioのインストール  
    ※Android SDKが必要  
    URL : https://developer.android.com/studio


# アプリケーションのインストール方法
1. GitHubから任意の場所にダウンロード

1. Unityでプロジェクトを開く
1. "AR B-sen"のSceneを開く
1. File > Build Settingsからビルド環境の設定を開く
1. Androidを選択し，Switch Platformを選択
1. Android端末をPCに接続し，Build & Run


# 使い方

## ROS-TMS for Smart Previewed Realityの実行

実行前に，ROSをインストールしたUbuntuでROS-TMS for Smart Previewed Realityをcatkin_makeしておく必要がある．

ROS-TMS for Smart Previewed Reality : https://github.com/SigmaHayashi/ros_tms_for_smart_previewed_reality

このアプリケーションをフルに利用するためには，B-sen，SmartPal V，Vicon，WHS-1が必要である．
また，データベースを利用するため，mongodbをインストールする必要がある．その他依存関係はROS-TMSのWikiを参照．

Wiki : https://github.com/irvs/ros_tms/wiki


### 実行手順
```
$ roscore
$ roslaunch rosbridge_server rosbridge_websocket.launch
$ roslaunch tms_db_manager tms_db_manager.launch
$ rosrun tms_ss_vicon vicon_stream
$ rosrun tms_ss_whs1 tms_ss_whs1
```

別のWindowsPCでWHS-1から情報を取得してROS-TMSのデータベースに情報を送信するアプリケーションを起動する．(whs1_client/Debug/**whs1_client.exe**)

whs1_client : https://github.com/irvs/whs1_client


## アプリの操作

1. Smart Previewed Realityアプリケーションを起動

    ※初回起動時は，Settingsボタンを押してROS-TMSを実行しているUbuntu PCのIPアドレスを指定する必要あり（うまく起動しない場合はWi-Fiを一度オフにしてからアプリを起動するとスムーズに起動するかも）

1. 起動直後の画面のまま画像マーカを認識させることで位置合わせを行う

1. 自己位置の微調整を行う
    1. Calibrationボタンを押す
    1. XYZ方向の移動，回転を調整する
    1. Back to Mainボタンを押す

1. 冷蔵庫，SmartPal，ベッドに近づくことでそれぞれに関連する情報が表示される
