﻿namespace EWallet.Dtos;

public sealed class OrderItem
{
    public required long OrderItemId { get; init; }
    public required int SenderWalletId { get; init; }
    public required int ReceiverWalletId { get; init; }
    public required decimal Amount { get; init; }
}
