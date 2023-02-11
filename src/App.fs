module App

open Elmish
open Elmish.React
open Feliz

type State = { Count: int; Loading: bool }

type Msg =
    | Increment
    | Decrement
    | IncrementDelayed

let init () =
    { Count = 0; Loading = false }, Cmd.none

let update msg state =
    match msg with
    | Increment ->
        { state with
            Count = state.Count + 1
            Loading = false },
        Cmd.none
    | Decrement -> { state with Count = state.Count - 1 }, Cmd.none
    | IncrementDelayed when state.Loading = true -> state, Cmd.none
    | IncrementDelayed ->
        let incrementDelayedCmd dispatch =
            let delayedDispatch =
                async {
                    do! Async.Sleep 1000
                    dispatch Increment
                }

            Async.StartImmediate delayedDispatch

        { state with Loading = true }, Cmd.ofSub incrementDelayedCmd

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
                prop.text "Increment Delayed" ] ]

Program.mkProgram init update render
|> Program.withReactSynchronous "elmish-app"
|> Program.run
