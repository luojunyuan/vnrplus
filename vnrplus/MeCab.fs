module MeCab

open MeCab
open MeCab.Extension.UniDic
open WanaKanaNet
open System

let Get func node : string option =
    let v = func node
    if String.IsNullOrEmpty v then None else Some v
let GetPos1 node = Get UniDicFeatureExtension.GetPos1 node
let GetGoshu node = Get UniDicFeatureExtension.GetGoshu node
let GetLemma node = Get UniDicFeatureExtension.GetLemma node
let GetPron node = Get UniDicFeatureExtension.GetPron node

type Hinshi =
| 未定だ
| 名詞
| 動詞
| 形容詞
| 副詞
| 助詞
| 助動詞
| 感動詞
| 形状詞
| 代名詞
| 連体詞
| 接尾辞
| 補助記号

let toHinshi partOfSpeech =
    match partOfSpeech with
    | "名詞" -> Hinshi.名詞
    | "動詞" -> Hinshi.動詞
    | "形容詞" -> Hinshi.形容詞
    | "副詞" -> Hinshi.副詞
    | "助詞" -> Hinshi.助詞
    | "助動詞" -> Hinshi.助動詞
    | "感動詞" -> Hinshi.感動詞
    | "形状詞" -> Hinshi.形状詞
    | "代名詞" -> Hinshi.代名詞
    | "連体詞" -> Hinshi.連体詞
    | "接尾辞" -> Hinshi.接尾辞
    | "補助記号" -> Hinshi.補助記号
    | _ -> Hinshi.未定だ

type MeCabWord =
    { Word: string
      Kana: string
      PartOfSpeech: Hinshi }
    
let parameter = MeCabParam()
parameter.DicDir <- System.IO.Path.Combine(AppContext.BaseDirectory, "UniDic")
let tagger =  MeCabTagger.Create parameter

let generateWords text =
    tagger.ParseToNodes text
    |> Seq.filter (fun n -> n.CharType > 0u)
    |> Seq.map (fun n ->
                           let hinshi = (defaultArg (GetPos1 n) "") |> toHinshi
                           { Word = n.Surface
                             Kana = match (defaultArg (GetGoshu n) "") with
                                    | "外" -> Array.last ((defaultArg (GetLemma n) " ").Split '-')
                                    | _ -> match not (WanaKana.IsKana n.Surface) && hinshi <> Hinshi.補助記号 with
                                           | true -> WanaKana.ToHiragana (defaultArg (GetPron n) "")
                                           | false -> ""
                             PartOfSpeech = hinshi })
