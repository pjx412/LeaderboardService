using CustomerRankService.Services;
using Microsoft.AspNetCore.Mvc;
using CustomerRankService.Models;
using System.Numerics;
using System.Collections.Generic;

namespace CustomerRankService.Controllers
{
    [ApiController]
    [Route("/")]    //多路由情况下可用[Route("[controller]")]
    public class LeaderboardController : ControllerBase
    {
        private readonly LeaderboardService _leaderboardService;
        public LeaderboardController(LeaderboardService leaderboardService)
        {
            _leaderboardService = leaderboardService;
        }

        [HttpPost("{customerId}/score/{score}")]
        public IActionResult UpdateScore([FromRoute] long customerId, [FromRoute] decimal score)
        {
            if (score > 1000 || score < -1000)
            {
                return BadRequest(new ResultDto { Code=-1, Success = false, Message = "Score must be between -1000 and 1000" });
            }

            var newScore = _leaderboardService.UpdateScore(customerId, score);
            return Ok(new ResultDto { Success = true, Data = newScore });
        }
       
        [HttpGet("leaderboard")]
        public IActionResult GetCustomersByRank([FromQuery] int start, [FromQuery] int end)
        {
            if (start <= 0 || end <= 0)
            {
                return BadRequest(new ResultDto { Code = -1, Success = false, Message = "Start and end must be greater than 0" });
            }

            if (end < start)
            {
                return BadRequest(new ResultDto { Code = -1, Success = false, Message = "End must be greater than start" });
            }

            var entries = _leaderboardService.GetCustomersByRank(start, end);
            return Ok(new ResultDto { Success = true, Data = entries });
        }

        [HttpGet("leaderboard/{customerId}")]
        public IActionResult GetCustomersByCustomerId([FromRoute] long customerId, [FromQuery] int high = 0, [FromQuery] int low = 0)
        {
            if (high < 0 || low < 0)
            {
                return BadRequest(new ResultDto { Code = -1, Success = false, Message = "High and low must be greater than or equal to 0" });
            }

            var entries = _leaderboardService.GetCustomersByCustomerId(customerId, high, low);
            return Ok(new ResultDto { Success = true, Data = entries });
        }
    }
}