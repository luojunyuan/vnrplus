```bash
// open terminal at vnrplus/cspipe
// ensure cx include `libs`
dotnet publish -o ~/Downloads/cx
source cxenv.sh
wine '/Users/kimika/Downloads/zetai/ぜったい絶頂☆性器の大発明!!　─処女を狙う学園道具多発エロ─.exe'

// open another terminal at ~/Download/cx
// note there is no .exe ext in the game name
source ~/RiderProjects/vnrplus/cxpipe/cxenv.sh
wine Downloads/cx/cxpipe.exe 'ぜったい絶頂☆性器の大発明!!　─処女を狙う学園道具多発エロ─'
```

publish

```
dotnet publish -o vnrplus
cp -r bin/Debug/net8.0/UniDic ~/Downloads/vnrplus
# tmp_start.sh in vnrplus -> depend on wine, game and cxpipe
# fswatch
# cxpipe libs (consider separate folder for FSharp.core.dll(net8 netfx))
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
