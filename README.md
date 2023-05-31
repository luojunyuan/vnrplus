publish

```
dotnet publish -o ~/Downloads/vnrplus
dotnet publish ../cxpipe/cxpipe.fsproj -c Release -o ~/Downloads/vnrplus
cp -r bin/Debug/net8.0/UniDic ~/Downloads/vnrplus
```

施工

すべてHook上に基づく

Fetures

* TextWindow 文本显示，出字了后再显示 (hwnd Option)
    1. 人名+原文+假名+查词
    2. (多段显示，前后翻页)
* 弾幕 Window (hwnd 必須) | Popup bubbles on Mac
* 地の文読み上げ
* 辞書 (Can use for perfomacing 地の文 振り仮名)

Windows

* TextWindow
* DanmakuWindow
* HookWindow
* MainWindow
* PreferenceWindow

Taskbar Menu

* Main Sakura Panal Dashboard
* Hook Config
* Jisho Config
* Preference
* Exit

人工字幕
* Subtitles helping tool for creator ?
* Subtitles manager

Community

Elmish
MVU
fsharp
Avalonia

https://medium.com/@MangelMaxime/my-tips-for-working-with-elmish-ab8d193d52fd
