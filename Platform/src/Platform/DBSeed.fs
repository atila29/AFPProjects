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
    let teachers = [{id=ObjectId.GenerateNewId();name="Batman"; email="batman@dtu.dk";};
                    {id=ObjectId.GenerateNewId();name="Søren Ryge"; email="sr@dtu.dk";}]
    let students = [{id=ObjectId.GenerateNewId();name="Lars-Kristian Rasmussen";studynumber="s051677"};
                    {id=ObjectId.GenerateNewId();name="Nick";studynumber="s182837";};
                    {id=ObjectId.GenerateNewId();name="Mathias Petersen";studynumber="s144874"}]
    let headOfStudy = [{id=ObjectId.GenerateNewId();name="Einstein"; department="Physics"; email="ein@dtu.dk"}]
    
    if db.GetCollection<Teacher>("teachers").EstimatedDocumentCount() = 0L
        then db.GetCollection<Teacher>("teachers").InsertMany(teachers)
    if db.GetCollection<Student>("students").EstimatedDocumentCount() = 0L
        then db.GetCollection<Student>("students").InsertMany(students)
    if db.GetCollection<HeadOfStudy>("headsOfStudy").EstimatedDocumentCount() = 0L
        then db.GetCollection<HeadOfStudy>("headsOfStudy").InsertMany(headOfStudy)



                