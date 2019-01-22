module Platform.HtmlViews

open Giraffe.GiraffeViewEngine

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