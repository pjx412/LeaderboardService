namespace CustomerRankService.Models
{
    public class ResultDto
    {
        public int Code { get; set; }

        public object Data { get; set; }

        public bool Success { get; set; }
        public string Message { get; set; }
    }
}
