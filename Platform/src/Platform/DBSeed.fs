module Platform.DBSeed

open Platform.Model.Data

open MongoDB.Bson
open MongoDB.Driver
open MongoDB.FSharp
open MongoDB.Bson.Serialization.IdGenerators
open MongoDB.Bson
open MongoDB.Bson

[<Literal>]
let DbName = "platformdb"
let client = MongoClient()
let db = client.GetDatabase(DbName)

let seedDB =
    let teachers = [{name="Batman"; email="batman@dtu.dk";}; {name="Søren Ryge"; email="sr@dtu.dk";}]
    let students = [{name="Lars-Kristian Rasmussen";studynumber="s051677"};
                    {name="Nick";studynumber="s182837";};
                    {name="Mathias Petersen";studynumber="s144874"}]
    let headOfStudy = [{name="Einstein"; department="Physics"; email="ein@dtu.dk"}]

    db.GetCollection<Teacher>("teachers").InsertMany(teachers)



                