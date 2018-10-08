// Copyright 2018 Fabulous contributors. See LICENSE.md for license.
namespace CurrencyApp

open Fabulous.Core
open Fabulous.DynamicViews
open Xamarin.Forms
open CurrencyApp.Services

module App = 
    type Model = 
      { 
        Price: double 
        Currencies: seq<string> option
        ComputedPrice: double

        CurrencyService: CurrencyService
      }

    type Msg = 
        | UpdatePrice of string

        | CurrenciesLoaded of seq<string>
        
        | ComputeCurrency

        | ComputeDone of float

    let loadCurrencies (currencyService : CurrencyService) = async {
        let! currencies = currencyService.GetAllCurrencies()
        return CurrenciesLoaded currencies
    }

    let computeCurrencyAsync (currencyService : CurrencyService) price = async {
        let! convertionRate = currencyService.GetConversionRate "USD" "EUR"
        return ComputeDone (price * convertionRate)
    }

    let initModel currencyService =
        { 
            Price = 0.; 
            Currencies = None;
            ComputedPrice = 0.;
            CurrencyService = currencyService;
        }

    let init currencyService () =
        (initModel currencyService),
        Cmd.ofAsyncMsg (loadCurrencies currencyService)

    let update msg model =
        match msg with
        | UpdatePrice price ->
            { model with Price = double price}, Cmd.none

        | CurrenciesLoaded currencies ->
            { model with Currencies = Some currencies }, Cmd.none
        
        | ComputeCurrency ->
            model, Cmd.ofAsyncMsg (computeCurrencyAsync model.CurrencyService model.Price)

        | ComputeDone price ->
            { model with ComputedPrice = price }, Cmd.none

    let view model dispatch =
        let title = "CurrencyApp"
        
        match model.Currencies with
        | None ->
            dependsOn () (fun m () ->
                View.ContentPage(
                    title=title,
                    content=View.StackLayout(
                        children=[Style.mkCentralLabel "Chargement..." ]
                    )
                )
            )
        | Some currencies ->
            dependsOn () (fun m () ->
                View.ContentPage(
                  title = title,
                  content = View.StackLayout(padding = 20.0, verticalOptions = LayoutOptions.Center,
                    children = [ 
                        View.Grid(
                            coldefs = [ 100.; GridLength.Star ],
                            columnSpacing = 10.,
                            children = [
                                yield (Style.mkFormLabel "Saisissez un prix : ").VerticalOptions(LayoutOptions.Center)
                                yield (Style.mkFormEntry "prix" "0" Keyboard.Numeric true (UpdatePrice >> dispatch)).GridColumn(1)
                            ])
                        View.Picker(currencies, title = "Sélectionnez une devise")
                        View.Label("Vers")
                        View.Picker(currencies, title = "Sélectionnez une devise")
                        View.Button("Convertir", command = (fun () -> dispatch ComputeCurrency))
                        View.Label (text = sprintf "Calcul: %f" model.ComputedPrice, horizontalOptions = LayoutOptions.Center)
                    ]))
            )
        
            
    let program service = Program.mkProgram (service |> init) update view

type App () as app = 
    inherit Application ()
    
    let currencyService = new CurrencyService()

    let runner = 
        App.program currencyService
#if DEBUG
        |> Program.withConsoleTrace
#endif
        |> Program.runWithDynamicView app

#if DEBUG
    // Uncomment this line to enable live update in debug mode. 
    // See https://fsprojects.github.io/Fabulous/tools.html for further  instructions.
    //
    //do runner.EnableLiveUpdate()
#endif    

    // Uncomment this code to save the application state to app.Properties using Newtonsoft.Json
    // See https://fsprojects.github.io/Fabulous/models.html for further  instructions.
#if APPSAVE
    let modelId = "model"
    override __.OnSleep() = 

        let json = Newtonsoft.Json.JsonConvert.SerializeObject(runner.CurrentModel)
        Console.WriteLine("OnSleep: saving model into app.Properties, json = {0}", json)

        app.Properties.[modelId] <- json

    override __.OnResume() = 
        Console.WriteLine "OnResume: checking for model in app.Properties"
        try 
            match app.Properties.TryGetValue modelId with
            | true, (:? string as json) -> 

                Console.WriteLine("OnResume: restoring model from app.Properties, json = {0}", json)
                let model = Newtonsoft.Json.JsonConvert.DeserializeObject<App.Model>(json)

                Console.WriteLine("OnResume: restoring model from app.Properties, model = {0}", (sprintf "%0A" model))
                runner.SetCurrentModel (model, Cmd.none)

            | _ -> ()
        with ex -> 
            App.program.onError("Error while restoring model found in app.Properties", ex)

    override this.OnStart() = 
        Console.WriteLine "OnStart: using same logic as OnResume()"
        this.OnResume()
#endif


