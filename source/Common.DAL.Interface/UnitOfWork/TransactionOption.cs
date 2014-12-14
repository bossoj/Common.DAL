namespace Common.DAL.Interface
{
    /// <summary>
    /// Экземпляр перечисления, описывающий требования к транзакции
    /// </summary>
    public enum TransactionOption
    {
        /// <summary>
        /// Выполнять запросы в текущей транзакции.
        /// Можно управлять транзакцией из вне.
        /// </summary>
        Current,

        /// <summary>
        /// Открыть новую транзакцию для запроса
        /// </summary>
        New
    }
}
