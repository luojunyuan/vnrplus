module Types

type HookParameter =
    { index: int
      text: string }
    
type HookParamEvent() =

    let event = Event<HookParameter>()

    [<CLIEvent>]
    member this.Event = event.Publish

    member this.TriggerEvent(arg) =
        event.Trigger(arg)

let hookParamEvent = HookParamEvent()
