module App

open Elmish
open Elmish.React
open Feliz
open System

type Todo =
    { Id: Guid
      Description: string
      EditDescription: string
      BeingEdited: bool
      Completed: bool }

type Filter =
    | Completed
    | All
    | NotCompleted

type State =
    { TodoList: Todo list
      NewTodo: string
      Filter: Filter }

type Msg =
    | AddNewTodo
    | SetNewTodo of string
    | DeleteTodo of Guid
    | ToggleCompleted of Guid
    | StartEditingTodo of Guid
    | SetEditedDescription of Guid * string
    | CancelEdit of Guid
    | ApplyEdit of Guid
    | SetFilter of Filter

let init () =
    { TodoList =
        [ { Id = Guid.NewGuid()
            Description = "initial"
            EditDescription = "initial"
            BeingEdited = false
            Completed = false }
          { Id = Guid.NewGuid()
            Description = "done this one"
            EditDescription = "done this one"
            BeingEdited = false
            Completed = true }
          { Id = Guid.NewGuid()
            Description = "being edited"
            EditDescription = "being edited"
            BeingEdited = true
            Completed = false } ]
      NewTodo = ""
      Filter = All }


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
              EditDescription = state.NewTodo
              BeingEdited = false
              Completed = false }
            :: state.TodoList }


let update (msg: Msg) (state: State) : State =
    printfn "Updating state %A" state

    match msg with
    | SetFilter filter -> { state with Filter = filter }
    | CancelEdit id ->
        { state with
            TodoList =
                state.TodoList
                |> List.map (fun todo ->
                    if todo.Id = id then
                        { todo with BeingEdited = false }
                    else
                        todo) }
    | ApplyEdit id ->
        let todoList =
            state.TodoList
            |> List.map (fun todo ->
                if todo.Id = id then
                    { todo with
                        Description = todo.EditDescription
                        BeingEdited = false }
                else
                    todo)

        { state with TodoList = todoList }

    | StartEditingTodo id ->
        let todoList =
            state.TodoList
            |> List.map (fun todo ->
                if todo.Id = id then
                    { todo with BeingEdited = true }
                else
                    todo)


        { state with TodoList = todoList }
    | SetEditedDescription(id, edit) ->
        let todoList =
            state.TodoList
            |> List.map (fun todo ->
                if todo.Id = id then
                    { todo with EditDescription = edit }
                else
                    todo)

        { state with TodoList = todoList }

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

let renderEditForm (todo: Todo) dispatch =
    div
        [ "box" ]
        [ div
              [ "field"; "is-grouped" ]
              [ div
                    [ "control"; "is-expanded" ]
                    [ controlledInput todo.EditDescription (fun desc ->
                          dispatch (SetEditedDescription(todo.Id, desc))) ]

                div
                    [ "control"; "buttons" ]
                    [ Html.button
                          [ prop.classes
                                [ "button"
                                  if todo.Description <> todo.EditDescription then
                                      "is-primary" ]
                            prop.onClick (fun _ -> dispatch (ApplyEdit todo.Id))
                            prop.children [ Html.i [ prop.classes [ "fa"; "fa-save" ] ] ] ]

                      Html.button
                          [ prop.classes [ "button"; "is-warning" ]
                            prop.onClick (fun _ -> dispatch (CancelEdit todo.Id))
                            prop.children [ Html.i [ prop.classes [ "fa"; "fa-arrow-right" ] ] ] ] ] ] ]

let todoList state (dispatch: Msg -> unit) =
    let activeTodos =
        state.TodoList
        |> List.filter (fun todo ->
            match state.Filter with
            | All -> true
            | Completed -> todo.Completed
            | NotCompleted -> not todo.Completed)

    Html.ul
        [ prop.children
              [ for todo in activeTodos -> if todo.BeingEdited then (renderEditForm todo dispatch) else renderTodo todo dispatch ] ]

let renderFilterTabs (state: State) (dispatch: Msg -> unit) =
    div
        [ "tabs"; "is-toggle"; "is-fullwidth" ]
        [ Html.ul
              [ Html.li
                    [ prop.className (if state.Filter = All then "is-active" else "")
                      prop.children [ Html.a [ prop.text "All"; prop.onClick (fun _ -> dispatch (SetFilter All)) ] ] ]

                Html.li
                    [ prop.className (if state.Filter = Completed then "is-active" else "")
                      prop.children
                          [ Html.a
                                [ prop.text "Completed"
                                  prop.onClick (fun _ -> dispatch (SetFilter Completed)) ] ] ]

                Html.li
                    [ prop.className (if state.Filter = NotCompleted then "is-active" else "")
                      prop.children
                          [ Html.a
                                [ prop.text "Not Completed"
                                  prop.onClick (fun _ -> dispatch (SetFilter NotCompleted)) ] ] ] ] ]

let render (state: State) (dispatch: Msg -> unit) =
    Html.div
        [ appTitle
          newTodoInputField state dispatch
          renderFilterTabs state dispatch
          todoList state dispatch ]

Program.mkSimple init update render
|> Program.withReactSynchronous "elmish-app"
|> Program.run


// continue from here! https://zaid-ajaj.github.io/the-elmish-book/#/chapters/elm/todo-app-exercises
