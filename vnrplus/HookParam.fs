module HookParam

type HookParameter =
    { index: int
      text: string }

let convertToHookParameter (arr: string array) =
    match arr with
    | [| index; text |] ->
        { index = int index; text = text }
    | _ ->
        printfn "Invalid input"
        { index = -1; text = "" }
        
type HookParamEvent() =

    let event = Event<HookParameter>()

    [<CLIEvent>]
    member this.Event = event.Publish

    member this.TriggerEvent(arg) =
        event.Trigger(arg)
