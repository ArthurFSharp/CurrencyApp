namespace CurrencyApp

open Xamarin.Forms
open Fabulous.DynamicViews
open CurrencyApp.Controls

module Style =
    let mkCentralLabel text =
        View.Label(text=text, horizontalOptions=LayoutOptions.Center, verticalOptions=LayoutOptions.CenterAndExpand)

    let mkFormLabel text =
        View.Label(text=text, margin=new Thickness(0., 20., 0., 5.))

    let mkFormEntry placeholder text keyboard isValid textChanged =
        View.BorderedEntry(placeholder=placeholder, text=text, keyboard=keyboard, textChanged=(fun e -> e.NewTextValue |> textChanged),
                           borderColor=(match isValid with true -> Color.Default | false -> Color.Red))

