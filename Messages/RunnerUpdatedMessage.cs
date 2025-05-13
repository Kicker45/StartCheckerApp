//------------------------------------------------------------------------------
// Název souboru: RunnerUpdatedMessage.cs
// Autor: Jan Nechanický
// Popis: Tento soubor obsahuje abstraktní základní třídu pro posílání zprávy o změně závodníka.
// Datum vytvoření: 1.4.2025
//------------------------------------------------------------------------------

using StartCheckerApp.Models;

namespace StartCheckerApp.Messages;

public record RunnerUpdatedMessage(Runner Runner);

