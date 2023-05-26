module Common

let pipeOutFile = @"z:\tmp\wine_out"

let replaceEmpty (oldValue:string) (message:string) = message.Replace(oldValue, "")
