using Microsoft.AspNetCore.Mvc;
using dynamicUssdProject.REPO; // Adjust the namespace based on your project structure
using dynamicUssdProject.Models;
using dynamicUssdProject.Models.Request; // Make sure this includes your request models for setting and verifying PINs

namespace dynamicUssdProject.Controllers
{
    [ApiController]
    [Route("api/pin")]
    public class PinManagementController : ControllerBase
    {
        private readonly UserPinRepository _userPinRepository;

        public PinManagementController(UserPinRepository userPinRepository)
        {
            _userPinRepository = userPinRepository;
        }

        [HttpPost("set")]
        public async Task<IActionResult> SetPin([FromBody] SetPinRequest request)
        {
            bool isSuccess = await _userPinRepository.SetPinAsync(request.PhoneNumber, request.Pin);

            if (!isSuccess)
            {
                return BadRequest("Phone number is already registered.");
            }

            return Ok("PIN set successfully.");
        }
        [HttpPost("Register")]
        public async Task<IActionResult> RegisterAccount([FromBody] RegisterAccountRequest request)
        {
            // Call the repository method, which already handles the user existence check
            bool isSuccess = await _userPinRepository.RegisterUserAsync(request.PhoneNumber, request.Pin,request.Balance,request.UserId);

            if (!isSuccess)
            {
                return BadRequest("Phone number is already registered.");
            }

            return Ok("Account set successfully.");
        }

        [HttpPost("verify")]
        public async Task<IActionResult> VerifyPin([FromBody] VerifyPinRequest request)
        {
            bool isValid = await _userPinRepository.VerifyPinAsync(request.PhoneNumber, request.Pin);
            return isValid ? Ok("PIN verified successfully.") : Unauthorized("Invalid PIN.");
        }

        [HttpPost("update")]
        public async Task<IActionResult> UpdatePin([FromBody] UpdatePinRequest request)
        {
            bool isSuccess = await _userPinRepository.UpdatePinAsync(request.PhoneNumber, request.NewPin);
            return isSuccess ? Ok("PIN updated successfully.") : BadRequest("Failed to update PIN.");
        }
    }
}
