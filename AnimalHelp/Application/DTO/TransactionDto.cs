﻿using System;
using AnimalHelp.Domain.Model.Donations;

namespace AnimalHelp.Application.DTO;

public class TransactionDto(DateTime dateTime, float amount, string note, bool isPayment, string counterparty)
{
    public DateTime DateTime { get; } = dateTime;
    public float Amount { get; } = amount;
    public string Note { get; } = note;
    public bool IsPayment { get; } = isPayment;
    public string Counterparty { get; } = counterparty;

    public static TransactionDto FromTransaction(Transaction transaction, bool isPayment)
    {
        return new TransactionDto(
            transaction.DateTime,
            transaction.Amount,
            transaction.Note,
            isPayment,
            isPayment ? transaction.Receiver.Owner : (transaction.IsAnonymous ? "Anonymous" : transaction.Sender.Owner)
        );
    }
}