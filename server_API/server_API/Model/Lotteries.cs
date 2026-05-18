using api_server.Model;
using server_API.Model;

namespace api_server.Models
{
    //הגרלות
    public class Lotteries
    {
            public int Id { get; set; }             // מזהה הגרלה

            public int GiftId { get; set; }          // המתנה שעליה בוצעה ההגרלה
            public Gift Gift { get; set; }           // פרטי המתנה

            public int UserId { get; set; }
            public User User { get; set; }
            public DateTime LotteryDate { get; set; }

        }

    
}