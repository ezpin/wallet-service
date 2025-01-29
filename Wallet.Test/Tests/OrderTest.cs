using EWallet.Api;
using EWallet.Exceptions;
using EWallet.Test.Helper;
using GrayMint.Common.Client;
using GrayMint.Common.Exceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EWallet.Test.Tests;

[TestClass]
public class OrderTest : BaseControllerTest
{
    [TestMethod]
    public async Task Fail_create_an_order_with_invalid_currency()
    {
        // create wallet1 on app1
        var walletDom = await WalletDom.Create(TestInit1);
        var walletDom2 = await WalletDom.Create(TestInit1);

        // create order
        var request = new CreateOrderRequest
        {
            CurrencyId = -1,
            OrderId = Guid.NewGuid(),
            TransactionType = TransactionType.Authorize,
            ParticipantWallets = new List<ParticipantTransferItem>
            {
                new ()
                {
                    SenderWalletId = walletDom.Wallet.WalletId,
                    ReceiverWalletId = walletDom2.Wallet.WalletId,
                    Amount = 100
                }
            }
        };
        try
        {
            await TestInit1.OrdersClient.CreateOrderAsync(TestInit1.AppId, request);
            Assert.Fail("exception is expected.");
        }
        catch (ApiException ex)
        {
            Assert.AreEqual(nameof(NotExistsException), ex.ExceptionTypeName);
        }
    }

    [TestMethod]
    public async Task Fail_create_an_order_with_negative_Amount()
    {
        // create wallet1 on app1
        var walletDom = await WalletDom.Create(TestInit1);

        // create order
        try
        {
            await walletDom.CreateOrder(TestInit1, amount: -1);
            Assert.Fail("exception is expected.");
        }
        catch (ApiException ex)
        {
            Assert.AreEqual(nameof(InvalidOperationException), ex.ExceptionTypeName);
            Assert.IsTrue(ex.Message.Contains("Amount"));
        }
    }

    [TestMethod]
    public async Task Fail_create_an_order_with_zero_Amount()
    {
        // create wallet1 on app1
        var walletDom = await WalletDom.Create(TestInit1);

        // create order
        try
        {
            await walletDom.CreateOrder(TestInit1, amount: 0);
            Assert.Fail("exception is expected.");
        }
        catch (ApiException ex)
        {
            Assert.AreEqual(nameof(InvalidOperationException), ex.ExceptionTypeName);
            Assert.IsTrue(ex.Message.Contains("Amount"));
        }
    }

    [TestMethod]
    public async Task Fail_create_an_order_with_duplicate_items()
    {
        // create wallet1 on app1
        var walletDom = await WalletDom.Create(TestInit1);
        var walletDom2 = await WalletDom.Create(TestInit1);

        // create order
        var request = new CreateOrderRequest
        {
            CurrencyId = walletDom.CurrencyId,
            OrderId = Guid.NewGuid(),
            TransactionType = TransactionType.Authorize,
            ParticipantWallets = new List<ParticipantTransferItem>
            {
                new ()
                {
                    SenderWalletId = walletDom.Wallet.WalletId,
                    ReceiverWalletId = walletDom2.Wallet.WalletId,
                    Amount = 100
                },
                new ()
                {
                    SenderWalletId = walletDom.Wallet.WalletId,
                    ReceiverWalletId = walletDom2.Wallet.WalletId,
                    Amount = 100
                }
            }
        };
        try
        {
            await TestInit1.OrdersClient.CreateOrderAsync(TestInit1.AppId, request);
            Assert.Fail("exception is expected.");
        }
        catch (ApiException ex)
        {
            Assert.AreEqual(nameof(InvalidOperationException), ex.ExceptionTypeName);
            Assert.IsTrue(ex.Message.Contains("duplicate records"));
        }
    }

    [TestMethod]
    public async Task Fail_create_an_order_With_same_sender_and_receiver()
    {
        // create wallet1 on app1
        var walletDom = await WalletDom.Create(TestInit1);

        // create order
        try
        {
            await walletDom.CreateOrder(TestInit1, senderWalletId: walletDom.Wallet.WalletId, receiverWalletId: walletDom.Wallet.WalletId);
            Assert.Fail("exception is expected.");
        }
        catch (ApiException ex)
        {
            Assert.AreEqual(nameof(InvalidOperationException), ex.ExceptionTypeName);
            Assert.IsTrue(ex.Message.Contains("SenderWallet and ReceiverWallet can not be same."));
        }
    }

    [TestMethod]
    public async Task Fail_create_an_order_when_senderWallet_does_not_belong_to_app()
    {
        // create wallet1 on app1
        var walletDom = await WalletDom.Create(TestInit1);

        // create app2
        var testInit2 = await TestInit.Create();

        // create wallet1 on app2
        var walletDom2 = await WalletDom.Create(testInit2);

        // create order
        var request = new CreateOrderRequest
        {
            CurrencyId = walletDom2.CurrencyId,
            OrderId = Guid.NewGuid(),
            TransactionType = TransactionType.Authorize,
            ParticipantWallets = new List<ParticipantTransferItem>
            {
                new ()
                {
                    SenderWalletId = walletDom.Wallet.WalletId,
                    ReceiverWalletId = walletDom2.Wallet.WalletId,
                    Amount = 100
                }
            }
        };
        try
        {
            await testInit2.OrdersClient.CreateOrderAsync(testInit2.AppId, request);
            Assert.Fail("exception is expected.");
        }
        catch (ApiException ex)
        {
            Assert.AreEqual(nameof(InvalidOperationException), ex.ExceptionTypeName);
            Assert.IsTrue(ex.Message.Contains("some wallets does not belong to the app"));
            Assert.IsTrue(ex.Message.Contains($"{walletDom.Wallet.WalletId}"));
        }
    }

    [TestMethod]
    public async Task Fail_create_an_order_when_receiverWallet_does_not_belong_to_app()
    {
        // create wallet1 on app1
        var walletDom = await WalletDom.Create(TestInit1);

        // create app2
        var testInit2 = await TestInit.Create();

        // create wallet1 on app2
        var walletDom2 = await WalletDom.Create(testInit2);

        // create order
        var request = new CreateOrderRequest
        {
            CurrencyId = walletDom2.CurrencyId,
            OrderId = Guid.NewGuid(),
            TransactionType = TransactionType.Authorize,
            ParticipantWallets = new List<ParticipantTransferItem>
            {
                new ()
                {
                    SenderWalletId = walletDom2.Wallet.WalletId,
                    ReceiverWalletId = walletDom.Wallet.WalletId,
                    Amount = 100
                }
            }
        };
        try
        {
            await testInit2.OrdersClient.CreateOrderAsync(testInit2.AppId, request);
            Assert.Fail("exception is expected.");
        }
        catch (ApiException ex)
        {
            Assert.AreEqual(nameof(InvalidOperationException), ex.ExceptionTypeName);
            Assert.IsTrue(ex.Message.Contains("some wallets does not belong to the app"));
            Assert.IsTrue(ex.Message.Contains($"{walletDom.Wallet.WalletId}"));
        }
    }

    [TestMethod]
    public async Task Fail_create_an_order_when_senderWallet_and_receiverWallet_has_same_app_but_does_not_belong_to_the_requested_app()
    {
        // create app2
        var testInit2 = await TestInit.Create();
        var walletDomApp2 = await WalletDom.Create(testInit2);

        // create wallet1
        var walletDom = await WalletDom.Create(TestInit1);
        var walletDom2 = await WalletDom.Create(TestInit1);

        // create order
        var request = new CreateOrderRequest
        {
            CurrencyId = walletDomApp2.CurrencyId,
            OrderId = Guid.NewGuid(),
            TransactionType = TransactionType.Authorize,
            ParticipantWallets = new List<ParticipantTransferItem>
            {
                new ()
                {
                    SenderWalletId = walletDom2.Wallet.WalletId,
                    ReceiverWalletId = walletDom.Wallet.WalletId,
                    Amount = 100
                }
            }
        };
        try
        {
            await testInit2.OrdersClient.CreateOrderAsync(testInit2.AppId, request);
            Assert.Fail("exception is expected.");
        }
        catch (ApiException ex)
        {
            Assert.AreEqual(nameof(InvalidOperationException), ex.ExceptionTypeName);
            Assert.IsTrue(ex.Message.Contains("some wallets does not belong to the app"));
            Assert.IsTrue(ex.Message.Contains($"{walletDom.Wallet.WalletId}"));
            Assert.IsTrue(ex.Message.Contains($"{walletDom2.Wallet.WalletId}"));
        }
    }

    [TestMethod]
    public async Task Fail_get_order_with_invalid_orderId()
    {
        // create wallet1
        var walletDom = await WalletDom.Create(TestInit1);

        // create order
        await walletDom.CreateOrder(TestInit1);

        try
        {
            await TestInit1.OrdersClient.GetOrderAsync(TestInit1.AppId, Guid.NewGuid());
            Assert.Fail("order does not exists exception is expected.");
        }
        catch (ApiException ex)
        {
            Assert.AreEqual(nameof(NotExistsException), ex.ExceptionTypeName);
        }
    }

    [TestMethod]
    public async Task Fail_capture_an_order_that_already_captured()
    {
        // create wallet1
        var walletDom = await WalletDom.Create(TestInit1);

        // create order
        var order = await walletDom.CreateOrder(TestInit1);

        // capture order
        await walletDom.Capture(TestInit1, order.OrderId);

        // capture the order again
        try
        {
            await walletDom.Capture(TestInit1, order.OrderId);
            Assert.Fail("already captured exception is expected.");
        }
        catch (ApiException ex)
        {
            Assert.AreEqual(nameof(OrderAlreadySetAsRequestedStateException), ex.ExceptionTypeName);
        }
    }

    [TestMethod]
    public async Task Fail_capture_an_order_that_already_voided()
    {
        // create wallet1
        var walletDom = await WalletDom.Create(TestInit1);

        // create order
        var order = await walletDom.CreateOrder(TestInit1);

        // capture order
        await walletDom.Void(TestInit1, order.OrderId);

        // capture the order again
        try
        {
            await walletDom.Capture(TestInit1, order.OrderId);
            Assert.Fail("already captured exception is expected.");
        }
        catch (ApiException ex)
        {
            Assert.AreEqual(nameof(InvalidTransactionTypeException), ex.ExceptionTypeName);
        }
    }

    [TestMethod]
    public async Task Fail_capture_an_order_that_already_captured_by_sale()
    {
        // create wallet1
        var walletDom = await WalletDom.Create(TestInit1);

        // create order
        var order = await walletDom.CreateOrder(TestInit1, transactionType: TransactionType.Sale);

        // capture the order again
        try
        {
            await walletDom.Capture(TestInit1, order.OrderId);
            Assert.Fail("already captured exception is expected.");
        }
        catch (ApiException ex)
        {
            Assert.AreEqual(nameof(OrderAlreadySetAsRequestedStateException), ex.ExceptionTypeName);
        }
    }

