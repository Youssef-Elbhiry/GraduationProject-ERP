using System.Text.Json.Serialization;

namespace ERP.Domain.Constants;

public enum SalesOrderStatus { Draft, Sent, Confirmed, Rejected, Cancelled }
public enum FulfillmentStatus { Waiting, Ready, Done, Cancelled }
public enum ProductionStatus { Pending, Completed, Cancelled }
public enum InvoiceStatus { Draft, Issued }
public enum PaymentMethod { Cash, BankTransfer }
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ProductMovementType { StockIn, Adjustment, Shipment, ProductionOutput }
public enum RawMaterialMovementType { StockIn, Adjustment, ProductionConsume }
