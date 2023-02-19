module App

open Elmish
open Elmish.React
open Feliz

module Cmd =
    let fromAsync (operation: Async<'msg>) : Cmd<'msg> =
        let sub (dispatch: 'msg -> unit) : unit =
            let delayedDispatch =
                async {
                    let! msg = operation
                    dispatch msg
                }

            Async.StartImmediate delayedDispatch

        Cmd.ofSub sub

type State = { Count: int; Loading: bool }

type Msg =
    | Increment
    | Decrement
    | IncrementDelayed
    | DecrementDelayed

let init () =
    { Count = 0; Loading = false }, Cmd.none


let delayedMsg msg =
    async {
        do! Async.Sleep 1000
        return msg
    }


let update msg state =
    match msg with
    | Increment ->
        { state with
            Count = state.Count + 1
            Loading = false },
        Cmd.none
    | Decrement -> { state with Count = state.Count - 1 }, Cmd.none
    | IncrementDelayed when state.Loading = true -> state, Cmd.none
    | IncrementDelayed -> { state with Loading = true }, Cmd.fromAsync (delayedMsg Increment)
    | DecrementDelayed when state.Loading = true -> state, Cmd.none
    | DecrementDelayed -> state, Cmd.fromAsync (delayedMsg Decrement)

let render (state: State) (dispatch: Msg -> unit) =
    printfn "rendering"

    let content =
        if state.Loading then
            Html.h1 "LOADING..."
        else
            Html.h1 state.Count

    Html.div
        [ content
          Html.button [ prop.onClick (fun _ -> dispatch Increment); prop.text "Increment" ]

          Html.button [ prop.onClick (fun _ -> dispatch Decrement); prop.text "Decrement" ]

          Html.button
              [ prop.onClick (fun _ -> dispatch IncrementDelayed)
                prop.disabled state.Loading
                prop.text "Increment Delayed" ]

          Html.button
              [ prop.onClick (fun _ -> dispatch DecrementDelayed)
                prop.disabled state.Loading
                prop.text "Decrement Delayed" ] ]

Program.mkProgram init update render
|> Program.withReactSynchronous "elmish-app"
|> Program.run

// continue from here: https://zaid-ajaj.github.io/the-elmish-book/#/chapters/commands/async-state
