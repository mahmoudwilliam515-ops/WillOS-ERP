namespace EnterpriseERP.Domain.Entities.Sales;

public enum SalesOrderStatus
{
    Draft = 0,               // مسودة
    PendingApproval = 1,      // قيد الموافقة
    Approved = 2,            // موافق عليه (إدارياً)
    CreditCheckHold = 3,     // متوقف للفحص الائتماني
    Confirmed = 4,           // مؤكد (جاهز للتنفيذ)
    ReleasedToWarehouse = 5,  // تم إرساله للمخزن
    PartiallyShipped = 6,     // مشحون جزئياً
    Shipped = 7,             // مشحون بالكامل
    PartiallyDelivered = 8,   // تم التسليم جزئياً
    Delivered = 9,           // تم التسليم بالكامل
    Invoiced = 10,           // مفوتر
    PartiallyPaid = 11,      // مدفوع جزئياً
    Paid = 12,               // مدفوع بالكامل
    Cancelled = 13,          // ملغى
    Closed = 14              // مغلق (مكتمل نهائياً)
}
