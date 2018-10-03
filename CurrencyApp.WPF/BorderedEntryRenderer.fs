namespace CurrencyApp.WPF

open System
open Xamarin.Forms
open Xamarin.Forms.Platform.WPF
open System.ComponentModel

type BorderedEntryRenderer() =
    inherit EntryRenderer()

    member this.BorderedEntry with get() = this.Element :?> CurrencyApp.Controls.BorderedEntry

    override this.OnElementChanged(e: ElementChangedEventArgs<Entry>) =
        base.OnElementChanged(e)

        if (e.NewElement <> null) then
            ()
        else
            ()

    override this.OnElementPropertyChanged(sender: obj, e: PropertyChangedEventArgs) =
        ()

module Dummy_BorderedEntryRenderer =
    [<assembly: ExportRenderer(typeof<CurrencyApp.Controls.BorderedEntry>, typeof<BorderedEntryRenderer>)>]
    do ()

