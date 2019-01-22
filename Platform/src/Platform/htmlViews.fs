module Platform.HtmlViews

open Giraffe.GiraffeViewEngine


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

let teacherTemplate () = div[] [
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
]

let headOfStudyTemplate () = div[] [
  form [_action "/submitrequest"; _method "post"] [
                p [] [ encodedText "headdsds" ]
                input [_type "text"; _name "title"] //type="text" name="lastname"
                p [] [ encodedText "description" ]
                input [_type "text"; _name "description"]
                p [] [ encodedText "teacher" ]
                input [_type "text"; _name "teacher"]
                br []
                input [_type "submit"; _value "Request"]
  ]
]