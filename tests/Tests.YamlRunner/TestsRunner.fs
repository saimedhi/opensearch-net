// SPDX-License-Identifier: Apache-2.0
//
// The OpenSearch Contributors require contributions made to
// this file be licensed under the Apache-2.0 license or a
// compatible open source license.
//
// Modifications Copyright OpenSearch Contributors. See
// GitHub history for details.
//
//  Licensed to Elasticsearch B.V. under one or more contributor
//  license agreements. See the NOTICE file distributed with
//  this work for additional information regarding copyright
//  ownership. Elasticsearch B.V. licenses this file to you under
//  the Apache License, Version 2.0 (the "License"); you may
//  not use this file except in compliance with the License.
//  You may obtain a copy of the License at
//
// 	http://www.apache.org/licenses/LICENSE-2.0
//
//  Unless required by applicable law or agreed to in writing,
//  software distributed under the License is distributed on an
//  "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
//  KIND, either express or implied.  See the License for the
//  specific language governing permissions and limitations
//  under the License.
//

namespace Tests.YamlRunner

open System.Diagnostics
open ShellProgressBar
open Tests.YamlRunner.Models
open Tests.YamlRunner.TestsReader
open Tests.YamlRunner.OperationExecutor
open Tests.YamlRunner.Stashes
open OpenSearch.Net
open Skips

type TestRunner(client:IOpenSearchLowLevelClient, version: string, suite: string, progress:IProgressBar, barOptions:ProgressBarOptions) =
    
    member this.OperationExecutor = OperationExecutor(client)
    
    member private this.RunOperation file section operation nth stashes (subProgressBar:IProgressBar) = async {
        let executionContext = {
            Version = version
            Suite = suite
            File= file
            Folder= file.Directory
            Section= section
            NthOperation= nth
            Operation= operation
            Stashes = stashes
            Elapsed = ref 0L
        }
        let sw = Stopwatch.StartNew()
        try
            printfn "--------------------------------------"
            printfn "Executing operation: %s" (operation.Log())
            printfn "Folder, File, Section, NthOperation, Operation, Stashes, Elapsed : %s %s %s %i %s %A %i " executionContext.Folder.Name executionContext.File.Name executionContext.Section executionContext.NthOperation executionContext.Operation.Name executionContext.Stashes executionContext.Elapsed.Value
            let! pass = this.OperationExecutor.Execute executionContext subProgressBar
            executionContext.Elapsed := sw.ElapsedMilliseconds
            match pass with
            | Failed f ->
                let c = pass.Context
                printfn "Operation failed: %s %s %s: %s %s" pass.Name c.Folder.Name c.File.Name (operation.Log()) (f.Log())
                subProgressBar.WriteLine <| sprintf "%s %s %s: %s %s" pass.Name c.Folder.Name c.File.Name (operation.Log()) (f.Log())
            | _ -> ignore()
            return pass
        with
        | e ->
            printfn "Exception occurred while executing operation: %s" (operation.Log())
            subProgressBar.WriteLine <| sprintf "E! File: %s/%s Op: (%i) %s Section: %s " file.Directory.Name file.Name nth (operation.Log()) section 
            return Failed <| SeenException (executionContext, e)
    }
    
    member private this.CreateOperations m file (ops:Operations) subProgressBar = 
        let executedOperations =
            let stashes = Stashes()
            ops
            |> List.indexed
            |> List.map (fun (i, op) -> async {
                let! pass = this.RunOperation file m op i stashes subProgressBar
                return pass
            })
        (m, executedOperations)
        
    member private this.RunTestFile subProgressbar (file:YamlTestDocument) sectionFilter = async {
        let m section ops = this.CreateOperations section file.FileInfo ops subProgressbar
        let bootstrap section operations =
            let ops = operations |> Option.map (m section) |> Option.toList |> List.collect (fun (_, ops) -> ops)
            ops
        
        let setup =  bootstrap "Setup" file.Setup 
        let teardown = bootstrap "TEARDOWN" file.Teardown 
        let sections =
            file.Tests
            |> List.map (fun s -> s.Operations |> m s.Name)
            |> List.filter(fun s ->
                let (name, _) = s
                match sectionFilter with | Some s when s <> name -> false | _ -> true
            )
            |> List.collect (fun s ->
                let (name, ops) = s
                [(name, setup @ ops)]
            )
        
        let l = sections.Length
        let ops = sections |> List.sumBy (fun (_, i) -> i.Length)
        subProgressbar.MaxTicks <- ops
        
        let runSection progressHeader sectionHeader (ops: Async<ExecutionResult> list) = async {
            let l = ops |> List.length
            let result =
                ops
                |> List.indexed
                |> Seq.unfold (fun ms ->
                    match ms with
                    | (i, op) :: tl ->
                        let operations = sprintf "%s [%i/%i] operations: %s" progressHeader (i+1) l sectionHeader
                        subProgressbar.Tick(operations)
                        let r = Async.RunSynchronously op
                        match r with
                        | Succeeded _context -> Some (r, tl)
                        | NotSkipped _context -> Some (r, tl)
                        | Skipped (_context, _reason) ->
                            subProgressbar.WriteLine <| sprintf "%s: %s " r.Name (r.Context.Operation.Log())
                            Some (r, [])
                        | Failed _context -> Some (r, [])
                    | [] -> None
                )
                |> List.ofSeq
            return sectionHeader, result
        }
        
        let runAllSections =
            sections
            |> Seq.indexed
            |> Seq.collect (fun (i, suite) ->
                let runTests () =
                    let run section =
                        let progressHeader = sprintf "[%i/%i] sections" (i+1) l
                        let (sectionHeader, ops) = section
                        runSection progressHeader sectionHeader ops;
                    [
                        // setup run as part of the suite, unfold will stop if setup fails or skips
                        run suite;
                        //always run teardown
                        run ("TEARDOWN", teardown)
                    ]
                let file =
                    let fi = file.FileInfo
                    let di = file.FileInfo.Directory
                    sprintf "%s/%s" di.Name fi.Name
                match Skips.SkipList.TryGetValue <| SkipFile(file) with
                | (true, s) ->
                    let (sectionHeader, _) = suite
                    match s with
                    | All -> []
                    | Section s when s = sectionHeader -> []
                    | Sections s when s |> List.exists (fun s -> s = sectionHeader) -> []
                    | _ -> runTests()
                | (false, _) -> runTests()
            )
            |> Seq.map Async.RunSynchronously
        
        return runAllSections |> Seq.toList
        
    }

    member this.RunTestsInFolder mainMessage (folder:YamlTestFolder) sectionFilter = async {
        let l = folder.Files.Length
        let run (i, document) = async {
            let file = sprintf "%s/%s" document.FileInfo.Directory.Name document.FileInfo.Name
            let message = sprintf "%s [%i/%i] Files : %s" mainMessage (i+1) l file
            progress.Tick(message)
            let message = sprintf "Inspecting file for sections" 
            use p = progress.Spawn(0, message, barOptions)
            
            let! result = this.RunTestFile p document sectionFilter
            
            return document, result
        }
            
        let actions =
            folder.Files
            |> Seq.indexed 
            |> Seq.map run 
            |> Seq.map Async.RunSynchronously
        return actions
    }

