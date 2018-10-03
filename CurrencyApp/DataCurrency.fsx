#r @"C:\Users\alemeur\.nuget\packages\fsharp.data\2.4.6\lib\net45\FSharp.Data.dll";;

open FSharp.Data
open FSharp.Data.JsonExtensions;;

let data = Http.RequestString("https://free.currencyconverterapi.com/api/v6/currencies")
let currencies = JsonValue.Parse(data);;

let loadJson(url) =
    async {
        let! json = Http.AsyncRequestString("https://free.currencyconverterapi.com/api/v6/currencies")
        return JsonValue.Parse(json)
    }

let GetAllCurrencies =
    async {
        let! data = loadJson("currencies")
        let s = data?results.Properties
                |> Seq.map (fun (code, country) -> code + " (" + country?currencyName.ToString() + ")")
        return s
    }

let getAllCurrencies =
    currencies?results.Properties
    |> Seq.map (fun (code, country) -> code + " (" + country?currencyName.ToString() + ")")

let getCurrencyKey index =
    let (code, name) = currencies?results.Properties.[index]
    code

// https://free.currencyconverterapi.com/api/v6/convert?q=USD_PHP&compact=y
let convertCurrency fromCurrency toCurrency price =
    let request = Http.RequestString("https://free.currencyconverterapi.com/api/v6/convert?q=" + fromCurrency + "_" + toCurrency + "&compact=y")
    let result = JsonValue.Parse(request)
    let _, currency = result.Properties.[0]
    currency?``val``.AsFloat()
   