    [TestMethod]
    public async Task Fail_void_an_order_that_already_voided()
    {
        // create wallet1
        var walletDom = await WalletDom.Create(TestInit1);

        // create order
        var order = await walletDom.CreateOrder(TestInit1);

        // capture order
        await walletDom.Void(TestInit1, order.OrderId);

        // capture the order again
        try
        {
            await walletDom.Void(TestInit1, order.OrderId);
            Assert.Fail("already voided exception is expected.");
        }
        catch (ApiException ex)
        {
            Assert.AreEqual(nameof(OrderAlreadySetAsRequestedStateException), ex.ExceptionTypeName);
        }
    }

    [TestMethod]
    public async Task Fail_void_with_wrong_orderId()
    {
        // create wallet1
        var walletDom = await WalletDom.Create(TestInit1);

        // create order
        await walletDom.CreateOrder(TestInit1);

        try
        {
            await walletDom.Void(TestInit1, Guid.NewGuid());
            Assert.Fail("wrong orderId exception is expected.");
        }
        catch (ApiException ex)
        {
            Assert.AreEqual(nameof(NotExistsException), ex.ExceptionTypeName);
        }
    }

    [TestMethod]
    public async Task Fail_create_order_with_system_wallet()
    {
        // create wallet1
        var app = await TestInit1.WalletDbContext.Apps.SingleAsync(x => x.AppId == TestInit1.AppId);
        var walletDom = await WalletDom.Create(TestInit1, null);

        // create order
        try
        {
            await walletDom.CreateOrder(TestInit1, senderWalletId: app.SystemWalletId, receiverWalletId: walletDom.Wallet.WalletId);
            Assert.Fail("Invalid operation exception is expected.");
        }
        catch (ApiException ex)
        {
            Assert.AreEqual(nameof(InvalidOperationException), ex.ExceptionTypeName);
        }
    }

    [TestMethod]
    public async Task Fail_capture_with_wrong_orderId()
    {
        // create wallet1
        var walletDom = await WalletDom.Create(TestInit1);

        // create order
        await walletDom.CreateOrder(TestInit1);

        try
        {
            await walletDom.Capture(TestInit1, Guid.NewGuid());
            Assert.Fail("wrong orderId exception is expected.");
        }
        catch (ApiException ex)
        {
            Assert.AreEqual(nameof(NotExistsException), ex.ExceptionTypeName);
        }
    }

    [TestMethod]
    public async Task Success_Sale_with_min_balance()
    {
        // create wallet1
        var walletDom = await WalletDom.Create(TestInit1);

        // create wallet2
        var walletDom2 = await WalletDom.Create(TestInit1, null);

        // create Sale order between created wallets
        await walletDom.CreateOrder(TestInit1, transactionType: TransactionType.Sale,
             receiverWalletId: walletDom2.Wallet.WalletId, amount: 100);
    }

    [TestMethod]
    public async Task Success_Sale_with_balance()
    {
        // create wallet1
        var walletDom = await WalletDom.Create(TestInit1);

        // create wallet2
        var walletDom2 = await WalletDom.Create(TestInit1, null);

        // create wallet3
        var walletDom3 = await WalletDom.Create(TestInit1, null);

        // increase balance of wallet2
        await walletDom.CreateOrder(TestInit1, transactionType: TransactionType.Sale,
            receiverWalletId: walletDom2.Wallet.WalletId, amount: 100);

        // create Sale order between created wallets
        await walletDom2.CreateOrder(TestInit1, transactionType: TransactionType.Sale,
            receiverWalletId: walletDom3.Wallet.WalletId, amount: 100, currencyId: walletDom.CurrencyId);
    }

    [TestMethod]
    public async Task Success_Sale_combination_balance_and_min_balance()
    {
        // create wallet1
        var walletDom = await WalletDom.Create(TestInit1);

        // create wallet2
        var walletDom2 = await WalletDom.Create(TestInit1, -51, walletDom.CurrencyId);

        // create wallet3
        var walletDom3 = await WalletDom.Create(TestInit1, null, walletDom.CurrencyId);

        // increase balance of wallet2
        await walletDom.CreateOrder(TestInit1, transactionType: TransactionType.Sale,
            receiverWalletId: walletDom2.Wallet.WalletId, amount: 100);

        // create Sale order between created wallets
        await walletDom2.CreateOrder(TestInit1, transactionType: TransactionType.Sale,
            receiverWalletId: walletDom3.Wallet.WalletId, amount: 150, currencyId: walletDom.CurrencyId);
    }

    [TestMethod]
    public async Task Fail_create_an_order_with_insufficient_balance_with_Sale()
    {
        // create wallet1
        var walletDom = await WalletDom.Create(TestInit1, null);

        // create order
        try
        {
            await walletDom.CreateOrder(TestInit1);
            Assert.Fail("InsufficientBalance exception is expected.");
        }
        catch (ApiException ex)
        {
            Assert.AreEqual(nameof(InsufficientBalanceException), ex.ExceptionTypeName);
        }
    }

    [TestMethod]
    public async Task Success_Authorize_with_balance()
    {
        // create wallet1
        var walletDom = await WalletDom.Create(TestInit1);

        // create wallet2
        var walletDom2 = await WalletDom.Create(TestInit1, null);

        // create wallet3
        var walletDom3 = await WalletDom.Create(TestInit1, null);

        // increase balance of wallet2
        await walletDom.CreateOrder(TestInit1, transactionType: TransactionType.Sale,
            receiverWalletId: walletDom2.Wallet.WalletId, amount: 100);

        // create Sale order between created wallets
        await walletDom2.CreateOrder(TestInit1, transactionType: TransactionType.Authorize,
            receiverWalletId: walletDom3.Wallet.WalletId, amount: 100, currencyId: walletDom.CurrencyId);
    }

    [TestMethod]
    public async Task Success_Authorize_with_min_balance()
    {
        // create wallet1
        var walletDom = await WalletDom.Create(TestInit1);

        // create wallet2
        var walletDom2 = await WalletDom.Create(TestInit1, null);

        // create Sale order between created wallets
        await walletDom.CreateOrder(TestInit1, transactionType: TransactionType.Authorize,
            receiverWalletId: walletDom2.Wallet.WalletId, amount: 100);
    }

    [TestMethod]
    public async Task Success_Authorize_with_combination_balance_and_min_balance()
    {
        // create wallet1
        var walletDom = await WalletDom.Create(TestInit1);

        // create wallet2
        var walletDom2 = await WalletDom.Create(TestInit1, -51, walletDom.CurrencyId);

        // create wallet3
        var walletDom3 = await WalletDom.Create(TestInit1, null, walletDom.CurrencyId);

        // increase balance of wallet2
        await walletDom.CreateOrder(TestInit1, transactionType: TransactionType.Sale,
            receiverWalletId: walletDom2.Wallet.WalletId, amount: 100);

        // create Sale order between created wallets
        await walletDom2.CreateOrder(TestInit1, transactionType: TransactionType.Authorize,
            receiverWalletId: walletDom3.Wallet.WalletId, amount: 150, currencyId: walletDom.CurrencyId);
    }

    [TestMethod]
    public async Task Fail_create_an_order_with_insufficient_balance_with_Authorize()
    {
        // create wallet1
        var walletDom = await WalletDom.Create(TestInit1, null);

        // create order
        try
        {
            await walletDom.CreateOrder(TestInit1, transactionType: TransactionType.Sale);
            Assert.Fail("InsufficientBalance exception is expected.");
        }
        catch (ApiException ex)
        {
            Assert.AreEqual(nameof(InsufficientBalanceException), ex.ExceptionTypeName);
        }
    }

    [TestMethod]
    public async Task Void_authorized()
    {
        // create wallet1
        var walletDom = await WalletDom.Create(TestInit1);

        // create an authorize order
        var orderId = Guid.NewGuid();
        await walletDom.CreateOrder(TestInit1, orderId: orderId);

        // void the order
        await walletDom.Void(TestInit1, orderId);
    }

    [TestMethod]
    public async Task Void_captured()
    {
        // create wallet1
        var walletDom = await WalletDom.Create(TestInit1);

        // create an authorize order
        var order = await walletDom.CreateOrder(TestInit1);

        // capture the order
        await walletDom.Capture(TestInit1, order.OrderId);

        // void the order
        await walletDom.Void(TestInit1, order.OrderId);
    }

    [TestMethod]
    public async Task Void_sale()
    {
        // create wallet1
        var walletDom = await WalletDom.Create(TestInit1);

        // create an authorize order
        var orderId = Guid.NewGuid();
        await walletDom.CreateOrder(TestInit1, orderId: orderId, transactionType: TransactionType.Sale, amount: 100);
        var receiverWallet = walletDom.ReceiverWallet;
        ArgumentNullException.ThrowIfNull(receiverWallet);

        var walletDom2 = await WalletDom.Create(TestInit1);
        await walletDom2.CreateOrder(TestInit1, senderWalletId: receiverWallet.WalletId, receiverWalletId: walletDom2.Wallet.WalletId,
            amount: 100, currencyId: walletDom.CurrencyId);

        // void the order
        await walletDom.Void(TestInit1, orderId);
    }

    [TestMethod]
    public async Task Void_sale_without_balance()
    {
        // create wallet1
        var walletDom = await WalletDom.Create(TestInit1);

        // create an authorize order
        var orderId = Guid.NewGuid();
        await walletDom.CreateOrder(TestInit1, orderId: orderId, transactionType: TransactionType.Sale);

        // void the order
        await walletDom.Void(TestInit1, orderId);
    }

    [TestMethod]
    public async Task Success_Capture()
    {
        // create wallet1
        var walletDom = await WalletDom.Create(TestInit1);

        // create order
        var orderId = Guid.NewGuid();
        const decimal amount = 100;
        await walletDom.CreateOrder(TestInit1, amount: amount, orderId: orderId);

        // capture the order
        await walletDom.Capture(TestInit1, orderId);

        // get order info and validate
        var order = await TestInit1.OrdersClient.GetOrderAsync(TestInit1.AppId, orderId);
        ArgumentNullException.ThrowIfNull(walletDom.ReceiverWallet);

        Assert.AreEqual(walletDom.CurrencyId, order.CurrencyId);
        Assert.AreEqual(orderId, order.OrderId);
        Assert.AreEqual(TransactionType.Authorize, order.TransactionType);
        Assert.AreEqual(OrderStatus.Captured, order.Status);
        Assert.IsNotNull(order.CapturedTime);
        Assert.AreEqual(1, order.Items.Count);
        Assert.IsNotNull(order.Items.SingleOrDefault(x =>
            x.Amount == amount &&
            x.SenderWalletId == walletDom.Wallet.WalletId &&
            x.ReceiverWalletId == walletDom.ReceiverWallet.WalletId));
    }

