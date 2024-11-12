using dynamicUssdProject.Models;
using dynamicUssdProject.Models.Request;
using dynamicUssdProject.Models.Response;
using dynamicUssdProject.REPO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace dynamicUssdProject.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UssdController : ControllerBase
    {
        private readonly UserPinRepository _userPinRepository;
        private readonly UserRepository _userRepository;
        private readonly AccountService _accountService;
        private readonly UssdMenuService _ussdMenuService;
        private readonly ApplicationDbContext _context;

        public UssdController(UserPinRepository userPinRepository, ApplicationDbContext context, UssdMenuService ussdMenuService, AccountService accountService, UserRepository userRepository)
        {
            _userPinRepository = userPinRepository;
            _accountService = accountService;
            _ussdMenuService = ussdMenuService;
            _context = context;
            _userRepository = userRepository;
        }

        private async Task<UssdResponse> ExecuteAction(string actionUrl)
        {
            // Based on ActionUrl, you can define specific logic here or call external services
            if (actionUrl.Contains("transferFunds"))
            {
                return new UssdResponse { Message = "Transfer Money action executed.", IsFinal = true };
            }
            else if (actionUrl.Contains("balanceInquiry"))
            {
                return new UssdResponse { Message = "Your balance is $100.", IsFinal = true };
            }

            // Default response for unhandled actions
            return new UssdResponse { Message = "Action completed.", IsFinal = true };
        }

        [HttpGet("options/{id}")]
        public async Task<IActionResult> GetMenuOptionById(int id)
        {
            var option = await _ussdMenuService.GetMenuOptionByIdAsync(id);
            if (option == null)
            {
                return NotFound();
            }

            return Ok(option);
        }

        [HttpGet("options")]
        public async Task<IActionResult> GetAllMenuOptions()
        {
            // Fetch all menu options from the database
            var menuOptions = await _context.Menus.Include(menu => menu.SubMenus).ToListAsync();

            // Return the list of menu options
            return Ok(menuOptions);
        }
    

    [HttpPost("transferFunds/mobileNumber")]
    public async Task<IActionResult> TransferFundsToMobile([FromBody] TransferFundsToMobileRequest request)
    {
        if (!await VerifyPin(request.PhoneNumber, request.Pin))
        {
            return Ok("Invalid PIN. Please try again.");
        }

        var result = await _userRepository.TransferToMobileAsync(request.PhoneNumber, request.RecipientNumber, request.Amount);
        return Ok(result);
    }

    [HttpPost("transferFunds/bankAccount")]
    public async Task<IActionResult> TransferFundsToBank([FromBody] TransferFundsToBankRequest request)
    {
        if (!await VerifyPin(request.PhoneNumber, request.Pin))
        {
            return Ok("Invalid PIN. Please try again.");
        }

        var result = await _userRepository.TransferToBankAsync(request.PhoneNumber, request.BankAccountNumber, request.Amount);
        return Ok(result);
    }

    [HttpPost("transferFunds/pesalink")]
    public async Task<IActionResult> TransferFundsPesalink([FromBody] TransferFundsPesalinkRequest request)
    {
        if (!await VerifyPin(request.PhoneNumber, request.Pin))
        {
            return Ok("Invalid PIN. Please try again.");
        }

        var result = await _userRepository.TransferViaPesalinkAsync(request.PhoneNumber, request.PesalinkId, request.Amount);
        return Ok(result);
    }

    [HttpPost("accountInquiry/balance")]
    public async Task<IActionResult> BalanceInquiry([FromBody] BalanceRequest request)
    {
        if (!await VerifyPin(request.PhoneNumber, request.Pin))
        {
            return Ok("Invalid PIN. Please try again.");
        }

        var balance = await _userRepository.GetBalanceAsync(request.PhoneNumber, request.Pin);
        return Ok($"Your account balance is: ${balance}");
    }
    [HttpPost("deposit")]
    public async Task<IActionResult> Deposit([FromBody] DepositRequest request)
    {
        if (!await VerifyPin(request.PhoneNumber, request.Pin))
        {
            return Ok("Invalid PIN. Please try again.");
        }

        // Call the deposit service method, passing the phone number and amount
        var result = await _userRepository.DepositAsync(request.PhoneNumber, request.Pin, request.Amount);

        // Return the result
        return Ok(result);
    }

    [HttpPost("accountInquiry/miniStatement")]
    public async Task<IActionResult> MiniStatement([FromBody] MiniStatementRequest request)
    {
        if (!await VerifyPin(request.PhoneNumber, request.Pin))
        {
            return Ok("Invalid PIN. Please try again.");
        }

        var miniStatement = await _accountService.GetMiniStatementAsync(request.PhoneNumber);
        return Ok(miniStatement);
    }

    [HttpPost("payBill/paybillNumber")]
    public async Task<IActionResult> PaybillNumber([FromBody] PaybillNumberRequest request)
    {
        if (!await VerifyPin(request.PhoneNumber, request.Pin))
        {
            return Ok("Invalid PIN. Please try again.");
        }

        var result = await _accountService.PaybillAsync(request.PhoneNumber, request.PaybillNumber, request.Amount);
        return Ok(result);
    }

    [HttpPost("payBill/tillNumber")]
    public async Task<IActionResult> TillNumber([FromBody] TillNumberRequest request)
    {
        if (!await VerifyPin(request.PhoneNumber, request.Pin))
        {
            return Ok("Invalid PIN. Please try again.");
        }

        var result = await _accountService.TillAsync(request.PhoneNumber, request.TillNumber, request.Amount);
        return Ok(result);
    }

    [HttpPost("airtimePurchase/myNumber")]
    public async Task<IActionResult> AirtimePurchaseMyNumber([FromBody] AirtimePurchaseMyNumberRequest request)
    {
        if (!await VerifyPin(request.PhoneNumber, request.Pin))
        {
            return Ok("Invalid PIN. Please try again.");
        }

        var result = await _accountService.BuyAirtimeAsync(request.PhoneNumber, request.Amount);
        return Ok(result);
    }

    [HttpPost("airtimePurchase/otherNumber")]
    public async Task<IActionResult> AirtimePurchaseOtherNumber([FromBody] AirtimePurchaseOtherNumberRequest request)
    {
        if (!await VerifyPin(request.PhoneNumber, request.Pin))
        {
            return Ok("Invalid PIN. Please try again.");
        }

        var result = await _accountService.BuyAirtimeForNumberAsync(request.PhoneNumber, request.OtherNumber, request.Amount);
        return Ok(result);
    }

    private async Task<bool> VerifyPin(string phoneNumber, string pin)
    {
        return await _userPinRepository.VerifyPinAsync(phoneNumber, pin);
    }

}

}
