﻿using Asp.Versioning;
using EWallet.Dtos;
using EWallet.Models.Views;
using EWallet.Service;
using Microsoft.AspNetCore.Mvc;

namespace EWallet.Server.Controllers;

[ApiController]
[ApiVersion("1")]
[Route("api/v{version:apiVersion}/apps/{appId}/wallets")]
public class WalletsController(WalletService walletService) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<ActionResult<Wallet>> CreateWallet(int appId)
    {
        return StatusCode(StatusCodes.Status201Created, await walletService.Create(appId));
    }

    [HttpGet("{walletId:int}")]
    public async Task<Wallet> GetWallet(int appId, int walletId)
    {
        return await walletService.Get(appId, walletId);
    }

    [HttpGet("transactions")]
    public async Task<OrderItemView[]> GetWalletTransactions(int appId, int walletId, int? participantWalletId = null,
        DateTime? beginTime = null, DateTime? endTime = null, int? orderTypeId = null, int? pageSize = null, int? pageNumber = null)
    {
        var orderItemViews = await walletService.GetWalletTransactions(
            appId, walletId, participantWalletId, beginTime, endTime, orderTypeId, pageSize, pageNumber);
        return orderItemViews;
    }

    [HttpPost("{walletId}/min-balance")]
    public async Task<Wallet> SetMinBalance(int appId, int walletId, SetMinBalanceRequest request)
    {
        var wallet = await walletService.SetMinBalance(appId, walletId, request);
        return wallet;
    }
}