    // document url: https://docs.google.com/spreadsheets/d/1vrY9GFmJwW-qulNaFsScPbPonDP4qdIJCtq4TCzBlLA/edit#gid=0
    [TestMethod]
    public async Task Void()
    {
        // Create wallets
        var systemWalletDom = await WalletDom.Create(TestInit1);
        var walletDom1 = await WalletDom.Create(TestInit1, minBalance: null, currencyId: systemWalletDom.CurrencyId);
        var walletDom2 = await WalletDom.Create(TestInit1, minBalance: null, currencyId: systemWalletDom.CurrencyId);
        var walletDom3 = await WalletDom.Create(TestInit1, minBalance: null, currencyId: systemWalletDom.CurrencyId);
        var walletDom4 = await WalletDom.Create(TestInit1, minBalance: null, currencyId: systemWalletDom.CurrencyId);
        var walletDom5 = await WalletDom.Create(TestInit1, minBalance: null, currencyId: systemWalletDom.CurrencyId);
        var walletDom6 = await WalletDom.Create(TestInit1, minBalance: null, currencyId: systemWalletDom.CurrencyId);

        // Transfer to wallets in order to make available balances based on document
        await systemWalletDom.CreateOrder(TestInit1, receiverWalletId: walletDom1.Wallet.WalletId, amount: 100, transactionType: TransactionType.Sale);
        await systemWalletDom.CreateOrder(TestInit1, receiverWalletId: walletDom2.Wallet.WalletId, amount: 150, transactionType: TransactionType.Sale);
        await systemWalletDom.CreateOrder(TestInit1, receiverWalletId: walletDom4.Wallet.WalletId, amount: 20, transactionType: TransactionType.Sale);
        await systemWalletDom.CreateOrder(TestInit1, receiverWalletId: walletDom5.Wallet.WalletId, amount: 40, transactionType: TransactionType.Sale);

        // Transfer to wallets in order to make pending balances based on document
        await systemWalletDom.CreateOrder(TestInit1, receiverWalletId: walletDom1.Wallet.WalletId, amount: 50, transactionType: TransactionType.Authorize);
        await systemWalletDom.CreateOrder(TestInit1, receiverWalletId: walletDom2.Wallet.WalletId, amount: 70, transactionType: TransactionType.Authorize);
        await systemWalletDom.CreateOrder(TestInit1, receiverWalletId: walletDom3.Wallet.WalletId, amount: 90, transactionType: TransactionType.Authorize);
        await systemWalletDom.CreateOrder(TestInit1, receiverWalletId: walletDom4.Wallet.WalletId, amount: 10, transactionType: TransactionType.Authorize);

        var orderTypeId = new Random().Next(int.MinValue, int.MaxValue);
        // Create order request
        var request = new CreateOrderRequest
        {
            OrderTypeId = orderTypeId,
            OrderId = Guid.NewGuid(),
            CurrencyId = systemWalletDom.CurrencyId,
            TransactionType = TransactionType.Authorize,
            ParticipantWallets = new List<ParticipantTransferItem>
            {
                new()
                {
                    SenderWalletId = walletDom1.Wallet.WalletId,
                    ReceiverWalletId = walletDom2.Wallet.WalletId,
                    Amount = 100
                },
                new()
                {
                    SenderWalletId = walletDom2.Wallet.WalletId,
                    ReceiverWalletId = walletDom3.Wallet.WalletId,
                    Amount = 150
                },
                new()
                {
                    SenderWalletId = walletDom3.Wallet.WalletId,
                    ReceiverWalletId = walletDom4.Wallet.WalletId,
                    Amount = 150
                },
                new()
                {
                    SenderWalletId = walletDom4.Wallet.WalletId,
                    ReceiverWalletId = walletDom5.Wallet.WalletId,
                    Amount = 30
                },
                new()
                {
                    SenderWalletId = walletDom5.Wallet.WalletId,
                    ReceiverWalletId = walletDom6.Wallet.WalletId,
                    Amount = 20
                },
                new()
                {
                    SenderWalletId = walletDom5.Wallet.WalletId,
                    ReceiverWalletId = walletDom2.Wallet.WalletId,
                    Amount = 40
                }
            }
        };

        // create order
        var order = await TestInit1.OrdersClient.CreateOrderAsync(TestInit1.AppId, request);

        // void the order
        await TestInit1.OrdersClient.VoidAsync(TestInit1.AppId, order.OrderId);

        // Get wallets after transfer
        var wallet1 = await TestInit1.WalletsClient.GetWalletAsync(TestInit1.AppId, walletDom1.Wallet.WalletId);
        var wallet2 = await TestInit1.WalletsClient.GetWalletAsync(TestInit1.AppId, walletDom2.Wallet.WalletId);
        var wallet3 = await TestInit1.WalletsClient.GetWalletAsync(TestInit1.AppId, walletDom3.Wallet.WalletId);
        var wallet4 = await TestInit1.WalletsClient.GetWalletAsync(TestInit1.AppId, walletDom4.Wallet.WalletId);
        var wallet5 = await TestInit1.WalletsClient.GetWalletAsync(TestInit1.AppId, walletDom5.Wallet.WalletId);
        var wallet6 = await TestInit1.WalletsClient.GetWalletAsync(TestInit1.AppId, walletDom6.Wallet.WalletId);

        // Available balance of wallets after transfer
        ArgumentNullException.ThrowIfNull(wallet1.Currencies);
        ArgumentNullException.ThrowIfNull(wallet2.Currencies);
        ArgumentNullException.ThrowIfNull(wallet3.Currencies);
        ArgumentNullException.ThrowIfNull(wallet4.Currencies);
        ArgumentNullException.ThrowIfNull(wallet5.Currencies);
        ArgumentNullException.ThrowIfNull(wallet6.Currencies);
        var wallet1AvailableBalance = wallet1.Currencies.Single(c => c.CurrencyId == systemWalletDom.CurrencyId).Balance;
        var wallet2AvailableBalance = wallet2.Currencies.Single(c => c.CurrencyId == systemWalletDom.CurrencyId).Balance;
        var wallet3AvailableBalance = wallet3.Currencies.Single(c => c.CurrencyId == systemWalletDom.CurrencyId).Balance;
        var wallet4AvailableBalance = wallet4.Currencies.Single(c => c.CurrencyId == systemWalletDom.CurrencyId).Balance;
        var wallet5AvailableBalance = wallet5.Currencies.Single(c => c.CurrencyId == systemWalletDom.CurrencyId).Balance;
        var wallet6AvailableBalance = wallet6.Currencies.SingleOrDefault(c => c.CurrencyId == systemWalletDom.CurrencyId)?.Balance;

        // Assert
        Assert.AreEqual(100, wallet1AvailableBalance);
        Assert.AreEqual(150, wallet2AvailableBalance);
        Assert.AreEqual(0, wallet3AvailableBalance);
        Assert.AreEqual(20, wallet4AvailableBalance);
        Assert.AreEqual(40, wallet5AvailableBalance);
        Assert.IsNull(wallet6AvailableBalance);
    }

    [TestMethod]
    public async Task Sale()
    {
        // Create wallets
        var systemWalletDom = await WalletDom.Create(TestInit1);
        var walletDom1 = await WalletDom.Create(TestInit1, minBalance: null, currencyId: systemWalletDom.CurrencyId);
        var walletDom2 = await WalletDom.Create(TestInit1, minBalance: null, currencyId: systemWalletDom.CurrencyId);
        var walletDom3 = await WalletDom.Create(TestInit1, minBalance: null, currencyId: systemWalletDom.CurrencyId);
        var walletDom4 = await WalletDom.Create(TestInit1, minBalance: null, currencyId: systemWalletDom.CurrencyId);
        var walletDom5 = await WalletDom.Create(TestInit1, minBalance: null, currencyId: systemWalletDom.CurrencyId);
        var walletDom6 = await WalletDom.Create(TestInit1, minBalance: null, currencyId: systemWalletDom.CurrencyId);

        // Transfer to wallets in order to make available balances based on document
        await systemWalletDom.CreateOrder(TestInit1, receiverWalletId: walletDom1.Wallet.WalletId, amount: 100, transactionType: TransactionType.Sale);
        await systemWalletDom.CreateOrder(TestInit1, receiverWalletId: walletDom2.Wallet.WalletId, amount: 150, transactionType: TransactionType.Sale);
        await systemWalletDom.CreateOrder(TestInit1, receiverWalletId: walletDom4.Wallet.WalletId, amount: 20, transactionType: TransactionType.Sale);
        await systemWalletDom.CreateOrder(TestInit1, receiverWalletId: walletDom5.Wallet.WalletId, amount: 40, transactionType: TransactionType.Sale);

        var orderTypeId = new Random().Next(int.MinValue, int.MaxValue);
        // Create order request
        var request = new CreateOrderRequest
        {
            OrderTypeId = orderTypeId,
            OrderId = Guid.NewGuid(),
            CurrencyId = systemWalletDom.CurrencyId,
            TransactionType = TransactionType.Sale,
            ParticipantWallets = new List<ParticipantTransferItem>
                {
                    new ()
                    {
                        SenderWalletId = walletDom1.Wallet.WalletId,
                        ReceiverWalletId = walletDom2.Wallet.WalletId,
                        Amount = 100
                    },
                    new ()
                    {
                        SenderWalletId = walletDom2.Wallet.WalletId,
                        ReceiverWalletId = walletDom3.Wallet.WalletId,
                        Amount = 150
                    },
                    new ()
                    {
                        SenderWalletId = walletDom3.Wallet.WalletId,
                        ReceiverWalletId = walletDom4.Wallet.WalletId,
                        Amount = 150
                    },
                    new ()
                    {
                        SenderWalletId = walletDom4.Wallet.WalletId,
                        ReceiverWalletId = walletDom5.Wallet.WalletId,
                        Amount = 30
                    },
                    new ()
                    {
                        SenderWalletId = walletDom5.Wallet.WalletId,
                        ReceiverWalletId = walletDom6.Wallet.WalletId,
                        Amount = 20
                    },
                    new ()
                    {
                        SenderWalletId = walletDom5.Wallet.WalletId,
                        ReceiverWalletId = walletDom2.Wallet.WalletId,
                        Amount = 40
                    }
                }
        };

        // create order
        await TestInit1.OrdersClient.CreateOrderAsync(TestInit1.AppId, request);

        // Get wallets after transfer
        var wallet1 = await TestInit1.WalletsClient.GetWalletAsync(TestInit1.AppId, walletDom1.Wallet.WalletId);
        var wallet2 = await TestInit1.WalletsClient.GetWalletAsync(TestInit1.AppId, walletDom2.Wallet.WalletId);
        var wallet3 = await TestInit1.WalletsClient.GetWalletAsync(TestInit1.AppId, walletDom3.Wallet.WalletId);
        var wallet4 = await TestInit1.WalletsClient.GetWalletAsync(TestInit1.AppId, walletDom4.Wallet.WalletId);
        var wallet5 = await TestInit1.WalletsClient.GetWalletAsync(TestInit1.AppId, walletDom5.Wallet.WalletId);
        var wallet6 = await TestInit1.WalletsClient.GetWalletAsync(TestInit1.AppId, walletDom6.Wallet.WalletId);

        // Available balance of wallets after transfer
        ArgumentNullException.ThrowIfNull(wallet1.Currencies);
        ArgumentNullException.ThrowIfNull(wallet2.Currencies);
        ArgumentNullException.ThrowIfNull(wallet3.Currencies);
        ArgumentNullException.ThrowIfNull(wallet4.Currencies);
        ArgumentNullException.ThrowIfNull(wallet5.Currencies);
        ArgumentNullException.ThrowIfNull(wallet6.Currencies);
        var wallet1AvailableBalance = wallet1.Currencies.Single(c => c.CurrencyId == systemWalletDom.CurrencyId).Balance;
        var wallet2AvailableBalance = wallet2.Currencies.Single(c => c.CurrencyId == systemWalletDom.CurrencyId).Balance;
        var wallet3AvailableBalance = wallet3.Currencies.Single(c => c.CurrencyId == systemWalletDom.CurrencyId).Balance;
        var wallet4AvailableBalance = wallet4.Currencies.Single(c => c.CurrencyId == systemWalletDom.CurrencyId).Balance;
        var wallet5AvailableBalance = wallet5.Currencies.Single(c => c.CurrencyId == systemWalletDom.CurrencyId).Balance;
        var wallet6AvailableBalance = wallet6.Currencies.Single(c => c.CurrencyId == systemWalletDom.CurrencyId).Balance;

        // Assert
        Assert.AreEqual(0, wallet1AvailableBalance);
        Assert.AreEqual(140, wallet2AvailableBalance);
        Assert.AreEqual(0, wallet3AvailableBalance);
        Assert.AreEqual(140, wallet4AvailableBalance);
        Assert.AreEqual(10, wallet5AvailableBalance);
        Assert.AreEqual(20, wallet6AvailableBalance);
    }

