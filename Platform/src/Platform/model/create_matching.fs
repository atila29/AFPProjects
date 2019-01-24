module Platform.Model.Matching


// Matches groups to a project each, of highest priority respectively. 
// Assumes that requests contain at least one project. 
// Request list -> (Group * Project) list
// ('a * ('b * 'c) list) list -> ('a * 'c) list when 'b : comparison
let create_matching requests = 

    // Auxiliary function to find a project with (shared) highest priority. 
    // Fails on completely empty list. 
    // (Priority * Project) list -> Project, where Priority in practice is an int.
    // ('a * 'b) list -> 'b when 'a : comparison
    let get_first_priority project_priorities =

        // (Nested) Auxiliary function ...
        // (Priority * Project) * ((Priority * Project) list) -> Project, where Priority in practice is an int.
        // 'a * 'b -> project_priorities:('a * 'b) list -> 'b when 'a : comparison
        let rec get_next_priority highest_so_far project_priorities =
            match project_priorities with
            | p :: rest  -> let a_priority, a_project = highest_so_far
                            let b_priority, b_project = p
                            if   b_priority > a_priority
                            then get_next_priority p rest
                            else get_next_priority highest_so_far rest
            | []         -> let (_, project) = highest_so_far
                            project

        match project_priorities with
        | p :: rest  -> get_next_priority p rest
        | []         -> failwith "Empty list of priorities."

    // Auxiliary function to match groups with a project of (shared) highest priority. 
    // Request list -> (Group * Project) list
    // ('a * ('b * 'c) list) list -> ('a * 'c) list when 'b : comparison
    let rec match_next requests =
        match requests with
        | (group, project_priorities) :: rest -> 
                        let first_priority = get_first_priority project_priorities
                        (group, first_priority) :: match_next rest
        | []                                  -> []

    match_next requests