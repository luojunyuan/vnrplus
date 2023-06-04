module MeCab

open MeCab
open MeCab.Extension.UniDic
open WanaKanaNet
open System

let GetPos1 node = node |> UniDicFeatureExtension.GetPos1 |> Common.optionStr

let GetGoshu node = node |> UniDicFeatureExtension.GetGoshu |> Common.optionStr

let GetLemma node = node |> UniDicFeatureExtension.GetLemma |> Common.optionStr

let GetPron node = node |> UniDicFeatureExtension.GetPron |> Common.optionStr

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

let createTagger ()=
    let parameter = MeCabParam()
    parameter.DicDir <- Common.unidicDir
    MeCabTagger.Create parameter

let private isCharType (node: MeCabNode) = node.CharType > 0u

let generateWords (tagger: MeCabTagger) text =
    text
    |> tagger.ParseToNodes 
    |> Seq.filter isCharType
    |> Seq.map (fun n ->
        let hinshi = n |> GetPos1 |> Option.defaultValue "" |> toHinshi // 品詞
        let goshu = n |> GetGoshu |> Option.defaultValue "" // 語種
        let lemma = n |> GetLemma |> Option.defaultValue " " // should never be Option
        let isKanji = not (n.Surface |> WanaKana.IsKana)
        
        { Word = n.Surface
          Kana =
            match goshu with
            | "外" -> lemma |> Common.split "-" |> Array.last
            | _ ->
                match isKanji && hinshi <> Hinshi.補助記号 with
                | true -> WanaKana.ToHiragana(GetPron n |> Option.defaultValue "")
                | false -> ""
          PartOfSpeech = hinshi })
