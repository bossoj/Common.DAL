using JetBrains.Annotations;
using System.Data.Entity;

namespace Common.DAL.EF
{
    /// <summary>
    /// Провайдер сессии EntityFramework
    /// </summary>
    public interface IDbContextProvider
    {
        ///<summary>
        /// Текущая сессия 
        ///</summary>
        [CanBeNull]
        DbContext CurrentDbContext { get; set; }    
    }
}