    [TestMethod]
    public async Task Authorize()
    {
        // Create wallets
        var systemWalletDom = await WalletDom.Create(TestInit1);
        var walletDom1 = await WalletDom.Create(TestInit1);
        var walletDom2 = await WalletDom.Create(TestInit1);
        var walletDom3 = await WalletDom.Create(TestInit1);
        var walletDom4 = await WalletDom.Create(TestInit1);
        var walletDom5 = await WalletDom.Create(TestInit1);
        var walletDom6 = await WalletDom.Create(TestInit1);

        // Transfer to wallets in order to make available balances based on document
        await systemWalletDom.CreateOrder(TestInit1, receiverWalletId: walletDom1.Wallet.WalletId, amount: 100, transactionType: TransactionType.Sale);
        await systemWalletDom.CreateOrder(TestInit1, receiverWalletId: walletDom2.Wallet.WalletId, amount: 150, transactionType: TransactionType.Sale);
        await systemWalletDom.CreateOrder(TestInit1, receiverWalletId: walletDom4.Wallet.WalletId, amount: 20, transactionType: TransactionType.Sale);
        await systemWalletDom.CreateOrder(TestInit1, receiverWalletId: walletDom5.Wallet.WalletId, amount: 40, transactionType: TransactionType.Sale);

        // Create order request
        var orderTypeId = new Random().Next(int.MinValue, int.MaxValue);
        var request = new CreateOrderRequest
        {
            OrderId = Guid.NewGuid(),
            CurrencyId = systemWalletDom.CurrencyId,
            OrderTypeId = orderTypeId,
            TransactionType = TransactionType.Authorize,
            ParticipantWallets = new List<ParticipantTransferItem>
                {
                    new ()
                    {
                        SenderWalletId = walletDom1.Wallet.WalletId,
                        ReceiverWalletId = walletDom2.Wallet.WalletId,
                        Amount = 100
                    },
                    new ()
                    {
                        SenderWalletId = walletDom2.Wallet.WalletId,
                        ReceiverWalletId = walletDom3.Wallet.WalletId,
                        Amount = 150
                    },
                    new ()
                    {
                        SenderWalletId = walletDom3.Wallet.WalletId,
                        ReceiverWalletId = walletDom4.Wallet.WalletId,
                        Amount = 150
                    },
                    new ()
                    {
                        SenderWalletId = walletDom4.Wallet.WalletId,
                        ReceiverWalletId = walletDom5.Wallet.WalletId,
                        Amount = 30
                    },
                    new ()
                    {
                        SenderWalletId = walletDom5.Wallet.WalletId,
                        ReceiverWalletId = walletDom6.Wallet.WalletId,
                        Amount = 20
                    },
                    new ()
                    {
                        SenderWalletId = walletDom5.Wallet.WalletId,
                        ReceiverWalletId = walletDom2.Wallet.WalletId,
                        Amount = 40
                    }
                }
        };

        // create order
        await TestInit1.OrdersClient.CreateOrderAsync(TestInit1.AppId, request);

        // Get wallets after transfer
        var wallet1 = await TestInit1.WalletsClient.GetWalletAsync(TestInit1.AppId, walletDom1.Wallet.WalletId);
        var wallet2 = await TestInit1.WalletsClient.GetWalletAsync(TestInit1.AppId, walletDom2.Wallet.WalletId);
        var wallet3 = await TestInit1.WalletsClient.GetWalletAsync(TestInit1.AppId, walletDom3.Wallet.WalletId);
        var wallet4 = await TestInit1.WalletsClient.GetWalletAsync(TestInit1.AppId, walletDom4.Wallet.WalletId);
        var wallet5 = await TestInit1.WalletsClient.GetWalletAsync(TestInit1.AppId, walletDom5.Wallet.WalletId);
        var wallet6 = await TestInit1.WalletsClient.GetWalletAsync(TestInit1.AppId, walletDom6.Wallet.WalletId);

        // Available balance of wallets after transfer
        ArgumentNullException.ThrowIfNull(wallet1.Currencies);
        ArgumentNullException.ThrowIfNull(wallet2.Currencies);
        ArgumentNullException.ThrowIfNull(wallet3.Currencies);
        ArgumentNullException.ThrowIfNull(wallet4.Currencies);
        ArgumentNullException.ThrowIfNull(wallet5.Currencies);
        ArgumentNullException.ThrowIfNull(wallet6.Currencies);
        var wallet1AvailableBalance = wallet1.Currencies.Single(c => c.CurrencyId == systemWalletDom.CurrencyId).Balance;
        var wallet2AvailableBalance = wallet2.Currencies.Single(c => c.CurrencyId == systemWalletDom.CurrencyId).Balance;
        var wallet3AvailableBalance = wallet3.Currencies.Single(c => c.CurrencyId == systemWalletDom.CurrencyId).Balance;
        var wallet4AvailableBalance = wallet4.Currencies.Single(c => c.CurrencyId == systemWalletDom.CurrencyId).Balance;
        var wallet5AvailableBalance = wallet5.Currencies.Single(c => c.CurrencyId == systemWalletDom.CurrencyId).Balance;
        var wallet6AvailableBalance = wallet6.Currencies.SingleOrDefault(c => c.CurrencyId == systemWalletDom.CurrencyId)?.Balance;

        // Assert
        Assert.AreEqual(0, wallet1AvailableBalance);
        Assert.AreEqual(0, wallet2AvailableBalance);
        Assert.AreEqual(-150, wallet3AvailableBalance);
        Assert.AreEqual(-10, wallet4AvailableBalance);
        Assert.AreEqual(-20, wallet5AvailableBalance);
        Assert.IsNull(wallet6AvailableBalance);
    }

    [TestMethod]
    public async Task AuthorizeAndCapture()
    {
        // Create wallets
        var systemWalletDom = await WalletDom.Create(TestInit1);
        var walletDom1 = await WalletDom.Create(TestInit1);
        var walletDom2 = await WalletDom.Create(TestInit1);
        var walletDom3 = await WalletDom.Create(TestInit1);
        var walletDom4 = await WalletDom.Create(TestInit1);

        // Transfer to wallets in order to make available balances based on document
        await systemWalletDom.CreateOrder(TestInit1, receiverWalletId: walletDom1.Wallet.WalletId, amount: 100, transactionType: TransactionType.Sale);

        // Create order request
        var orderTypeId = new Random().Next(int.MinValue, int.MaxValue);
        var request = new CreateOrderRequest
        {
            OrderId = Guid.NewGuid(),
            CurrencyId = systemWalletDom.CurrencyId,
            OrderTypeId = orderTypeId,
            TransactionType = TransactionType.Authorize,
            ParticipantWallets = new List<ParticipantTransferItem>
                {
                    new ()
                    {
                        SenderWalletId = walletDom1.Wallet.WalletId,
                        ReceiverWalletId = walletDom2.Wallet.WalletId,
                        Amount = 100
                    },
                    new ()
                    {
                        SenderWalletId = walletDom2.Wallet.WalletId,
                        ReceiverWalletId = walletDom3.Wallet.WalletId,
                        Amount = 90
                    },
                    new ()
                    {
                        SenderWalletId = walletDom3.Wallet.WalletId,
                        ReceiverWalletId = walletDom4.Wallet.WalletId,
                        Amount = 80
                    }
                }
        };

        // create order
        var order = await TestInit1.OrdersClient.CreateOrderAsync(TestInit1.AppId, request);

        // Get wallets after transfer
        var wallet1 = await TestInit1.WalletsClient.GetWalletAsync(TestInit1.AppId, walletDom1.Wallet.WalletId);
        var wallet2 = await TestInit1.WalletsClient.GetWalletAsync(TestInit1.AppId, walletDom2.Wallet.WalletId);
        var wallet3 = await TestInit1.WalletsClient.GetWalletAsync(TestInit1.AppId, walletDom3.Wallet.WalletId);
        var wallet4 = await TestInit1.WalletsClient.GetWalletAsync(TestInit1.AppId, walletDom4.Wallet.WalletId);

        // Available balance of wallets after transfer
        ArgumentNullException.ThrowIfNull(wallet1.Currencies);
        ArgumentNullException.ThrowIfNull(wallet2.Currencies);
        ArgumentNullException.ThrowIfNull(wallet3.Currencies);
        ArgumentNullException.ThrowIfNull(wallet4.Currencies);
        var wallet1AvailableBalance = wallet1.Currencies.Single(c => c.CurrencyId == systemWalletDom.CurrencyId).Balance;
        var wallet2AvailableBalance = wallet2.Currencies.Single(c => c.CurrencyId == systemWalletDom.CurrencyId).Balance;
        var wallet3AvailableBalance = wallet3.Currencies.Single(c => c.CurrencyId == systemWalletDom.CurrencyId).Balance;

        // Assert
        Assert.AreEqual(0, wallet1AvailableBalance);
        Assert.AreEqual(-90, wallet2AvailableBalance);
        Assert.AreEqual(-80, wallet3AvailableBalance);
        Assert.IsNull(wallet4.Currencies.SingleOrDefault(c => c.CurrencyId == systemWalletDom.CurrencyId));

        // capture the order
        await TestInit1.OrdersClient.CaptureAsync(TestInit1.AppId, order.OrderId);

        // Get wallets after capture
        wallet1 = await TestInit1.WalletsClient.GetWalletAsync(TestInit1.AppId, walletDom1.Wallet.WalletId);
        wallet2 = await TestInit1.WalletsClient.GetWalletAsync(TestInit1.AppId, walletDom2.Wallet.WalletId);
        wallet3 = await TestInit1.WalletsClient.GetWalletAsync(TestInit1.AppId, walletDom3.Wallet.WalletId);
        wallet4 = await TestInit1.WalletsClient.GetWalletAsync(TestInit1.AppId, walletDom4.Wallet.WalletId);

        // Available balance of wallets after capture
        ArgumentNullException.ThrowIfNull(wallet1.Currencies);
        ArgumentNullException.ThrowIfNull(wallet2.Currencies);
        ArgumentNullException.ThrowIfNull(wallet3.Currencies);
        ArgumentNullException.ThrowIfNull(wallet4.Currencies);
        wallet1AvailableBalance = wallet1.Currencies.Single(c => c.CurrencyId == systemWalletDom.CurrencyId).Balance;
        wallet2AvailableBalance = wallet2.Currencies.Single(c => c.CurrencyId == systemWalletDom.CurrencyId).Balance;
        wallet3AvailableBalance = wallet3.Currencies.Single(c => c.CurrencyId == systemWalletDom.CurrencyId).Balance;
        var wallet4AvailableBalance = wallet4.Currencies.Single(c => c.CurrencyId == systemWalletDom.CurrencyId).Balance;

        // Assert
        Assert.AreEqual(0, wallet1AvailableBalance);
        Assert.AreEqual(10, wallet2AvailableBalance);
        Assert.AreEqual(10, wallet3AvailableBalance);
        Assert.AreEqual(80, wallet4AvailableBalance);
    }


