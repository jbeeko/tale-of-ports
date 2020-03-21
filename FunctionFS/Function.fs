namespace TaleOfPorts

open System
open System.IO
open Microsoft.AspNetCore.Mvc
open Microsoft.Azure.WebJobs
open Microsoft.Azure.WebJobs.Extensions.Http
open Microsoft.AspNetCore.Http
open Microsoft.Extensions.Logging
open Newtonsoft.Json

module FSNaivePort =
    [<FunctionName("FSNaive")>]
    let Run 
        ([<HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)>]
        req: HttpRequest, log: ILogger) =

        log.LogInformation "F# HTTP trigger function processed a request."

        let mutable name = req.Query.["name"].ToString()

        let requestBody = (new StreamReader(req.Body)).ReadToEndAsync().Result
        let data = JsonConvert.DeserializeObject<{|name:string|}> requestBody

        if String.IsNullOrEmpty(name) && (box data) <> null then name <- data.name

        let responseMessage = 
            if String.IsNullOrEmpty(name)
            then 
                "This HTTP triggered function executed successfully. Pass a name in the query "+
                "string or in the request body for a personalized response."
            else (sprintf "Hello, %s. This HTTP triggered function executed successfully." name)
        OkObjectResult(responseMessage)

module FSIdiomatic1 =
    [<FunctionName("FSIdiomatic1")>]
    let Run 
        ([<HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)>]
        req: HttpRequest, log: ILogger) =

        log.LogInformation "F# HTTP trigger function processed a request."

        let name =
            match List.ofSeq req.Query.["name"] with
            | h::_ -> Some h
            | _ ->
                let requestBody = (new StreamReader(req.Body)).ReadToEndAsync().Result
                let jObj = JsonConvert.DeserializeObject<{|name:string|}> requestBody
                match box jObj with
                | null -> None
                | _ -> match jObj.name with | null -> None | n -> Some n

        (match name with
        | None -> 
            "This HTTP triggered function executed successfully. Pass a name in the query "+
            "string or in the request body for a personalized response."
        | Some n -> sprintf "Hello, %s. This HTTP triggered function executed successfully." n) 
        |> OkObjectResult

module FSIdiomatic2 =
    [<FunctionName("FSIdiomatic2")>]
    let Run 
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
