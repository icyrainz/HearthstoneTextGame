namespace HearthstoneTextGame

open IntelliFactory.Html
open IntelliFactory.WebSharper
open IntelliFactory.WebSharper.Sitelets

type Action =
    | Home
    | About

module Controls =

    [<Sealed>]
    type EntryPoint() =
        inherit Web.Control()

        [<JavaScript>]
        override __.Body =
            Client.Main() :> _

    [<Sealed>]
    type About() =
        inherit Web.Control()

        [<JavaScript>]
        override __.Body =
            Client.About() :> _


module Skin =
    open System.Web

    type Page =
        {
            Menu : list<Content.HtmlElement>
            Body : list<Content.HtmlElement>
        }

    let MainTemplate =
        Content.Template<Page>("~/index.html")
            .With("menu", fun x -> x.Menu)
            .With("body", fun x -> x.Body)


    let WithTemplate menu body : Content<Action> =
        Content.WithTemplate MainTemplate <| fun context ->
            {
                Menu = menu context
                Body = body context
            }

module Site =

    let ( => ) text url =
        A [HRef url] -< [Text text]

    let Menu (ctx: Context<Action>) =
        [
            LI ["Home" => ctx.Link Home]
            LI ["About" => ctx.Link About]
        ]

    let HomePage =
        Skin.WithTemplate Menu <| fun ctx ->
            [
                Div [new Controls.EntryPoint()]
            ]

    let AboutPage =
        Skin.WithTemplate Menu <| fun ctx ->
            [
                Div [new Controls.About()]
            ]

    let Main =
        Sitelet.Sum [
            Sitelet.Content "/" Home HomePage
            Sitelet.Content "/About" About AboutPage
            Sitelet.Infer (function
                | Home -> HomePage
                | About -> AboutPage)
        ]

[<Sealed>]
type Website() =
    interface IWebsite<Action> with
        member this.Sitelet = Site.Main
        member this.Actions = [Home; About]

type Global() =
    inherit System.Web.HttpApplication()

    member g.Application_Start(sender: obj, args: System.EventArgs) =
        ()

[<assembly: Website(typeof<Website>)>]
do ()