    [TestMethod]
    public async Task Authorize_with_min_balance()
    {
        // Create wallets
        var systemWalletDom = await WalletDom.Create(TestInit1);
        var walletDom1 = await WalletDom.Create(TestInit1);
        var walletDom2 = await WalletDom.Create(TestInit1);
        var walletDom3 = await WalletDom.Create(TestInit1);

        // set min balance for wallet4
        var walletDom4 = await WalletDom.Create(TestInit1, minBalance: -30, currencyId: systemWalletDom.CurrencyId);

        var walletDom5 = await WalletDom.Create(TestInit1);
        var walletDom6 = await WalletDom.Create(TestInit1);

        // Transfer to wallets in order to make available balances based on document
        await systemWalletDom.CreateOrder(TestInit1, receiverWalletId: walletDom1.Wallet.WalletId, amount: 100, transactionType: TransactionType.Sale);
        await systemWalletDom.CreateOrder(TestInit1, receiverWalletId: walletDom2.Wallet.WalletId, amount: 150, transactionType: TransactionType.Sale);
        await systemWalletDom.CreateOrder(TestInit1, receiverWalletId: walletDom4.Wallet.WalletId, amount: 20, transactionType: TransactionType.Sale);
        await systemWalletDom.CreateOrder(TestInit1, receiverWalletId: walletDom5.Wallet.WalletId, amount: 40, transactionType: TransactionType.Sale);

        // Create order request
        var orderTypeId = new Random().Next(int.MinValue, int.MaxValue);
        var request = new CreateOrderRequest
        {
            OrderId = Guid.NewGuid(),
            CurrencyId = systemWalletDom.CurrencyId,
            OrderTypeId = orderTypeId,
            TransactionType = TransactionType.Authorize,
            ParticipantWallets = new List<ParticipantTransferItem>
                {
                    new ()
                    {
                        SenderWalletId = walletDom1.Wallet.WalletId,
                        ReceiverWalletId = walletDom2.Wallet.WalletId,
                        Amount = 100
                    },
                    new ()
                    {
                        SenderWalletId = walletDom2.Wallet.WalletId,
                        ReceiverWalletId = walletDom3.Wallet.WalletId,
                        Amount = 150
                    },
                    new ()
                    {
                        SenderWalletId = walletDom3.Wallet.WalletId,
                        ReceiverWalletId = walletDom4.Wallet.WalletId,
                        Amount = 150
                    },
                    new ()
                    {
                        SenderWalletId = walletDom4.Wallet.WalletId,
                        ReceiverWalletId = walletDom5.Wallet.WalletId,
                        Amount = 30
                    },
                    new ()
                    {
                        SenderWalletId = walletDom5.Wallet.WalletId,
                        ReceiverWalletId = walletDom6.Wallet.WalletId,
                        Amount = 20
                    },
                    new ()
                    {
                        SenderWalletId = walletDom5.Wallet.WalletId,
                        ReceiverWalletId = walletDom2.Wallet.WalletId,
                        Amount = 40
                    }
                }
        };

        // create order
        await TestInit1.OrdersClient.CreateOrderAsync(TestInit1.AppId, request);

        // Get wallets after transfer
        var wallet1 = await TestInit1.WalletsClient.GetWalletAsync(TestInit1.AppId, walletDom1.Wallet.WalletId);
        var wallet2 = await TestInit1.WalletsClient.GetWalletAsync(TestInit1.AppId, walletDom2.Wallet.WalletId);
        var wallet3 = await TestInit1.WalletsClient.GetWalletAsync(TestInit1.AppId, walletDom3.Wallet.WalletId);
        var wallet4 = await TestInit1.WalletsClient.GetWalletAsync(TestInit1.AppId, walletDom4.Wallet.WalletId);
        var wallet5 = await TestInit1.WalletsClient.GetWalletAsync(TestInit1.AppId, walletDom5.Wallet.WalletId);
        var wallet6 = await TestInit1.WalletsClient.GetWalletAsync(TestInit1.AppId, walletDom6.Wallet.WalletId);

        // Available balance of wallets after transfer
        ArgumentNullException.ThrowIfNull(wallet1.Currencies);
        ArgumentNullException.ThrowIfNull(wallet2.Currencies);
        ArgumentNullException.ThrowIfNull(wallet3.Currencies);
        ArgumentNullException.ThrowIfNull(wallet4.Currencies);
        ArgumentNullException.ThrowIfNull(wallet5.Currencies);
        ArgumentNullException.ThrowIfNull(wallet6.Currencies);
        var wallet1AvailableBalance = wallet1.Currencies.Single(c => c.CurrencyId == systemWalletDom.CurrencyId).Balance;
        var wallet2AvailableBalance = wallet2.Currencies.Single(c => c.CurrencyId == systemWalletDom.CurrencyId).Balance;
        var wallet3AvailableBalance = wallet3.Currencies.Single(c => c.CurrencyId == systemWalletDom.CurrencyId).Balance;
        var wallet4AvailableBalance = wallet4.Currencies.Single(c => c.CurrencyId == systemWalletDom.CurrencyId).Balance;
        var wallet4AvailableMinBalance = wallet4.Currencies.Single(c => c.CurrencyId == systemWalletDom.CurrencyId).MinBalance;
        var wallet5AvailableBalance = wallet5.Currencies.Single(c => c.CurrencyId == systemWalletDom.CurrencyId).Balance;
        var wallet6AvailableBalance = wallet6.Currencies.SingleOrDefault(c => c.CurrencyId == systemWalletDom.CurrencyId)?.Balance;

        // Assert
        Assert.AreEqual(0, wallet1AvailableBalance);
        Assert.AreEqual(0, wallet2AvailableBalance);
        Assert.AreEqual(-150, wallet3AvailableBalance);
        Assert.AreEqual(0, wallet4AvailableBalance);
        Assert.AreEqual(-20, wallet4AvailableMinBalance);
        Assert.AreEqual(-20, wallet5AvailableBalance);
        Assert.IsNull(wallet6AvailableBalance);
    }

    [TestMethod]
    public async Task InsufficientBalance_Authorize()
    {
        // Create wallets
        var systemWalletDom = await WalletDom.Create(TestInit1);
        var walletDom1 = await WalletDom.Create(TestInit1);
        var walletDom2 = await WalletDom.Create(TestInit1);
        var walletDom3 = await WalletDom.Create(TestInit1);
        var walletDom4 = await WalletDom.Create(TestInit1);
        var walletDom5 = await WalletDom.Create(TestInit1);

        // Transfer to wallets in order to make available balances based on document
        await systemWalletDom.CreateOrder(TestInit1, receiverWalletId: walletDom1.Wallet.WalletId, amount: 100, transactionType: TransactionType.Sale);
        await systemWalletDom.CreateOrder(TestInit1, receiverWalletId: walletDom2.Wallet.WalletId, amount: 150, transactionType: TransactionType.Sale);
        await systemWalletDom.CreateOrder(TestInit1, receiverWalletId: walletDom4.Wallet.WalletId, amount: 20, transactionType: TransactionType.Sale);
        await systemWalletDom.CreateOrder(TestInit1, receiverWalletId: walletDom5.Wallet.WalletId, amount: 40, transactionType: TransactionType.Sale);

        var orderTypeId = new Random().Next(int.MinValue, int.MaxValue);
        // Create order request
        var request = new CreateOrderRequest
        {
            OrderTypeId = orderTypeId,
            OrderId = Guid.NewGuid(),
            CurrencyId = systemWalletDom.CurrencyId,
            TransactionType = TransactionType.Authorize,
            ParticipantWallets = new List<ParticipantTransferItem>
                {
                    new ()
                    {
                        SenderWalletId = walletDom1.Wallet.WalletId,
                        ReceiverWalletId = walletDom2.Wallet.WalletId,
                        Amount = 101
                    },
                    new ()
                    {
                        SenderWalletId = walletDom2.Wallet.WalletId,
                        ReceiverWalletId = walletDom3.Wallet.WalletId,
                        Amount = 150
                    },
                    new ()
                    {
                        SenderWalletId = walletDom3.Wallet.WalletId,
                        ReceiverWalletId = walletDom4.Wallet.WalletId,
                        Amount = 150
                    },
                    new ()
                    {
                        SenderWalletId = walletDom4.Wallet.WalletId,
                        ReceiverWalletId = walletDom5.Wallet.WalletId,
                        Amount = 180
                    }
                }
        };

        try
        {
            // create order
            await TestInit1.OrdersClient.CreateOrderAsync(TestInit1.AppId, request);
            Assert.Fail("Insufficient Balance exception is expected.");
        }
        catch (ApiException ex)
        {
            Assert.AreEqual(nameof(InsufficientBalanceException), ex.ExceptionTypeName);
        }
    }

