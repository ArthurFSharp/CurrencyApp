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
        Currencies: seq<string * CurrencyModel> option
        ComputedPrice: float

        SelectedFromCurrency: int
        SelectedToCurrency: int

        CurrencyService: CurrencyService
      }

    type Msg = 
        | UpdatePrice of string

        | CurrenciesLoaded of seq<string * CurrencyModel>

        | FromCurrency of int
        | ToCurrency of int
        
        | ComputeCurrency

        | ComputeDone of float

    let loadCurrencies (currencyService : CurrencyService) = async {
        let! currencies = currencyService.GetAllCurrencies()
        return CurrenciesLoaded currencies
    }

    let computeCurrencyAsync (currencyService : CurrencyService) (currencies : seq<string * CurrencyModel> option) price indexFromCurrency indexToCurrency = async {
        match currencies with
        | None ->
            return ComputeDone 0.
        | Some currencies ->
            let currencyFrom = fst (Seq.item indexFromCurrency currencies)
            let currencyTo = fst (Seq.item indexToCurrency currencies)
            let! convertionRate = currencyService.GetConversionRate currencyFrom currencyTo
            return ComputeDone (price * convertionRate)
    }

    let initModel currencyService =
        { 
            Price = 0.; 
            Currencies = None;
            ComputedPrice = 0.;
            SelectedFromCurrency = 0;
            SelectedToCurrency = 0;
            CurrencyService = currencyService;
        }

    let init currencyService () =
        (initModel currencyService),
        Cmd.ofAsyncMsg (loadCurrencies currencyService)

    let update msg model =
        match msg with
        | UpdatePrice price ->
            { model with Price = float price}, Cmd.none

        | CurrenciesLoaded currencies ->
            { model with Currencies = Some currencies }, Cmd.none

        | FromCurrency index ->
            { model with SelectedFromCurrency = index }, Cmd.none
        | ToCurrency index ->
            { model with SelectedToCurrency = index }, Cmd.none
        
        | ComputeCurrency ->
            model, Cmd.ofAsyncMsg (computeCurrencyAsync model.CurrencyService model.Currencies model.Price model.SelectedFromCurrency model.SelectedToCurrency)

        | ComputeDone price ->
            { model with ComputedPrice = price }, Cmd.none

    let view model dispatch =
        let title = "CurrencyApp"
        
        match model.Currencies with
        | None ->
            View.ContentPage(
                    title=title,
                    content=View.StackLayout(
                        children=[Style.mkCentralLabel "Chargement..." ]
                    )
                )
        | Some currencies ->
            View.ContentPage(
                  title = title,
                  content = View.StackLayout(padding = 20.0, verticalOptions = LayoutOptions.StartAndExpand,
                    children = [ 
                        View.Image(source="https://cdn2.iconfinder.com/data/icons/e-commerce-icons-2/256/Ecommerce_Icons_Rose_Color-47-128.png", heightRequest = 128., widthRequest = 128., horizontalOptions = LayoutOptions.Center)
                        View.Grid(
                            coldefs = [ 100.; GridLength.Star ],
                            columnSpacing = 10.,
                            children = [
                                yield (Style.mkFormLabel "Saisissez un prix : ")
                                yield (Style.mkFormEntry "prix" "0" Keyboard.Numeric true (UpdatePrice >> dispatch)).GridColumn(1).HeightRequest(30.)
                            ])
                        Style.mkFormPicker "Sélectionnez une devise" (Seq.map (fun c -> sprintf "%s - %s" (snd c).Id (snd c).CurrencyName) currencies) model.SelectedFromCurrency (FromCurrency >> dispatch)
                        View.Label("Vers")
                        Style.mkFormPicker "Sélectionnez une devise" (Seq.map (fun c -> sprintf "%s - %s" (snd c).Id (snd c).CurrencyName) currencies) model.SelectedToCurrency (ToCurrency >> dispatch)
                        View.Button("Convertir", command = (fun () -> dispatch ComputeCurrency))
                        View.Label (text = (sprintf "Calcul: %A %s" model.ComputedPrice (snd (Seq.item model.SelectedToCurrency currencies)).CurrencySymbol), horizontalOptions = LayoutOptions.CenterAndExpand, widthRequest = 200.)
                    ]))
        
            
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


