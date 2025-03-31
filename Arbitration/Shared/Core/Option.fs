module Arbitration.Shared.Core.Option

open System

let inline toVOption (defaultValue: 'T) (value: Nullable<'T>) : 'T =
    value
    |> ValueOption.ofNullable
    |> ValueOption.defaultWith (fun () -> defaultValue)