    [TestMethod]
    public async Task InsufficientBalance_Sale()
    {
        // Create wallets
        var systemWalletDom = await WalletDom.Create(TestInit1);
        var walletDom1 = await WalletDom.Create(TestInit1);
        var walletDom2 = await WalletDom.Create(TestInit1);
        var walletDom3 = await WalletDom.Create(TestInit1);
        var walletDom4 = await WalletDom.Create(TestInit1);
        var walletDom5 = await WalletDom.Create(TestInit1);
        var walletDom6 = await WalletDom.Create(TestInit1);

        // Transfer to wallets in order to make available balances based on document
        await systemWalletDom.CreateOrder(TestInit1, receiverWalletId: walletDom1.Wallet.WalletId, amount: 100, transactionType: TransactionType.Sale);
        await systemWalletDom.CreateOrder(TestInit1, receiverWalletId: walletDom2.Wallet.WalletId, amount: 150, transactionType: TransactionType.Sale);
        await systemWalletDom.CreateOrder(TestInit1, receiverWalletId: walletDom4.Wallet.WalletId, amount: 20, transactionType: TransactionType.Sale);
        await systemWalletDom.CreateOrder(TestInit1, receiverWalletId: walletDom5.Wallet.WalletId, amount: 40, transactionType: TransactionType.Sale);

        // Transfer to wallets in order to make pending balances based on document
        await systemWalletDom.CreateOrder(TestInit1, receiverWalletId: walletDom1.Wallet.WalletId, amount: 50, transactionType: TransactionType.Authorize);
        await systemWalletDom.CreateOrder(TestInit1, receiverWalletId: walletDom2.Wallet.WalletId, amount: 70, transactionType: TransactionType.Authorize);
        await systemWalletDom.CreateOrder(TestInit1, receiverWalletId: walletDom3.Wallet.WalletId, amount: 90, transactionType: TransactionType.Authorize);
        await systemWalletDom.CreateOrder(TestInit1, receiverWalletId: walletDom4.Wallet.WalletId, amount: 10, transactionType: TransactionType.Authorize);

        // Create order request
        var orderTypeId = new Random().Next(int.MinValue, int.MaxValue);
        var request = new CreateOrderRequest
        {
            OrderTypeId = orderTypeId,
            OrderId = Guid.NewGuid(),
            CurrencyId = systemWalletDom.CurrencyId,
            TransactionType = TransactionType.Sale,
            ParticipantWallets = new List<ParticipantTransferItem>
                {
                    new ()
                    {
                        SenderWalletId = walletDom1.Wallet.WalletId,
                        ReceiverWalletId = walletDom5.Wallet.WalletId,
                        Amount = 100
                    },
                    new ()
                    {
                        SenderWalletId = walletDom2.Wallet.WalletId,
                        ReceiverWalletId = walletDom3.Wallet.WalletId,
                        Amount = 150
                    },
                    new ()
                    {
                        SenderWalletId = walletDom3.Wallet.WalletId,
                        ReceiverWalletId = walletDom4.Wallet.WalletId,
                        Amount = 150
                    },
                    new ()
                    {
                        SenderWalletId = walletDom4.Wallet.WalletId,
                        ReceiverWalletId = walletDom5.Wallet.WalletId,
                        Amount = 180
                    },
                    new ()
                    {
                        SenderWalletId = walletDom5.Wallet.WalletId,
                        ReceiverWalletId = walletDom6.Wallet.WalletId,
                        Amount = 20
                    },
                    new ()
                    {
                        SenderWalletId = walletDom5.Wallet.WalletId,
                        ReceiverWalletId = walletDom2.Wallet.WalletId,
                        Amount = 40
                    }
                }
        };

        try
        {
            // create order
            await TestInit1.OrdersClient.CreateOrderAsync(TestInit1.AppId, request);
            Assert.Fail("Insufficient Balance exception is expected.");
        }
        catch (ApiException ex)
        {
            Assert.AreEqual(nameof(InsufficientBalanceException), ex.ExceptionTypeName);
        }
    }

    [TestMethod]
    public async Task Capture()
    {
        // Create wallets
        var systemWalletDom = await WalletDom.Create(TestInit1);
        var walletDom1 = await WalletDom.Create(TestInit1);
        var walletDom2 = await WalletDom.Create(TestInit1);
        var walletDom3 = await WalletDom.Create(TestInit1);
        var walletDom4 = await WalletDom.Create(TestInit1);
        var walletDom5 = await WalletDom.Create(TestInit1);
        var walletDom6 = await WalletDom.Create(TestInit1);

        // Transfer to wallets in order to make available balances based on document
        await systemWalletDom.CreateOrder(TestInit1, receiverWalletId: walletDom1.Wallet.WalletId, amount: 100, transactionType: TransactionType.Sale);
        await systemWalletDom.CreateOrder(TestInit1, receiverWalletId: walletDom2.Wallet.WalletId, amount: 150, transactionType: TransactionType.Sale);
        await systemWalletDom.CreateOrder(TestInit1, receiverWalletId: walletDom4.Wallet.WalletId, amount: 20, transactionType: TransactionType.Sale);
        await systemWalletDom.CreateOrder(TestInit1, receiverWalletId: walletDom5.Wallet.WalletId, amount: 40, transactionType: TransactionType.Sale);

        // Transfer to wallets in order to make pending balances based on document
        await systemWalletDom.CreateOrder(TestInit1, receiverWalletId: walletDom1.Wallet.WalletId, amount: 50, transactionType: TransactionType.Authorize);
        await systemWalletDom.CreateOrder(TestInit1, receiverWalletId: walletDom2.Wallet.WalletId, amount: 70, transactionType: TransactionType.Authorize);
        await systemWalletDom.CreateOrder(TestInit1, receiverWalletId: walletDom3.Wallet.WalletId, amount: 90, transactionType: TransactionType.Authorize);
        await systemWalletDom.CreateOrder(TestInit1, receiverWalletId: walletDom4.Wallet.WalletId, amount: 10, transactionType: TransactionType.Authorize);

        // Create order request
        var orderTypeId = new Random().Next(int.MinValue, int.MaxValue);
        var request = new CreateOrderRequest
        {
            CurrencyId = systemWalletDom.CurrencyId,
            OrderTypeId = orderTypeId,
            TransactionType = TransactionType.Authorize,
            ParticipantWallets = new List<ParticipantTransferItem>
                {
                    new ()
                    {
                        SenderWalletId = walletDom1.Wallet.WalletId,
                        ReceiverWalletId = walletDom2.Wallet.WalletId,
                        Amount = 100
                    },
                    new ()
                    {
                        SenderWalletId = walletDom2.Wallet.WalletId,
                        ReceiverWalletId = walletDom3.Wallet.WalletId,
                        Amount = 150
                    },
                    new ()
                    {
                        SenderWalletId = walletDom3.Wallet.WalletId,
                        ReceiverWalletId = walletDom4.Wallet.WalletId,
                        Amount = 150
                    },
                    new ()
                    {
                        SenderWalletId = walletDom4.Wallet.WalletId,
                        ReceiverWalletId = walletDom5.Wallet.WalletId,
                        Amount = 30
                    },
                    new ()
                    {
                        SenderWalletId = walletDom5.Wallet.WalletId,
                        ReceiverWalletId = walletDom6.Wallet.WalletId,
                        Amount = 20
                    },
                    new ()
                    {
                        SenderWalletId = walletDom5.Wallet.WalletId,
                        ReceiverWalletId = walletDom2.Wallet.WalletId,
                        Amount = 40
                    }
                }
        };

        // create order
        var order = await TestInit1.OrdersClient.CreateOrderAsync(TestInit1.AppId, request);

        // capture the order
        await TestInit1.OrdersClient.CaptureAsync(TestInit1.AppId, order.OrderId);

        // Get wallets after transfer
        var wallet1 = await TestInit1.WalletsClient.GetWalletAsync(TestInit1.AppId, walletDom1.Wallet.WalletId);
        var wallet2 = await TestInit1.WalletsClient.GetWalletAsync(TestInit1.AppId, walletDom2.Wallet.WalletId);
        var wallet3 = await TestInit1.WalletsClient.GetWalletAsync(TestInit1.AppId, walletDom3.Wallet.WalletId);
        var wallet4 = await TestInit1.WalletsClient.GetWalletAsync(TestInit1.AppId, walletDom4.Wallet.WalletId);
        var wallet5 = await TestInit1.WalletsClient.GetWalletAsync(TestInit1.AppId, walletDom5.Wallet.WalletId);
        var wallet6 = await TestInit1.WalletsClient.GetWalletAsync(TestInit1.AppId, walletDom6.Wallet.WalletId);

        // Available balance of wallets after transfer
        ArgumentNullException.ThrowIfNull(wallet1.Currencies);
        ArgumentNullException.ThrowIfNull(wallet2.Currencies);
        ArgumentNullException.ThrowIfNull(wallet3.Currencies);
        ArgumentNullException.ThrowIfNull(wallet4.Currencies);
        ArgumentNullException.ThrowIfNull(wallet5.Currencies);
        ArgumentNullException.ThrowIfNull(wallet6.Currencies);
        var wallet1AvailableBalance = wallet1.Currencies.Single(c => c.CurrencyId == systemWalletDom.CurrencyId).Balance;
        var wallet2AvailableBalance = wallet2.Currencies.Single(c => c.CurrencyId == systemWalletDom.CurrencyId).Balance;
        var wallet3AvailableBalance = wallet3.Currencies.Single(c => c.CurrencyId == systemWalletDom.CurrencyId).Balance;
        var wallet4AvailableBalance = wallet4.Currencies.Single(c => c.CurrencyId == systemWalletDom.CurrencyId).Balance;
        var wallet5AvailableBalance = wallet5.Currencies.Single(c => c.CurrencyId == systemWalletDom.CurrencyId).Balance;
        var wallet6AvailableBalance = wallet6.Currencies.Single(c => c.CurrencyId == systemWalletDom.CurrencyId).Balance;

        // Assert
        Assert.AreEqual(0, wallet1AvailableBalance);
        Assert.AreEqual(140, wallet2AvailableBalance);
        Assert.AreEqual(0, wallet3AvailableBalance);
        Assert.AreEqual(140, wallet4AvailableBalance);
        Assert.AreEqual(10, wallet5AvailableBalance);
        Assert.AreEqual(20, wallet6AvailableBalance);
    }

    [TestMethod]
    public async Task Create_an_order_with_Wallet_Idempotent_Exception()
    {
        // create wallet1
        var systemWalletDom = await WalletDom.Create(TestInit1);
        var walletDom1 = await WalletDom.Create(TestInit1);
        await systemWalletDom.CreateOrder(TestInit1, receiverWalletId: walletDom1.Wallet.WalletId, amount: 1000, transactionType: TransactionType.Sale);

        var walletDom2 = await WalletDom.Create(TestInit1);
        var walletDom3 = await WalletDom.Create(TestInit1);

        // Create order request
        var orderTypeId = new Random().Next(int.MinValue, int.MaxValue);
        var request = new CreateOrderRequest
        {
            OrderId = Guid.NewGuid(),
            CurrencyId = systemWalletDom.CurrencyId,
            OrderTypeId = orderTypeId,
            TransactionType = TransactionType.Authorize,
            ParticipantWallets = new List<ParticipantTransferItem>
            {
                new ()
                {
                    SenderWalletId = walletDom1.Wallet.WalletId,
                    ReceiverWalletId = walletDom2.Wallet.WalletId,
                    Amount = 100
                },
                new ()
                {
                    SenderWalletId = walletDom2.Wallet.WalletId,
                    ReceiverWalletId = walletDom3.Wallet.WalletId,
                    Amount = 100
                }
            }
        };
        var order = await TestInit1.OrdersClient.CreateOrderAsync(TestInit1.AppId, request);

        var idempotentOrder = await TestInit1.OrdersClient.CreateOrderAsync(TestInit1.AppId, request);
        Assert.AreEqual(order.OrderId, idempotentOrder.OrderId);
    }

