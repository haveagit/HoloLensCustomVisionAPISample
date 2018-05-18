# HoloLensCustomVisionSample

de:code 2018 AC62「簡単！！HoloLensで始めるCognitive Services～de:code 2018特別バージョン～」の  
Custom Vision用サンプルコードです。  
HoloLensで画像キャプチャを取得し、Custom Visionを呼び出すことで\
対象物を分類分けして表示、音声出力します。

![image](https://github.com/haveagit/HoloLensCustomVisionAPISample/blob/master/Assets/image/cat.jpg)

信頼度が50%（※）を切る場合は「判定不能」としますので、\
その場合は撮影しなおしてください。\
※閾値はプログラム内で変更可能

## バージョン情報
 Unity：2017.1.2p3  
 MRToolkit：HoloToolkit-Unity-v1.2017.1.2  
 VisualStudio：15.5.4  

## 使い方

1.本PJをクローンし、Azure Custom Vision APIのimage fileのキーを  
 GetCustomVisionInfo.cs の visionAPIKeyに設定してください。  

2.上記1と同じように、visionURLにCustom Visionのimage fileのURLを設定してください。

3.エアタップで画像取得～Custom Visionの呼び出しを行います。  

# 注意点

1.AzureおよびCustom Vision 自体の操作、設定に関しては本PJ内では説明致しません。\
  Custom Visionについては下記の記事も参考になります。\
  情報は2017年のものなので古いですが、Custom Visionの設定周りはほぼ変わりませんでした（2018年5月時点）。\
 [HoloLensで始めるCognitive Services（Custom Vision Services編）](https://qiita.com/morio36/items/42ee34a1c97929d44ca2)

2.UWP Capability SettingsのWebcam,Internet Clientは必須です
