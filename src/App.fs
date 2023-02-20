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

type Deferred<'t> =
    | NotStarted
    | InProgress
    | Resolved of 't

type DeferredRandom = Deferred<Result<double, string>>

type State =
    { Count: int
      Loading: bool
      RandomNumber: DeferredRandom }

type AsyncOperationStatus<'t> =
    | Started
    | Finished of 't

type Msg =
    | Increment
    | Decrement
    | IncrementDelayed
    | DecrementDelayed
    | GenerateRandomNumber of AsyncOperationStatus<Result<double, string>>

let rnd = System.Random()

let init () =
    { Count = 0
      Loading = false
      RandomNumber = NotStarted },
    Cmd.none


let delayedMsg msg =
    async {
        do! Async.Sleep 1000
        return msg
    }

let generateRandomNumber () =
    async {
        let nextDouble = rnd.NextDouble()
        do! Async.Sleep 200
        return GenerateRandomNumber(Finished(Ok nextDouble))
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
    | GenerateRandomNumber Started when state.RandomNumber = InProgress -> state, Cmd.none
    | GenerateRandomNumber Started ->
        let task =
            async {
                let nextDouble = rnd.NextDouble()
                do! Async.Sleep 200

                if (nextDouble < 0.5) then
                    let errMsg = sprintf "Failed! Random number %f was less than 0.5" nextDouble
                    return GenerateRandomNumber(Finished(Error errMsg))
                else
                    return GenerateRandomNumber(Finished(Ok nextDouble))
            }

        { state with RandomNumber = InProgress }, Cmd.fromAsync (task)
    | GenerateRandomNumber(Finished x) -> { state with RandomNumber = Resolved x }, Cmd.none

let render (state: State) (dispatch: Msg -> unit) =
    printfn "rendering"

    let content =
        if state.Loading then
            Html.h1 "LOADING..."
        else
            Html.h1 state.Count

    let randomBit (numberState: DeferredRandom) =
        match numberState with
        | NotStarted -> Html.h1 "Nothing yet"
        | InProgress -> Html.h1 "Loading"
        | Resolved (Ok number) ->
            Html.h1 [
                prop.style [ style.color.green ]
                prop.text (sprintf "Successfully generated random number: %f" number)
            ]

        | Resolved (Error errorMsg) ->
            Html.h1 [
                prop.style [ style.color.crimson ]
                prop.text errorMsg
            ]

    Html.div
        [ Html.button
              [ prop.onClick (fun _ -> dispatch (GenerateRandomNumber Started))
                prop.text "Randomize me" ]
          randomBit state.RandomNumber
          content
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
