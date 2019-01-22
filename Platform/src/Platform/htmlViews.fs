module Platform.HtmlViews

open Giraffe.GiraffeViewEngine
open Platform.Model.Input
open Platform.Model.Data

open Platform.Model.Domain

// ---------------------------------
// Views
// ---------------------------------

let studentTableTemplate (students: Student list) = div[] [
  table [_class "table"] [
    thead [] [
      tr [] [
        th [ _scope "col"] [encodedText "id"]
      ]
    ]
    tbody [] [
      yield!
        students
        |> List.map (fun req -> tr [] [
          td [] [ encodedText (string req) ]
        ])
    ]
  ]
]

let navbar () = ([
    ul [_class "nav nav-tabs bg-primary"] [
        li [_class "nav-item"] [
            a [ _class "nav-link active"; _href "/" ] [ str "index"]
        ]
        li [_class "nav-item"] [
            a [ _class "nav-link active"; _href "/student" ] [ str "student" ]
        ]
        li [_class "nav-item"] [
            a [ _class "nav-link active"; _href "/teacher" ] [ str "teacher" ]
        ]
        li [_class "nav-item"] [
            a [ _class "nav-link active"; _href "/head" ] [ str "head of study" ]
        ]
    ]
])

let layout (content: XmlNode list) =
        html [] [
            head [] [
                title []  [ encodedText "Platform" ]
                link [ _rel  "stylesheet"
                       _type "text/css"
                       _href "/main.css" ]
                link [ _rel  "stylesheet"
                       _href "https://fonts.googleapis.com/css?family=Roboto:300,400,500,700|Material+Icons" ]
                link [ _rel  "stylesheet"
                       _href "https://unpkg.com/bootstrap-material-design@4.1.1/dist/css/bootstrap-material-design.min.css" ]

            ]
            body [] (navbar()@ [
                div [_class "container"] content
            ])
        ]



let teacherTemplate (students: Student list) = div[] [
  form [_action "/submitrequest"; _method "post"] [
                p [] [ encodedText "title" ]
                input [_type "text"; _name "title"] //type="text" name="lastname"
                p [] [ encodedText "description" ]
                input [_type "text"; _name "description"]
                p [] [ encodedText "teacher" ]
                input [_type "text"; _name "teacher"]
                br []
                input [_type "submit"; _value "Request"]
  ]
  div [] [
      h1 [] [encodedText "students"]
      studentTableTemplate students
  ]
]






let projectTableTemplate (requests: ProjectData list) = div[] [
  table [_class "table"] [
    thead [] [
      tr [] [
        th [ _scope "col"] [encodedText "id"]
        th [ _scope "col"] [encodedText "title"]
        th [ _scope "col"] [encodedText "description"]
        th [ _scope "col"] [encodedText "teacher"]
      ]
    ]
    tbody [] [
      yield!
        requests
        |> List.map (fun req -> tr [] [
          td [] [ encodedText (string req.id) ]
          td [] [ encodedText (string req.title) ]
          td [] [ encodedText (string req.description) ]
          td [] [ encodedText (string req.teacher) ]
        ])
    ]
  ]
]

let studentsTable (students: Student list) = div[] [
  table [_class "table"] [
    thead [] [
      tr [] [
        th [ _scope "col"] [encodedText "id"]
      ]
    ]
    tbody [] [
      yield!
        students
        |> List.map (fun s -> tr [] [
          td [] [ encodedText s ]
        ])
    ]
  ]
]

let headOfStudyView (requests: ProjectData list)(students: Student list) = div [] [
    projectTableTemplate requests
    h2 [] [encodedText "students"]
    studentsTable students
    form [_action "/api/student"; _method "post"] [
                p [] [ encodedText "id" ]
                input [_type "text"; _name "id"] 
                br []
                input [_type "submit"; _value "add Student"]
  ]
]


