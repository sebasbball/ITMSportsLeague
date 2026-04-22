using SportsLeague.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace SportsLeague.Domain.Interfaces.Services
{
    public interface IMatchEventService
    {
        #region Match Results Methods
        Task<MatchResult> RegisterResultAsync(int matchId, MatchResult result);
        Task<MatchResult?> GetResultByMatchAsync(int matchId);
        #endregion

        #region Goal Methods
        Task<Goal> RegisterGoalAsync(int matchId, Goal goal);
        Task<IEnumerable<Goal>> GetGoalsByMatchAsync(int matchId);
        Task DeleteGoalAsync(int goalId);
        #endregion

        #region Cards Methods
        Task<Card> RegisterCardAsync(int matchId, Card card);
        Task<IEnumerable<Card>> GetCardsByMatchAsync(int matchId);
        Task DeleteCardAsync(int cardId);
        #endregion

        // Con esta unificación de servicios
        // se puede manejar toda la lógica relacionada con los eventos de un partido
        // (resultados, goles y tarjetas) desde un solo punto,
        // lo que facilita el mantenimiento y la coherencia de la aplicación.
        // Además, me evita duplicar código en servicios separados para cada tipo de evento,
        // ya que muchos de los procesos de validación y manejo de errores pueden ser compartidos.
    }
}