    [TestMethod]
    public async Task Create_an_order_with_sequence()
    {
        // create wallet on app1
        var walletDom = await WalletDom.Create(TestInit1);
        var walletDom2 = await WalletDom.Create(TestInit1, currencyId: walletDom.CurrencyId);
        var walletDom3 = await WalletDom.Create(TestInit1, currencyId: walletDom.CurrencyId);
        var walletDom4 = await WalletDom.Create(TestInit1, currencyId: walletDom.CurrencyId);

        // create order
        var request = new CreateOrderRequest
        {
            CurrencyId = walletDom.CurrencyId,
            OrderId = Guid.NewGuid(),
            TransactionType = TransactionType.Authorize,
            ParticipantWallets = new List<ParticipantTransferItem>
            {
                new ()
                {
                    SenderWalletId = walletDom.Wallet.WalletId,
                    ReceiverWalletId = walletDom2.Wallet.WalletId,
                    Amount = 100
                },
                new ()
                {
                    SenderWalletId = walletDom3.Wallet.WalletId,
                    ReceiverWalletId = walletDom4.Wallet.WalletId,
                    Amount = 100
                }
            }
        };

        var senderWalletBefore1 = await TestInit1.WalletsClient.GetWalletAsync(TestInit1.AppId, walletDom.Wallet.WalletId);
        var senderWalletBefore2 = await TestInit1.WalletsClient.GetWalletAsync(TestInit1.AppId, walletDom3.Wallet.WalletId);
        var receiverWalletBefore1 = await TestInit1.WalletsClient.GetWalletAsync(TestInit1.AppId, walletDom2.Wallet.WalletId);
        var receiverWalletBefore2 = await TestInit1.WalletsClient.GetWalletAsync(TestInit1.AppId, walletDom4.Wallet.WalletId);

        ArgumentNullException.ThrowIfNull(senderWalletBefore1.Currencies);
        ArgumentNullException.ThrowIfNull(senderWalletBefore2.Currencies);
        ArgumentNullException.ThrowIfNull(receiverWalletBefore1.Currencies);
        ArgumentNullException.ThrowIfNull(receiverWalletBefore2.Currencies);

        // create order
        var orderCreated = await TestInit1.OrdersClient.CreateOrderAsync(TestInit1.AppId, request);
        var order = await TestInit1.OrdersClient.GetOrderAsync(TestInit1.AppId, orderCreated.OrderId);

        // validate general properties
        Assert.IsNull(order.CapturedTime);
        Assert.IsNull(order.VoidedTime);
        Assert.AreEqual(OrderStatus.Authorized, order.Status);

        var senderWalletAfter1 = await TestInit1.WalletsClient.GetWalletAsync(TestInit1.AppId, walletDom.Wallet.WalletId);
        var senderWalletAfter2 = await TestInit1.WalletsClient.GetWalletAsync(TestInit1.AppId, walletDom3.Wallet.WalletId);
        var receiverWalletAfter1 = await TestInit1.WalletsClient.GetWalletAsync(TestInit1.AppId, walletDom2.Wallet.WalletId);
        var receiverWalletAfter2 = await TestInit1.WalletsClient.GetWalletAsync(TestInit1.AppId, walletDom4.Wallet.WalletId);

        ArgumentNullException.ThrowIfNull(senderWalletAfter1.Currencies);
        ArgumentNullException.ThrowIfNull(senderWalletAfter2.Currencies);
        ArgumentNullException.ThrowIfNull(receiverWalletAfter1.Currencies);
        ArgumentNullException.ThrowIfNull(receiverWalletAfter2.Currencies);

        // validate receiver balance
        Assert.AreEqual(receiverWalletBefore1.Currencies.SingleOrDefault(x => x.CurrencyId == walletDom.CurrencyId)?.Balance ?? 0,
            receiverWalletAfter1.Currencies.SingleOrDefault(x => x.CurrencyId == walletDom.CurrencyId)?.Balance ?? 0);

        Assert.AreEqual(receiverWalletBefore2.Currencies.SingleOrDefault(x => x.CurrencyId == walletDom.CurrencyId)?.Balance ?? 0,
            receiverWalletAfter2.Currencies.SingleOrDefault(x => x.CurrencyId == walletDom.CurrencyId)?.Balance ?? 0);
    }

    [TestMethod]
    public async Task Create_an_order_withOut_sequence()
    {
        // create wallet on app1
        var walletDom = await WalletDom.Create(TestInit1);
        var walletDom2 = await WalletDom.Create(TestInit1, currencyId: walletDom.CurrencyId);
        var walletDom3 = await WalletDom.Create(TestInit1, currencyId: walletDom.CurrencyId);

        // create order
        var request = new CreateOrderRequest
        {
            CurrencyId = walletDom.CurrencyId,
            OrderId = Guid.NewGuid(),
            TransactionType = TransactionType.Authorize,
            ParticipantWallets = new List<ParticipantTransferItem>
            {
                new ()
                {
                    SenderWalletId = walletDom.Wallet.WalletId,
                    ReceiverWalletId = walletDom2.Wallet.WalletId,
                    Amount = 100
                },
                new ()
                {
                    SenderWalletId = walletDom.Wallet.WalletId,
                    ReceiverWalletId = walletDom3.Wallet.WalletId,
                    Amount = 100
                }
            }
        };

        var senderWalletBefore = await TestInit1.WalletsClient.GetWalletAsync(TestInit1.AppId, walletDom.Wallet.WalletId);
        var receiverWalletBefore = await TestInit1.WalletsClient.GetWalletAsync(TestInit1.AppId, walletDom2.Wallet.WalletId);
        var receiverWalletBefore2 = await TestInit1.WalletsClient.GetWalletAsync(TestInit1.AppId, walletDom3.Wallet.WalletId);
        ArgumentNullException.ThrowIfNull(senderWalletBefore.Currencies);
        ArgumentNullException.ThrowIfNull(receiverWalletBefore.Currencies);
        ArgumentNullException.ThrowIfNull(receiverWalletBefore2.Currencies);

        var orderCreated = await TestInit1.OrdersClient.CreateOrderAsync(TestInit1.AppId, request);
        var order = await TestInit1.OrdersClient.GetOrderAsync(TestInit1.AppId, orderCreated.OrderId);

        // validate general properties
        Assert.IsNull(order.CapturedTime);
        Assert.IsNull(order.VoidedTime);
        Assert.AreEqual(OrderStatus.Authorized, order.Status);

        var senderWalletAfter = await TestInit1.WalletsClient.GetWalletAsync(TestInit1.AppId, walletDom.Wallet.WalletId);
        var receiverWalletAfter = await TestInit1.WalletsClient.GetWalletAsync(TestInit1.AppId, walletDom2.Wallet.WalletId);
        var receiverWalletAfter2 = await TestInit1.WalletsClient.GetWalletAsync(TestInit1.AppId, walletDom3.Wallet.WalletId);
        ArgumentNullException.ThrowIfNull(senderWalletAfter.Currencies);
        ArgumentNullException.ThrowIfNull(receiverWalletAfter.Currencies);
        ArgumentNullException.ThrowIfNull(receiverWalletAfter2.Currencies);

        // validate receiver balance
        Assert.AreEqual(receiverWalletAfter.Currencies.SingleOrDefault(x => x.CurrencyId == walletDom.CurrencyId)?.Balance ?? 0,
            receiverWalletBefore.Currencies.SingleOrDefault(x => x.CurrencyId == walletDom.CurrencyId)?.Balance ?? 0);

        Assert.AreEqual(receiverWalletAfter2.Currencies.SingleOrDefault(x => x.CurrencyId == walletDom.CurrencyId)?.Balance ?? 0,
            receiverWalletBefore2.Currencies.SingleOrDefault(x => x.CurrencyId == walletDom.CurrencyId)?.Balance ?? 0);

    }

