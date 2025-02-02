﻿namespace EWallet.Dtos;

public sealed class SetMinBalanceRequest
{
    public required int CurrencyId { get; init; }
    public required decimal MinBalance { get; init; }
}