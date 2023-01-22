module App

open Elmish
open Elmish.React
open Feliz

type State =
    { TodoList: string list
      NewTodo: string }

type Msg =
    | AddNewTodo
    | SetNewTodo of string

let init () =
    { TodoList = [ "initial"; "state" ]
      NewTodo = "" }

let update (msg: Msg) (state: State) : State =
    printfn "Updating state %A" state

    match msg with
    | SetNewTodo newTodo -> { state with NewTodo = newTodo }
    | AddNewTodo when state.NewTodo = "" -> state
    | AddNewTodo ->
        printfn "Adding new todo: %s" state.NewTodo

        { TodoList = state.NewTodo :: state.TodoList
          NewTodo = "" }

let appTitle = Html.h1 [ prop.className "title"; prop.text "Elmish To-Do List" ]

let todoInput state dispatch =
    Html.input [ prop.value state.NewTodo; prop.onChange (SetNewTodo >> dispatch) ]

let addTodoButton dispatch =
    Html.button [ prop.onClick (fun _ -> dispatch AddNewTodo); prop.text "Add" ]

let controlledInput (value: string) (onChange: string -> unit) =
    Html.div
        [ prop.classes [ "control"; "is-expanded" ]
          prop.children
              [ Html.input
                    [ prop.classes [ "input"; "is-medium" ]
                      prop.valueOrDefault value
                      prop.onChange onChange ] ] ]

let controlledButton onClick =
    Html.div
        [ prop.classes [ "control" ]
          prop.children
              [ Html.button
                    [ prop.classes [ "button"; "is-medium"; "is-primary" ]
                      prop.onClick onClick
                      prop.children [ Html.i [ prop.classes [ "fa"; "fa-plus" ] ] ] ] ] ]

let inputField state dispatch =
    Html.div
        [ prop.classes [ "field"; "has-addons" ]
          prop.children
              [ controlledInput state.NewTodo (SetNewTodo >> dispatch)
                controlledButton (fun _ -> dispatch AddNewTodo) ] ]

let todoList state dispatch =
    Html.ul (
        state.TodoList
        |> List.map (fun todo -> Html.li [ prop.text todo; prop.classes [ "box"; "subtitle" ] ])
    )

let twodoList state =
    Html.ul [ for todo in state.TodoList -> Html.li [ prop.text todo; prop.classes [ "box"; "subtitle" ] ] ]


let render (state: State) (dispatch: Msg -> unit) =
    Html.div [ appTitle; inputField state dispatch; twodoList state ]

Program.mkSimple init update render
|> Program.withReactSynchronous "elmish-app"
|> Program.run


// continue from here! https://zaid-ajaj.github.io/the-elmish-book/#/chapters/elm/todo-app-part2