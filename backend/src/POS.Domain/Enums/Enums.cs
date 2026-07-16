namespace POS.Domain.Enums;

public enum UserRole
{
    Owner,
    Manager,
    Cashier
}

public enum OrderStatus
{
    Open,
    Completed,
    Voided,
    Refunded,
    PartiallyRefunded
}

public enum PaymentMethod
{
    Cash,
    CardPresent,
    CardNotPresent,
    ApplePay,
    GooglePay,
    QrBankTransfer,
    BuyNowPayLater,
    GiftCard,
    StoreCredit,
    Crypto
}

public enum PaymentStatus
{
    Pending,
    Authorized,
    Captured,
    Failed,
    Refunded,
    PartiallyRefunded
}
