[<AutoOpen>]
module Fun.DevTools.HtmlConvert.Utils

open System
open System.Text
open Fun.Result
open HtmlAgilityPack

type StringBuilder with

    member sb.AppendStringValue(value: string) =
        match value with
        | NullOrEmptyString -> sb.Append("")
        | SafeStringLower "true" -> sb.Append("true")
        | SafeStringLower "false" -> sb.Append("false")
        | INT32 x -> sb.Append(x)
        | _ -> sb.Append("\"").Append(value.Trim()).Append("\"")


let convert (html: string) =
    let doc = HtmlDocument()
    doc.OptionDefaultUseOriginalName <- true
    doc.LoadHtml(html)

    let sb = StringBuilder()

    let rec processNode deep directChild (node: HtmlNode) =
        let indent = if deep > 0 then String.Concat([| for _ in 1..deep -> "    " |]) else ""

        if node.NodeType = HtmlNodeType.Text then
            if node.InnerHtml.Trim() |> String.IsNullOrEmpty |> not then
                sb.Append(indent).AppendStringValue(node.InnerHtml.Trim()).AppendLine() |> ignore

        else if node.NodeType = HtmlNodeType.Comment then
            sb.Append(indent).Append("// ").Append(node.InnerHtml.Trim()).AppendLine() |> ignore

        else
            let name = node.Name

            if String.IsNullOrEmpty(name) |> not then
                let tagName = if Char.IsUpper name[0] then name + "''" else name

                let appendAttrs () =
                    for attr in node.Attributes do
                        let attrName =
                            match attr.Name with
                            | "for"
                            | "type"
                            | "class"
                            | "async"
                            | "open"
                            | "span"
                            | "title"
                            | "style" -> attr.Name + "'"
                            | _ -> attr.Name

                        let attrValue = attr.Value

                        if
                            Char.IsUpper attrName[0]
                            && not (String.IsNullOrEmpty attrValue)
                            && not (attrValue.Contains " ")
                            && (attrValue.Contains "." || attrValue.StartsWith "@")
                        then
                            let attrValue = if attrValue.StartsWith "@" then attrValue.Substring(1) else attrValue
                            sb.Append(indent).Append("    ").Append(attrName).Append(" ").Append(attrValue).AppendLine() |> ignore
                        else
                            sb.Append(indent).Append("    ").Append(attrName).Append(" ").AppendStringValue(attrValue).AppendLine()
                            |> ignore

                if (name = "script" || name = "style") && String.IsNullOrEmpty node.InnerText |> not then
                    sb.Append(indent).Append(tagName).Append(" {").AppendLine() |> ignore
                    appendAttrs ()
                    sb.Append(indent).Append("    html.raw \"\"\"").AppendLine() |> ignore
                    sb.Append(indent).Append("        ").Append(node.InnerText.Trim()).AppendLine() |> ignore
                    sb.Append(indent).Append("    \"\"\"").AppendLine() |> ignore
                    sb.Append(indent).AppendLine("}") |> ignore

                else if name = "svg" then
                    sb.Append(indent).Append("Static.html \"\"\"").AppendLine() |> ignore
                    sb.Append(indent).Append("    ").Append(node.WriteTo()).AppendLine() |> ignore
                    sb.Append(indent).Append("\"\"\"").AppendLine() |> ignore

                else
                    sb.Append(indent).Append(tagName).Append(" {").AppendLine() |> ignore

                    appendAttrs ()

                    for ele in node.ChildNodes do
                        processNode (deep + 1) false ele

                    sb.Append(indent).AppendLine("}") |> ignore

            else if directChild then
                sb.Append(indent).AppendStringValue(node.InnerHtml).AppendLine() |> ignore
            else
                sb.Append(indent).Append("html.text ").AppendStringValue(node.InnerHtml).AppendLine() |> ignore

    for ele in doc.DocumentNode.ChildNodes do
        processNode 0 false ele

    sb.ToString()
