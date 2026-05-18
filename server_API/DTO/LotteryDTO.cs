namespace WebApplication1.DTO
{
    public class LotteryDTO
    {
        public int GiftId { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; }
        public int Quantity { get; set; }=1;
        public DateTime LotteryDate { get; set; }
    }

}
