namespace HearthstoneTextGame

open IntelliFactory.WebSharper

[<Require(typeof<JQuery.Resources.JQuery>)>]
[<Sealed>]
type TwitterBootstrap() =
    inherit Resources.BaseResource("//maxcdn.bootstrapcdn.com/bootstrap/3.2.0/",
        "js/bootstrap.min.js", "css/bootstrap.min.css", "css/bootstrap-theme.min.css")

[<Require(typeof<TwitterBootstrap>)>]
[<Sealed>]
type MyJs() =
    inherit Resources.BaseResource("/", "js/main.js", "js/pnotify.custom.min.js", "css/pnotify.custom.min.css")


[<assembly: Require(typeof<MyJs>)>]
do ()