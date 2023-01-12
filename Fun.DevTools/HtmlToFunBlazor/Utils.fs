[<AutoOpen>]
module Fun.DevTools.HtmlToFunBlazor.Utils

open System
open System.IO
open System.Text
open FSharp.Data
open Fun.Result


type StringBuilder with

    member sb.AppendStringValue(value: string) =
        match value with
        | INT32 x -> sb.Append(x)
        | _ -> sb.Append("\"").Append(value.Trim()).Append("\"")


let convert (html: string) =
    use reader = new StringReader(html)
    let doc = HtmlDocument.Load reader

    let sb = StringBuilder()

    let rec processNode deep directChild (node: HtmlNode) =
        let indent = 
            if deep > 0 then String.Concat([|for _ in 0..deep -> "    "|])
            else ""

        let name = node.Name()

        if String.IsNullOrEmpty(name) |> not then
            let tagName =
                if Char.IsUpper name[0] then name + "'()"
                else name

            sb.Append(indent).Append(tagName).Append(" {").AppendLine() |> ignore
            
            for attr in node.Attributes() do
                let attrName =
                    match attr.Name() with
                    | "class" -> "class'"
                    | "style" -> "style'"
                    | _ -> attr.Name()

                let attrValue = attr.Value()

                if Char.IsUpper attrName[0] && not(String.IsNullOrEmpty attrValue) && not(attrValue.Contains " ") && (attrValue.Contains "." || attrValue.StartsWith "@") then
                    let attrValue = if attrValue.StartsWith "@" then attrValue.Substring(1) else attrValue
                    sb.Append(indent).Append("    ").Append(attrName).Append(" ").Append(attrValue).AppendLine() |> ignore
                else
                    sb.Append(indent).Append("    ").Append(attrName).Append(" ").AppendStringValue(attrValue).AppendLine() |> ignore
            
            let children = node.Elements()

            if children.Length = 1 then
                processNode (deep + 1) true children[0]

            if children.Length > 1 then
                sb.Append(indent).Append("    ").Append("childContent [").AppendLine() |> ignore
                
                for ele in children do
                    processNode (if deep = 0 then deep + 1 else deep + 2) false ele

                sb.Append(indent).Append("    ").Append("]").AppendLine() |> ignore
                
            sb.Append(indent).AppendLine("}") |> ignore

        else
            if directChild then
                sb.Append(indent).AppendStringValue(node.InnerText()).AppendLine() |> ignore
            else
                sb.Append(indent).Append("html.text ").AppendStringValue(node.InnerText()).AppendLine() |> ignore

    for ele in doc.Elements() do
        processNode 0 false ele

    sb.ToString()