    [TestMethod]
    public async Task Success_transfer_when_receiverWallets_have_not_balance()
    {
        // Create wallets
        var systemWalletDom = await WalletDom.Create(TestInit1);
        var walletDom1 = await WalletDom.Create(TestInit1, currencyId: systemWalletDom.CurrencyId);
        var walletDom2 = await WalletDom.Create(TestInit1, currencyId: systemWalletDom.CurrencyId);
        var walletDom3 = await WalletDom.Create(TestInit1, currencyId: systemWalletDom.CurrencyId);
        var walletDom4 = await WalletDom.Create(TestInit1, currencyId: systemWalletDom.CurrencyId);

        // increase balance of wallet1 and wallet3
        await systemWalletDom.CreateOrder(TestInit1, receiverWalletId: walletDom1.Wallet.WalletId, amount: 100, transactionType: TransactionType.Sale);
        await systemWalletDom.CreateOrder(TestInit1, receiverWalletId: walletDom3.Wallet.WalletId, amount: 150, transactionType: TransactionType.Sale);

        // Get wallets before transfer
        var walletBefore1 = await TestInit1.WalletsClient.GetWalletAsync(TestInit1.AppId, walletDom1.Wallet.WalletId);
        var walletBefore2 = await TestInit1.WalletsClient.GetWalletAsync(TestInit1.AppId, walletDom2.Wallet.WalletId);
        var walletBefore3 = await TestInit1.WalletsClient.GetWalletAsync(TestInit1.AppId, walletDom3.Wallet.WalletId);
        var walletBefore4 = await TestInit1.WalletsClient.GetWalletAsync(TestInit1.AppId, walletDom4.Wallet.WalletId);

        // Available balance of wallets before transfer
        ArgumentNullException.ThrowIfNull(walletBefore1.Currencies);
        ArgumentNullException.ThrowIfNull(walletBefore2.Currencies);
        ArgumentNullException.ThrowIfNull(walletBefore3.Currencies);
        ArgumentNullException.ThrowIfNull(walletBefore4.Currencies);

        var wallet1Balance = walletBefore1.Currencies.Single(c => c.CurrencyId == systemWalletDom.CurrencyId).Balance;
        var wallet2Balance = walletBefore2.Currencies.Single(c => c.CurrencyId == systemWalletDom.CurrencyId).Balance;
        var wallet3Balance = walletBefore3.Currencies.Single(c => c.CurrencyId == systemWalletDom.CurrencyId).Balance;
        var wallet4Balance = walletBefore4.Currencies.Single(c => c.CurrencyId == systemWalletDom.CurrencyId).Balance;

        // Create order request
        var orderTypeId = new Random().Next(int.MinValue, int.MaxValue);
        var request = new CreateOrderRequest
        {
            OrderId = Guid.NewGuid(),
            CurrencyId = systemWalletDom.CurrencyId,
            OrderTypeId = orderTypeId,
            TransactionType = TransactionType.Authorize,
            ParticipantWallets = new List<ParticipantTransferItem>
            {
                new ()
                {
                    SenderWalletId = walletDom1.Wallet.WalletId,
                    ReceiverWalletId = walletDom2.Wallet.WalletId,
                    Amount = 10
                },
                new ()
                {
                    SenderWalletId = walletDom3.Wallet.WalletId,
                    ReceiverWalletId = walletDom4.Wallet.WalletId,
                    Amount = 15
                }
            }
        };

        // create order
        var orderCreated = await TestInit1.OrdersClient.CreateOrderAsync(TestInit1.AppId, request);
        var order = await TestInit1.OrdersClient.GetOrderAsync(TestInit1.AppId, orderCreated.OrderId);

        // validate general properties
        Assert.IsNull(order.CapturedTime);
        Assert.IsNull(order.VoidedTime);
        Assert.AreEqual(OrderStatus.Authorized, order.Status);

        // Get wallets after transfer
        var walletAfter1 = await TestInit1.WalletsClient.GetWalletAsync(TestInit1.AppId, walletDom1.Wallet.WalletId);
        var walletAfter2 = await TestInit1.WalletsClient.GetWalletAsync(TestInit1.AppId, walletDom2.Wallet.WalletId);
        var walletAfter3 = await TestInit1.WalletsClient.GetWalletAsync(TestInit1.AppId, walletDom3.Wallet.WalletId);
        var walletAfter4 = await TestInit1.WalletsClient.GetWalletAsync(TestInit1.AppId, walletDom4.Wallet.WalletId);

        // Available balance of wallets after transfer
        ArgumentNullException.ThrowIfNull(walletAfter1.Currencies);
        ArgumentNullException.ThrowIfNull(walletAfter2.Currencies);
        ArgumentNullException.ThrowIfNull(walletAfter3.Currencies);
        ArgumentNullException.ThrowIfNull(walletAfter4.Currencies);

        var wallet1AvailableBalance = walletAfter1.Currencies.Single(c => c.CurrencyId == systemWalletDom.CurrencyId).Balance;
        var wallet2AvailableBalance = walletAfter2.Currencies.Single(c => c.CurrencyId == systemWalletDom.CurrencyId).Balance;
        var wallet3AvailableBalance = walletAfter3.Currencies.Single(c => c.CurrencyId == systemWalletDom.CurrencyId).Balance;
        var wallet4AvailableBalance = walletAfter4.Currencies.Single(c => c.CurrencyId == systemWalletDom.CurrencyId).Balance;

        // Assert
        Assert.AreEqual(wallet1Balance - 10, wallet1AvailableBalance);
        Assert.AreEqual(wallet2Balance, wallet2AvailableBalance);
        Assert.AreEqual(wallet3Balance - 15, wallet3AvailableBalance);
        Assert.AreEqual(wallet4Balance, wallet4AvailableBalance);
    }

    [TestMethod]
    public async Task All_transfer_should_not_proceed_if_each_transfer_fail()
    {
        // Create wallets
        var systemWalletDom = await WalletDom.Create(TestInit1);
        var walletDom1 = await WalletDom.Create(TestInit1, currencyId: systemWalletDom.CurrencyId);
        var walletDom2 = await WalletDom.Create(TestInit1, currencyId: systemWalletDom.CurrencyId);
        var walletDom3 = await WalletDom.Create(TestInit1);
        var walletDom4 = await WalletDom.Create(TestInit1, currencyId: systemWalletDom.CurrencyId);

        // increase balance of wallet1
        await systemWalletDom.CreateOrder(TestInit1, receiverWalletId: walletDom1.Wallet.WalletId, amount: 100, transactionType: TransactionType.Sale);

        // create order
        var request = new CreateOrderRequest
        {
            CurrencyId = systemWalletDom.CurrencyId,
            OrderId = Guid.NewGuid(),
            TransactionType = TransactionType.Authorize,
            AllowPartialSuccess = false,
            ParticipantWallets = new List<ParticipantTransferItem>
            {
                new ()
                {
                    SenderWalletId = walletDom1.Wallet.WalletId,
                    ReceiverWalletId = walletDom2.Wallet.WalletId,
                    Amount = 10
                },
                new ()
                {
                    SenderWalletId = walletDom3.Wallet.WalletId,
                    ReceiverWalletId = walletDom4.Wallet.WalletId,
                    Amount = 20
                }
            }
        };

        var walletBefore1 = await TestInit1.WalletsClient.GetWalletAsync(TestInit1.AppId, walletDom1.Wallet.WalletId);
        var walletBefore2 = await TestInit1.WalletsClient.GetWalletAsync(TestInit1.AppId, walletDom2.Wallet.WalletId);
        var walletBefore3 = await TestInit1.WalletsClient.GetWalletAsync(TestInit1.AppId, walletDom3.Wallet.WalletId);
        var walletBefore4 = await TestInit1.WalletsClient.GetWalletAsync(TestInit1.AppId, walletDom4.Wallet.WalletId);

        ArgumentNullException.ThrowIfNull(walletBefore1.Currencies);
        ArgumentNullException.ThrowIfNull(walletBefore2.Currencies);
        ArgumentNullException.ThrowIfNull(walletBefore3.Currencies);
        ArgumentNullException.ThrowIfNull(walletBefore4.Currencies);

        var wallet1Balance = walletBefore1.Currencies.Single(c => c.CurrencyId == systemWalletDom.CurrencyId).Balance;

        // create order
        try
        {
            await TestInit1.OrdersClient.CreateOrderAsync(TestInit1.AppId, request);

            Assert.Fail("InsufficientBalance exception is expected.");
        }
        catch (ApiException ex)
        {
            var walletAfter1 = await TestInit1.WalletsClient.GetWalletAsync(TestInit1.AppId, walletDom1.Wallet.WalletId);
            ArgumentNullException.ThrowIfNull(walletAfter1.Currencies);
            var wallet1AvailableBalance = walletAfter1.Currencies.Single(c => c.CurrencyId == systemWalletDom.CurrencyId).Balance;

            // Assert
            Assert.AreEqual(wallet1Balance, wallet1AvailableBalance);
            Assert.AreEqual(nameof(InsufficientBalanceException), ex.ExceptionTypeName);
        }
    }

    [TestMethod]
    public async Task Should_process_all_transfers_even_if_one_fail()
    {
        // Create wallets
        var systemWalletDom = await WalletDom.Create(TestInit1);
        var walletDom1 = await WalletDom.Create(TestInit1, currencyId: systemWalletDom.CurrencyId);
        var walletDom2 = await WalletDom.Create(TestInit1, currencyId: systemWalletDom.CurrencyId);
        var walletDom3 = await WalletDom.Create(TestInit1, currencyId: systemWalletDom.CurrencyId);
        var walletDom4 = await WalletDom.Create(TestInit1, currencyId: systemWalletDom.CurrencyId);

        // increase balance of wallet1 and wallet4
        await systemWalletDom.CreateOrder(TestInit1, receiverWalletId: walletDom1.Wallet.WalletId, amount: 100, transactionType: TransactionType.Sale);
        await systemWalletDom.CreateOrder(TestInit1, receiverWalletId: walletDom4.Wallet.WalletId, amount: 100, transactionType: TransactionType.Sale);

        // create order
        var request = new CreateOrderRequest
        {
            CurrencyId = systemWalletDom.CurrencyId,
            OrderId = Guid.NewGuid(),
            TransactionType = TransactionType.Authorize,
            AllowPartialSuccess = true,
            ParticipantWallets = new List<ParticipantTransferItem>
            {
                new ()
                {
                    SenderWalletId = walletDom1.Wallet.WalletId,
                    ReceiverWalletId = walletDom2.Wallet.WalletId,
                    Amount = 10
                },
                new ()
                {
                    SenderWalletId = walletDom3.Wallet.WalletId,
                    ReceiverWalletId = walletDom4.Wallet.WalletId,
                    Amount = 20
                },
                new ()
                {
                    SenderWalletId = walletDom4.Wallet.WalletId,
                    ReceiverWalletId = walletDom1.Wallet.WalletId,
                    Amount = 5
                }
            }
        };

        var walletBefore1 = await TestInit1.WalletsClient.GetWalletAsync(TestInit1.AppId, walletDom1.Wallet.WalletId);
        var walletBefore2 = await TestInit1.WalletsClient.GetWalletAsync(TestInit1.AppId, walletDom2.Wallet.WalletId);
        var walletBefore3 = await TestInit1.WalletsClient.GetWalletAsync(TestInit1.AppId, walletDom3.Wallet.WalletId);
        var walletBefore4 = await TestInit1.WalletsClient.GetWalletAsync(TestInit1.AppId, walletDom4.Wallet.WalletId);

        ArgumentNullException.ThrowIfNull(walletBefore1.Currencies);
        ArgumentNullException.ThrowIfNull(walletBefore2.Currencies);
        ArgumentNullException.ThrowIfNull(walletBefore3.Currencies);
        ArgumentNullException.ThrowIfNull(walletBefore4.Currencies);

        var wallet1Balance = walletBefore1.Currencies.Single(c => c.CurrencyId == systemWalletDom.CurrencyId).Balance;
        var wallet3Balance = walletBefore3.Currencies.Single(c => c.CurrencyId == systemWalletDom.CurrencyId).Balance;
        var wallet4Balance = walletBefore4.Currencies.Single(c => c.CurrencyId == systemWalletDom.CurrencyId).Balance;

        // create order
        await TestInit1.OrdersClient.CreateOrderAsync(TestInit1.AppId, request);

        var walletAfter1 = await TestInit1.WalletsClient.GetWalletAsync(TestInit1.AppId, walletDom1.Wallet.WalletId);
        var walletAfter3 = await TestInit1.WalletsClient.GetWalletAsync(TestInit1.AppId, walletDom3.Wallet.WalletId);
        var walletAfter4 = await TestInit1.WalletsClient.GetWalletAsync(TestInit1.AppId, walletDom4.Wallet.WalletId);
        ArgumentNullException.ThrowIfNull(walletAfter1.Currencies);
        ArgumentNullException.ThrowIfNull(walletAfter3.Currencies);
        ArgumentNullException.ThrowIfNull(walletAfter4.Currencies);

        var wallet1AvailableBalance = walletAfter1.Currencies.Single(c => c.CurrencyId == systemWalletDom.CurrencyId).Balance;
        var wallet3AvailableBalance = walletAfter3.Currencies.Single(c => c.CurrencyId == systemWalletDom.CurrencyId).Balance;
        var wallet4AvailableBalance = walletAfter4.Currencies.Single(c => c.CurrencyId == systemWalletDom.CurrencyId).Balance;

        // Assert
        Assert.AreEqual(wallet1Balance - 10, wallet1AvailableBalance);
        Assert.AreEqual(wallet3Balance, wallet3AvailableBalance);
        Assert.AreEqual(wallet4Balance - 5, wallet4AvailableBalance);
    }
}