using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using User.ApplicationDbContext;
using User.DTO;
using User.Model;
using User.Service;

namespace User.Controllers
{
    [Route("api/")]
    [ApiController]
    public class Payment : ControllerBase
    {
        private readonly PayPalService _payPalService;
        private readonly DB _db;

        public Payment(PayPalService payPalService,DB dB)
        {
            _payPalService = payPalService;
            _db = dB;
        }

        [Authorize(Roles = "User,Broker,Company")]
        [HttpPost("Payment")]
        public async Task<IActionResult> PaymentMethod(PaymentDTO payment)
        {
            if (payment.Amount>= 1)
            {
                var result = await _payPalService.CreateOrder(payment.Amount.Value);
                return Ok(result);
            }
            return BadRequest(new { message = "المبلغ يجب ان يكون اكبر من 1" });
        }

        [Authorize(Roles = "User,Broker,Company")]
        [HttpPost("capture-order")]
        public async Task<IActionResult> CaptureOrder(PaymentDTO payment)
        {
            if (payment.OrderId == null)
            {
                return BadRequest(new ApiResponse { Message = "رقم العملية غير موجود" });
            }
            var ID = User.FindFirstValue("ID");
            var Details = await _payPalService.CaptureOrder(payment.OrderId!);
            var save = _db.paymentDetails.FirstOrDefault();
            dynamic transaction = Details;
            List<PayemntDetailsDTO> payemntDetailsDTO = new List<PayemntDetailsDTO>();
            if (Details != null)
            {
                var paymentDetails = new PaymentDetails
                {
                    OrderId = transaction.order_id,
                    TransactionId = transaction.transaction_id,
                    Status = transaction.status,
                    Amount = decimal.Parse(transaction.amount),
                    UserId = ID,
                    DateTime = DateTime.Parse(transaction.Date)
                };

                _db.paymentDetails.Add(paymentDetails);
                await _db.SaveChangesAsync();
            }

            return Ok();
        }
    }
}
