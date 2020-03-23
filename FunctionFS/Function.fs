namespace TaleOfPorts

open System
open System.IO
open Microsoft.AspNetCore.Mvc
open Microsoft.Azure.WebJobs
open Microsoft.Azure.WebJobs.Extensions.Http
open Microsoft.AspNetCore.Http
open Microsoft.Extensions.Logging
open Newtonsoft.Json

// Function generated from the dotnet template using
// dotnet new http --language F# --name HttpTrigger
module FSTemplate =

    // Define a nullable container to deserialize into.
    [<AllowNullLiteral>]
    type NameContainer() =
        member val Name = "" with get, set

    // For convenience, it's better to have a central place for the literal.
    [<Literal>]
    let Name = "name"

    [<FunctionName("FSTemplate")>]
    let run 
        ([<HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)>]
        req: HttpRequest) (log: ILogger) =
        async {
            log.LogInformation("F# HTTP trigger function processed a request.")

            let nameOpt = 
                if req.Query.ContainsKey(Name) then
                    Some(req.Query.[Name].[0])
                else
                    None

            use stream = new StreamReader(req.Body)
            let! reqBody = stream.ReadToEndAsync() |> Async.AwaitTask

            let data = JsonConvert.DeserializeObject<NameContainer>(reqBody)

            let name =
                match nameOpt with
                | Some n -> n
                | None ->
                    match data with
                    | null -> ""
                    | nc -> nc.Name
            
            let responseMessage =             
                if (String.IsNullOrWhiteSpace(name)) then
                    "This HTTP triggered function executed successfully. Pass a name in the query string "+
                    "or in the request body for a personalized response."
                else
                    "Hello, " +  name + ". This HTTP triggered function executed successfully."

            return OkObjectResult(responseMessage) :> IActionResult
        } |> Async.StartAsTask


module FSNaive =
    [<FunctionName("FSNaive")>]
    let run 
        ([<HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)>]
        req: HttpRequest, log: ILogger) =

        async {
            log.LogInformation "F# HTTP trigger function processed a request."
            let mutable name = req.Query.["name"].ToString()
            
            let! requestBody = (new StreamReader(req.Body)).ReadToEndAsync() |> Async.AwaitTask
            let data = JsonConvert.DeserializeObject<{|name:string|}> requestBody

            if String.IsNullOrEmpty(name) && (box data) <> null then name <- data.name

            let responseMessage = 
                if String.IsNullOrEmpty(name)
                then 
                    "This HTTP triggered function executed successfully. Pass a name in the query "+
                    "string or in the request body for a personalized response."
                else (sprintf "Hello, %s. This HTTP triggered function executed successfully." name)
            return OkObjectResult(responseMessage)
        } |> Async.StartAsTask


// Not async {} because let! must be at the top level of the computation expression
module FSIdiomatic1 =
    [<FunctionName("FSIdiomatic1")>]
    let run 
        ([<HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)>]
        req: HttpRequest, log: ILogger) =

        async {
            log.LogInformation "F# HTTP trigger function processed a request."

            let! requestBody = (new StreamReader(req.Body)).ReadToEndAsync() |> Async.AwaitTask
            let name =
                match List.ofSeq req.Query.["name"] with
                | h::_ -> Some h
                | _ ->
                    let jObj = JsonConvert.DeserializeObject<{|name:string|}> requestBody
                    match box jObj with
                    | null -> None
                    | _ -> match jObj.name with | null -> None | n -> Some n

            let res =  
                (match name with
                | None -> 
                    "This HTTP triggered function executed successfully. Pass a name in the query "+
                    "string or in the request body for a personalized response."
                | Some n -> sprintf "Hello, %s. This HTTP triggered function executed successfully." n) 
                |> OkObjectResult
            return res
        } |> Async.StartAsTask

// Not async {} because let! must be at the top level of the computation expression
module FSIdiomatic2 =
    [<FunctionName("FSIdiomatic2")>]
    let run 
        ([<HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)>]
        req: HttpRequest, log: ILogger) =
        log.LogInformation "F# HTTP trigger function processed a request."

        let queryName =
            match List.ofSeq req.Query.["name"] with
            | h::_ -> Some h
            | _ -> None

        let jsonName = 
            let requestBody = (new StreamReader(req.Body)).ReadToEndAsync().Result
            let jObj = JsonConvert.DeserializeObject<{|name:string|}> requestBody
            match box jObj with
            | null -> None
            | _ -> match jObj.name with | null -> None | n -> Some n

        (match queryName, jsonName with
        | Some n, None | _, Some n -> sprintf "Hello, %s. This HTTP triggered function executed successfully." n
        | _ -> 
            "This HTTP triggered function executed successfully. Pass a name in the query "+
            "string or in the request body for a personalized response.")
        |> OkObjectResult