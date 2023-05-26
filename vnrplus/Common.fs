module Common

open System

let inline dispose (x: IDisposable) = x.Dispose()
