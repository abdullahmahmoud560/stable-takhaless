using Mysqlx.Crud;
using PayPalCheckoutSdk.Core;
using PayPalCheckoutSdk.Orders;

namespace User.Service
{
    public class PayPalService
    {
        private readonly PayPalEnvironment _environment;
        private readonly PayPalHttpClient _client;

        public PayPalService(string clientId, string secret, bool isSandbox)
        {
            _environment = isSandbox
                ? new SandboxEnvironment(clientId, secret)
                : new LiveEnvironment(clientId, secret);

            _client = new PayPalHttpClient(_environment);
        }

        public async Task<object> CreateOrder(decimal amount, string currency = "USD")
        {
            var orderRequest = new OrderRequest()
            {
                CheckoutPaymentIntent = "CAPTURE",
                PurchaseUnits = new List<PurchaseUnitRequest>
        {
            new PurchaseUnitRequest
            {
                AmountWithBreakdown = new AmountWithBreakdown
                {
                    CurrencyCode = currency,
                    Value =amount.ToString("F2")
                }
            }
        }
            };

            var request = new OrdersCreateRequest();
            request.Prefer("return=representation");
            request.RequestBody(orderRequest);

            var response = await _client.Execute(request);
            var result = response.Result<PayPalCheckoutSdk.Orders.Order>();

            // استخراج رابط الموافقة
            var approveLink = result.Links.FirstOrDefault(link => link.Rel == "approve")?.Href;

            return new { orderId = result.Id, approveUrl = approveLink };
        }

        public async Task<object> CaptureOrder(string orderId)
        {
            try
            {
                var request = new OrdersCaptureRequest(orderId);
                request.RequestBody(new OrderActionRequest());

                var response = await _client.Execute(request);
                var result = response.Result<PayPalCheckoutSdk.Orders.Order>();

                if (result == null || result.PurchaseUnits == null)
                {
                    return new { message = "لم يتم العثور على بيانات الطلب." };
                }

                // استخراج بيانات المعاملة
                var transaction = result.PurchaseUnits
                    .SelectMany(unit => unit.Payments?.Captures ?? new List<Capture>())
                    .Select(capture => new
                    {
                        order_id = result.Id,         // ✅ رقم الطلب
                        transaction_id = capture.Id,  // ✅ معرف المعاملة
                        status = capture.Status,      // ✅ حالة المعاملة
                        amount = capture.Amount.Value, // ✅ قيمة المبلغ فقط بدون العملة
                        Date = capture.CreateTime     // ✅ تاريخ ووقت المعاملة
                    })
                    .FirstOrDefault(); // جلب أول معاملة فقط

                return transaction != null ? (dynamic)transaction : new { message = "لم يتم العثور على معاملة لهذا الطلب." };
            }
            catch (Exception ex)
            {
                return new { message = $"حدث خطأ: {ex.Message}" };
            }
        }
    }
}
