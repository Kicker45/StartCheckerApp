//------------------------------------------------------------------------------
// Název souboru: IMessageService.cs
// Autor: Jan Nechanický
// Popis: Soubor pro definici vlastní zprávy pro volání v aplikaci.
// Datum vytvoření: 1.4.2025
//------------------------------------------------------------------------------
namespace StartCheckerApp.Services;
public interface IMessageService
{
    Task ShowMessageAsync(string message);
}
