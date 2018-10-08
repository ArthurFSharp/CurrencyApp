namespace CurrencyApp.Services

open FSharp.Data
open FSharp.Data.JsonExtensions
open System.Net

type CurrencyService() =

    let baseUrl = "https://free.currencyconverterapi.com/api/v6/"
    let wc = new WebClient()

    let loadJson(url) =
        async {
            let json = wc.DownloadString(baseUrl + url)
            return JsonValue.Parse(json)
        }

    member this.GetAllCurrencies () =
        async {
            let! data = loadJson("currencies")
            let s = data?results.Properties
                    |> Seq.map (fun (code, country) -> code + " (" + country?currencyName.ToString() + ")")
            return s
        }
    
    member this.GetCurrencyAtIndex currencies index =
        let (code, _) = currencies?results.Properties.[index]
        code

    member this.GetConversionRate fromCurrency toCurrency =
        async {
            let! data = loadJson("convert?q=" + fromCurrency + "_" + toCurrency + "&compact=y")
            let _, rate = data.Properties.[0]
            return rate?``val``.AsFloat()
        }
