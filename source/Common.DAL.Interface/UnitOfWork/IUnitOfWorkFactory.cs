﻿using System.Data;

namespace Common.DAL.Interface
{
    /// <summary>
    /// Интерфейс фабрики UnitOfWork
    /// </summary>
    public interface IUnitOfWorkFactory
    {
        /// <summary>
        /// Создает UnitOfWork с поддержкой состояний, если у UnitOfWork не будет вызван метод <see cref="IUnitOfWork.Commit" />, то автоматически будет выполнен rollback
        /// </summary>
        /// <returns>UnitOfWork</returns>
        IUnitOfWork Create();

        /// <summary>
        /// Создает UnitOfWork с поддержкой состояний, если у UnitOfWork не будет вызван метод <see cref="IUnitOfWork.Commit" />, то автоматически будет выполнен rollback
        /// </summary>
        /// <param name="isolationLevel">Уровень изоляции (задает поведение при блокировке транзакции для подключения)</param>
        /// <param name="transactionOption">Требование к транзакции</param>
        /// <returns>UnitOfWork</returns>
        IUnitOfWork Create(IsolationLevel isolationLevel, TransactionOption transactionOption = TransactionOption.New);

        /// <summary>
        /// Создает UnitOfWork с поддержкой состояний, если у UnitOfWork не будет вызван метод <see cref="IUnitOfWork.Commit" />, то автоматически будет выполнен rollback
        /// </summary>
        /// <param name="transactionOption">Требование к транзакции</param>
        /// <returns>UnitOfWork</returns>
        IUnitOfWork Create(TransactionOption transactionOption);

        /// <summary>
        /// Создает UnitOfWork с поддержкой состояний, если у UnitOfWork не будет вызван метод <see cref="IUnitOfWork.Commit" />, то автоматически будет выполнен rollback
        /// </summary>
        /// <returns>UnitOfWork</returns>
        IUnitOfWorkAsync CreateAsync();

        /// <summary>
        /// Создает UnitOfWork с поддержкой состояний, если у UnitOfWork не будет вызван метод <see cref="IUnitOfWork.Commit" />, то автоматически будет выполнен rollback
        /// </summary>
        /// <param name="isolationLevel">Уровень изоляции (задает поведение при блокировке транзакции для подключения)</param>
        /// <param name="transactionOption">Требование к транзакции</param>
        /// <returns>UnitOfWork</returns>
        IUnitOfWorkAsync CreateAsync(IsolationLevel isolationLevel, TransactionOption transactionOption = TransactionOption.New);

        /// <summary>
        /// Создает UnitOfWork с поддержкой состояний, если у UnitOfWork не будет вызван метод <see cref="IUnitOfWork.Commit" />, то автоматически будет выполнен rollback
        /// </summary>
        /// <param name="transactionOption">Требование к транзакции</param>
        /// <returns>UnitOfWork</returns>
        IUnitOfWorkAsync CreateAsync(TransactionOption transactionOption);
    }
}
