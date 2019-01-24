module Platform.HtmlViews

open Giraffe.GiraffeViewEngine
open Platform.Model.Input
open Platform.Model.Data

// ---------------------------------
// Views
// ---------------------------------

let studentTableTemplate (students: Student list) = div[] [
  table [_class "table"] [
    thead [] [
      tr [] [
        th [ _scope "col"] [encodedText "studynr."]
        th [ _scope "col"] [encodedText "name"]
      ]
    ]
    tbody [] [
      yield!
        students
        |> List.map (fun req -> tr [] [
          td [] [ encodedText (string req.studynumber) ]
          td [] [ encodedText (string req.name) ]
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

let groupTable (groups: Group list) = div[] [
  table [_class "table"] [
    thead [] [
      tr [] [
        th [ _scope "col"] [encodedText "id"]
        th [ _scope "col"] [encodedText "students"]
      ]
    ]
    tbody [] [
      yield!
        groups
        |> List.map (fun grp -> match grp with
                                | {number=n; students=ss} -> tr [] [
                                                               td [] [ encodedText (string n) ]
                                                               td [] [ encodedText (String.concat ", " (ss |> Seq.map (fun s -> string s))) ]
                                                             ])
    ]
  ]
]

let teacherTableTemplate (teachers: Teacher list) = div [] [
  table [_class "table"] [
    thead [] [
      tr [] [
        th [ _scope "col"] [encodedText "name"]
        th [ _scope "col"] [encodedText "email"]
      ]
    ]
    tbody [] [
      yield!
        teachers
        |> List.map (fun t -> match t with
                                | {email=mail; name=name} -> tr [] [
                                                               td [] [ encodedText name ]
                                                               td [] [ encodedText mail]
                                                             ])
    ]
  ]
]

let teacherTemplate (students: Student list) (groups: Group list) (teachers: Teacher list) = div[] [
  h2 [] [encodedText "teachers"]
  teacherTableTemplate teachers
  h2 [] [encodedText "Project proposals"]
  form [_action "/api/project/submit"; _method "post"] [
                p [] [ encodedText "Title" ]
                input [_type "text"; _name "title"] //type="text" name="lastname"
                p [] [ encodedText "Description" ]
                input [_type "text"; _name "description"]
                p [] [ encodedText "Teacher email" ]
                input [_type "text"; _name "teacherEmail"]
                p [] [ encodedText "Prerequisites (comma-separated)" ]
                input [_type "text"; _name "prerequisitesCS"]
                p [] [ encodedText "Cosupervisor emails (comma-separated)" ]
                input [_type "text"; _name "cosupervisorsEmailCS"]
                // p [] [ encodedText "Restrictions (command-separated)" ] // TODO: her skal custom restrictions være
                // input [_type "text"; _name "restrictionsCS"]
                br []
                input [_type "submit"; _value "Request"]
  ]
  div [] [
      h1 [] [encodedText "students"]
      studentTableTemplate students
  ]
  div [] [
      h1 [] [encodedText "groups"]
      groupTable groups
      div [] [
        h2 [] [encodedText "Create group"]
        form [_action "/api/group/create"; _method "post"] [
                p [] [ encodedText "number" ]
                input [_type "number"; _name "number"]
                input [_type "submit"; _value "create group"]
        ]
        h2 [] [encodedText "Add member to group"]
        form [_action "/api/group/add"; _method "post"] [
                p [] [ encodedText "group number" ]
                input [_type "number"; _name "groupNumber"] 
                p [] [ encodedText "studentId" ]
                input [_type "text"; _name "studynumber"] 
                br []
                input [_type "submit"; _value "add member"]
        ]
      ]
  ]
  div [] [
    h2 [] [encodedText "Asignment of projects"]
    form [_action "/api/project/assign"; _method "post"] [
                p [] [ encodedText "group number" ]
                input [_type "number"; _name "groupNumber"] 
                p [] [ encodedText "project id" ]
                input [_type "text"; _name "projectId"]
                br []
                input [_type "submit"; _value "Assign"]
    ]
  ]
]

let errorTemplate (error: string) = div[] [
    p [_class "errortext"] [encodedText error]
]

let projectTableTemplate (requests: Project list) = div[] [
  table [_class "table"] [
    thead [] [
      tr [] [
        th [ _scope "col"] [encodedText "id"]
        th [ _scope "col"] [encodedText "title"]
        th [ _scope "col"] [encodedText "description"]
        th [ _scope "col"] [encodedText "teacher"]
        th [ _scope "col"] [encodedText "courseno"]
        th [ _scope "col"] [encodedText "status"]
        // th [ _scope "col"] [encodedText "restrictions"]
        th [ _scope "col"] [encodedText "prerequisites"]
        th [ _scope "col"] [encodedText "cosupervisors"]
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
          td [] [ encodedText (string req.courseno) ]
          td [] [ encodedText (string (match req.status with
                                       | ProjectStatus.Request -> "requested"
                                       | ProjectStatus.Accepted -> "accepted" 
                                       | ProjectStatus.Declined -> "declined"
                                       | ProjectStatus.Published -> "published"
                                       | _ -> ""
                                       ))]
          // td [] [ encodedText (req.restrictions |> List.map (fun r -> string r.name) |> String.concat "<br/>") ]
          td [] [ encodedText (req.prerequisites |> String.concat "<br/>") ]
          td [] [ encodedText (req.cosupervisors |> List.ofSeq |> List.map (fun c -> c.name) |> String.concat "<br/>") ]
        ])
    ]
  ]
]

let studentsTable (students: Student list) = div[] [
  table [_class "table"] [
    thead [] [
      tr [] [
        th [ _scope "col"] [encodedText "studynr."]
        th [ _scope "col"] [encodedText "name"]
      ]
    ]
    tbody [] [
      yield!
        students
        |> List.map (fun s -> tr [] [
          td [] [ encodedText s.studynumber ]
          td [] [ encodedText s.name ]
        ])
    ]
  ]
]

let headOfStudyView (requests: Project list)(students: Student list) = div [] [
    h2 [] [encodedText "projects"]
    projectTableTemplate requests
    h2 [] [encodedText "students"]
    studentsTable students
    form [_action "/api/student"; _method "post"] [
                p [] [ encodedText "name" ]
                input [_type "text"; _name "name"] 
                p [] [ encodedText "studynumber" ]
                input [_type "text"; _name "studynumber"] 
                br []
                input [_type "submit"; _value "add Student"]
    ]

    h2 [] [encodedText "accept project"]
    form [_action "/api/project/accept"; _method "post"] [
                p [] [ encodedText "id" ]
                input [_type "text"; _name "id"]
                br []
                p [] [ encodedText "course#" ]
                input [_type "text"; _name "courseno"]
                input [_type "submit"; _value "Accept"]
    ]

    h2 [] [encodedText "decline project"]
    form [_action "/api/project/decline"; _method "post"] [
                p [] [ encodedText "id" ]
                input [_type "text"; _name "id"] 
                input [_type "submit"; _value "Decline"]
    ]

    h2 [] [encodedText "Publish Project"]
    form [_action "/api/project/publish"; _method "post"] [
                p [] [encodedText "id"]
                input [_type "text"; _name "id"]
                input [_type "submit"; _value "Publish"]
    ]
]


let inspectPublishedProjectsView (publishedProjects: Project list) = div[] [
    div [] [
      h1 [] [encodedText "Bachelor Projects"]
      projectTableTemplate publishedProjects
  ]
]



let studentViewTemplate (publishedProjects: Project list) (groups: Group list) = div [] [
    inspectPublishedProjectsView publishedProjects
    div [] [
      yield!
        groups |> List.map (fun g -> div [] [
          h4 [] [encodedText (string g.number)]
        ])
    ]
    h2 [] [encodedText "create priority"]
    form [_action "/api/group/priority"; _method "post"] [
    p [] [ encodedText "groupNr" ]
    input [_type "text"; _name "groupNo"]
    p [] [ encodedText "project id's ordered, and seperated by comma" ]
    input [_type "text"; _name "projectIds"]
    br []
    input [_type "submit"; _value "Accept"]
    ]
    
]