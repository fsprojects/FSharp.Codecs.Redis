module Tests.Tests
open Fuchu
open FSharp.Codecs.Redis
open FSharp.Codecs.Redis.Operators
open FSharpPlus
open StackExchange.Redis
type Person = {
    Name: string
    Age: int
}

type Person with
    static member Create name age = { Person.Name = name; Age = age }

    static member OfRedis (o : HashEntry list) = 
        Person.Create <!> (o .@ "name") <*> (o .@ "age")
        
    static member OfRedis (o : NameValueEntry list) = 
        Person.Create <!> (o .@ "name") <*> (o .@ "age")
        

    static member ToHashEntry (x: Person) : HashEntry list =
        [ 
            "name" .= x.Name
            "age" .= x.Age
        ]
    static member ToNameValueEntry (x: Person) : NameValueEntry list =
        [ 
            "name" .= x.Name
            "age" .= x.Age
        ]

type Item = {
    Id: int
    Brand: string
    Availability: string option
}

type Item with
    static member RedisObjCodecHashEntry =
        fun id brand availability -> { Item.Id = id; Brand = brand; Availability = availability }
        <!> rreq  "id"          (fun x -> Some x.Id     )
        <*> rreq  "brand"       (fun x -> Some x.Brand  )
        <*> ropt "availability" (fun x -> x.Availability)
        |> Codec.ofConcrete
    static member RedisObjCodecNameValueEntry =
        fun id brand availability -> { Item.Id = id; Brand = brand; Availability = availability }
        <!> rreq  "id"          (fun x -> Some x.Id     )
        <*> rreq  "brand"       (fun x -> Some x.Brand  )
        <*> ropt "availability" (fun x -> x.Availability)
        |> Codec.ofConcrete

open FsCheck
open FsCheck.GenOperators
let personHashEntries = [ HashEntry(implicit "name", implicit "John"); HashEntry(implicit "age", implicit 44) ]
let personNameValueEntries = [ NameValueEntry(implicit "name", implicit "John"); NameValueEntry(implicit "age", implicit 44) ]
let person = { Person.Name = "John"; Age = 44 }
let itemHashEntries = [HashEntry(implicit "id", implicit 11); HashEntry(implicit "brand", implicit "Spinal trap")]
let itemNameValueEntries = [NameValueEntry(implicit "id", implicit 11); NameValueEntry(implicit "brand", implicit "Spinal trap")]
let item = {Id=11; Brand="Spinal trap"; Availability= None}
let tests = [
        testList "From Redis" [

            test "Person HashEntry" {
                let actual : Person ParseResult =Person.OfRedis personHashEntries
                Assert.Equal("Person", Ok person, actual)
            }
            test "Person NameValueEntry" {
                let actual : Person ParseResult =Person.OfRedis personNameValueEntries
                Assert.Equal("Person", Ok person, actual)
            }
            test "Item HashEntry" {
                let actual : Item ParseResult = Codec.decode Item.RedisObjCodecHashEntry itemHashEntries
                Assert.Equal("Item", Ok item, actual)
            }
            test "Item NameValueEntry" {
                let actual : Item ParseResult = Codec.decode Item.RedisObjCodecNameValueEntry itemNameValueEntries
                Assert.Equal("Item", Ok item, actual)
            }
        ]
        testList "To Redis" [
            test "Person HashEntry" {
                let actual = Person.ToHashEntry person
                Assert.Equal("Person", personHashEntries, actual)
            }
            test "Person NameValueEntry" {
                let actual = Person.ToNameValueEntry person
                Assert.Equal("Person", personNameValueEntries, actual)
            }
            test "Item HashEntry" {
                let actual = Codec.encode Item.RedisObjCodecHashEntry item
                Assert.Equal("Item", itemHashEntries, actual)
            }
            test "Item NameValueEntry" {
                let actual = Codec.encode Item.RedisObjCodecNameValueEntry item
                Assert.Equal("Item", itemNameValueEntries, actual)
            }
        ]
    ]

[<EntryPoint>]
let main _ = 
    printfn "Running tests..."
    runParallel (TestList (tests))
