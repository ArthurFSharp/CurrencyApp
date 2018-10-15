namespace CurrencyApp.Services

open FSharp.Data
open FSharp.Data.JsonExtensions
open System.Net
open Newtonsoft.Json

type CurrencyModel =
        {
            CurrencyName: string
            CurrencySymbol: string
            Id: string
        }

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
            let results = data?results
            let currencies = JsonConvert.DeserializeObject<Map<string, CurrencyModel>>(results.ToString()) |> Map.toSeq
            return currencies
        }
    
    member this.GetCurrencyAtIndex index = async {
        let! data = loadJson("currencies")
        let (code, _) = data?results.Properties.[index]
        return code
    }

    member this.GetConversionRate fromCurrency toCurrency =
        async {
            let! data = loadJson("convert?q=" + fromCurrency + "_" + toCurrency + "&compact=y")
            let _, rate = data.Properties.[0]
            return rate?``val``.AsFloat()
        }
