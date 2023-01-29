module App

open Elmish
open Elmish.React
open Feliz
open System

type Todo =
    { Id: Guid
      Description: string
      Completed: bool }

type TodoBeingEdited = { Id: Guid; Description: string }

type State =
    { TodoList: Todo list
      NewTodo: string
      TodoBeingEdited: TodoBeingEdited option }

type Msg =
    | AddNewTodo
    | SetNewTodo of string
    | DeleteTodo of Guid
    | ToggleCompleted of Guid
    | StartEditingTodo of Guid
    | SetEditedDescription of string
    | CancelEdit
    | ApplyEdit

let init () =
    { TodoList =
        [ { Id = Guid.NewGuid()
            Description = "initial"
            Completed = false } ]
      NewTodo = ""
      TodoBeingEdited = None }


let toggleCompleted state id =
    state.TodoList
    |> List.map (fun todo ->
        if todo.Id = id then
            { todo with
                Completed = not todo.Completed }
        else
            todo)

let addNewTodo state =
    { state with
        NewTodo = ""
        TodoList =
            { Id = Guid.NewGuid()
              Description = state.NewTodo
              Completed = false }
            :: state.TodoList }


let update (msg: Msg) (state: State) : State =
    printfn "Updating state %A" state

    match msg with
    | CancelEdit -> { state with TodoBeingEdited = None }
    | ApplyEdit ->
        match state.TodoBeingEdited with
        | None -> state
        | Some edited when edited.Description = "" -> state
        | Some edited ->
            let mapToEdited (todos: Todo list) =
                todos
                |> List.map (fun todo ->
                    if todo.Id = edited.Id then
                        { todo with
                            Description = edited.Description }
                    else
                        todo)

            let newTodoList = mapToEdited state.TodoList

            { state with
                TodoList = newTodoList
                TodoBeingEdited = None }

    | StartEditingTodo id ->
        let editedTodo =
            state.TodoList
            |> List.tryFind (fun todo -> todo.Id = id)
            |> Option.map (fun todo ->
                { Id = todo.Id
                  Description = todo.Description })

        { state with
            TodoBeingEdited = editedTodo }
    | SetEditedDescription edit ->
        { state with
            TodoBeingEdited =
                state.TodoBeingEdited
                |> Option.map (fun todo -> { todo with Description = edit }) }

    | SetNewTodo newTodo -> { state with NewTodo = newTodo }
    | DeleteTodo id ->
        { state with
            TodoList = state.TodoList |> List.filter (fun todo -> todo.Id <> id) }
    | AddNewTodo when state.NewTodo = "" -> state
    | AddNewTodo -> addNewTodo state
    | ToggleCompleted id ->
        { state with
            TodoList = toggleCompleted state id }

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

let controlledButton onClick icon =
    Html.div
        [ prop.classes [ "control" ]
          prop.children
              [ Html.button
                    [ prop.classes [ "button"; "is-medium"; "is-primary" ]
                      prop.onClick onClick
                      prop.children [ Html.i [ prop.classes [ "fa"; icon ] ] ] ] ] ]

let newTodoInputField state dispatch =
    Html.div
        [ prop.classes [ "field"; "has-addons" ]
          prop.children
              [ controlledInput state.NewTodo (SetNewTodo >> dispatch)
                controlledButton (fun _ -> dispatch AddNewTodo) "fa-plus" ] ]


/// Helper function to easily construct div with only classes and children
let div classes (children: Fable.React.ReactElement list) =
    Html.div [ prop.classes classes; prop.children children ]

let editTodoInputField state dispatch =
    div
        [ "field"; "has-addons" ]
        [ controlledInput state.NewTodo (SetNewTodo >> dispatch)
          controlledButton (fun _ -> dispatch AddNewTodo) "fa-plus" ]

let renderTodo (todo: Todo) (dispatch: Msg -> unit) =
    div
        [ "box" ]
        [ div
              [ "columns"; "is-mobile"; "is-vcentered" ]
              [ div [ "column" ] [ Html.p [ prop.className "subtitle"; prop.text todo.Description ] ]

                div
                    [ "column"; "is-narrow" ]
                    [ div
                          [ "buttons" ]
                          [ Html.button
                                [ prop.classes
                                      [ "button"
                                        if todo.Completed then
                                            "is-success" ]
                                  prop.onClick (fun _ -> dispatch (ToggleCompleted todo.Id))
                                  prop.children [ Html.i [ prop.classes [ "fa"; "fa-check" ] ] ] ]
                            Html.button
                                [ prop.classes [ "button"; "is-primary" ]
                                  prop.onClick (fun _ -> dispatch (StartEditingTodo todo.Id))
                                  prop.children [ Html.i [ prop.classes [ "fa"; "fa-edit" ] ] ] ]
                            Html.button
                                [ prop.classes [ "button"; "is-danger" ]
                                  prop.onClick (fun _ -> dispatch (DeleteTodo todo.Id))
                                  prop.children [ Html.i [ prop.classes [ "fa"; "fa-times" ] ] ] ] ] ] ] ]

let renderEditForm todoBeingEdited dispatch =
    div
        [ "box" ]
        [ div
              [ "field"; "is-grouped" ]
              [ div
                    [ "control"; "is-expanded" ]
                    [ controlledInput todoBeingEdited.Description (SetEditedDescription >> dispatch) ]
                // [ Html.input
                //       [ prop.classes [ "input"; "is-medium" ]
                //         prop.valueOrDefault todoBeingEdited.Description
                //         prop.onTextChange (SetEditedDescription >> dispatch) ] ]

                div
                    [ "control"; "buttons" ]
                    [ Html.button
                          [ prop.classes [ "button"; "is-primary" ]
                            prop.onClick (fun _ -> dispatch ApplyEdit)
                            prop.children [ Html.i [ prop.classes [ "fa"; "fa-save" ] ] ] ]

                      Html.button
                          [ prop.classes [ "button"; "is-warning" ]
                            prop.onClick (fun _ -> dispatch CancelEdit)
                            prop.children [ Html.i [ prop.classes [ "fa"; "fa-arrow-right" ] ] ] ] ] ] ]

let todoList state (dispatch: Msg -> unit) =
    Html.ul
        [ prop.children
              [ for todo in state.TodoList ->
                    match state.TodoBeingEdited with
                    | Some todoBeingEdited when todoBeingEdited.Id = todo.Id -> renderEditForm todoBeingEdited dispatch
                    | _ -> renderTodo todo dispatch ] ]

// TODO: can we get onto the 'Some' path earlier? i.e. find todoBeingEdited before looping over TodoList?

let render (state: State) (dispatch: Msg -> unit) =
    Html.div [ appTitle; newTodoInputField state dispatch; todoList state dispatch ]

Program.mkSimple init update render
|> Program.withReactSynchronous "elmish-app"
|> Program.run


// continue from here! https://zaid-ajaj.github.io/the-elmish-book/#/chapters/elm/todo-app-exercises