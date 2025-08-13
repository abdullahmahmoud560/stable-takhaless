using firstProject.DTO;
using firstProject.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace firstProject.Controllers
{
    [Route("api/")]
    [ApiController]
    public class SelectController : ControllerBase
    {
        private readonly UserManager<User> _userManager;

        public SelectController(UserManager<User> userManager)
        {
            _userManager = userManager;
        }

        //جلب بيانات المستخدم
        [Authorize]
        [HttpPost("Select-Data")]
        public async Task<IActionResult> selectData([FromBody] GetID getID)
        {
            if (getID.ID == null) {
                return BadRequest(new ApiResponse { Message = "برجاء ملئ البيانات المطلوبة" });
            }
            var data = await _userManager.FindByIdAsync(getID.ID!);

            if (data == null)
            {
                return NotFound(new ApiResponse { Message = "بيانات المستخدم غير موجودة" });
            }

            var CustomData = new SelectDTO
            {
                Id = data.Id,
                fullName = data.fullName,
                Email = data.Email,
                PhoneNumber = data.PhoneNumber,
                taxRecord = data.taxRecord,
                InsuranceNumber = data.InsuranceNumber,
                license = data.license,
                Identity = data.Identity,
            };
            return Ok(CustomData);
        }

        //جلب بيانات المستخدمين
        [Authorize]
        [HttpPost("Select-Broker-User")]
        public async Task<IActionResult> selectBrokerUser([FromBody] GetID getID)
        {
            if (string.IsNullOrEmpty(getID.ID) && string.IsNullOrEmpty(getID.BrokerID))
            {
                return BadRequest(new ApiResponse { Message = "برجاء ملء البيانات المطلوبة" });
            }


            var data = await _userManager.FindByIdAsync(getID.ID!);
            var data1 = await _userManager.FindByIdAsync(getID.BrokerID!);


            if (data == null && data1 == null)
            {
                return NotFound(new ApiResponse { Message = "بيانات المستخدم غير موجودة" });
            }

            var response = new
            {
                User = data != null ? new SelectDTO
                {
                    Id = data.Id,
                    fullName = data.fullName,
                    Email = data.Email,
                    PhoneNumber = data.PhoneNumber,
                    taxRecord = data.taxRecord,
                    InsuranceNumber = data.InsuranceNumber,
                    license = data.license,
                    Identity = data.Identity
                } : null,

                Broker = data1 != null ? new SelectDTO
                {
                    Id = data1.Id,
                    fullName = data1.fullName,
                    Email = data1.Email,
                    PhoneNumber = data1.PhoneNumber,
                    taxRecord = data1.taxRecord,
                    InsuranceNumber = data1.InsuranceNumber,
                    license = data1.license,
                    Identity = data1.Identity
                } : null
            };

            return Ok(response);
        }
    }
 }