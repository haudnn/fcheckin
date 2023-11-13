using OnetezSoft.Data;
using OnetezSoft.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OnetezSoft.Services
{
  public class CfrService
  {
    public static async Task<bool> SetupEvaluates(string companyId)
    {
      var evaluates = new List<CfrEvaluateModel>
      {
        new CfrEvaluateModel { id = Mongo.RandomId(), name = "Tôi muốn ghi nhận bạn", star = 5, type = 2, special = false },
        new CfrEvaluateModel { id = Mongo.RandomId(), name = "Tôi muốn ghi nhận, cố gắng hơn bạn nhé", star = 10, type = 2, special = false },
        new CfrEvaluateModel { id = Mongo.RandomId(), name = "Bạn đang làm tốt, tiếp tục phát huy nhé", star = 15, type = 2, special = false },
        new CfrEvaluateModel { id = Mongo.RandomId(), name = "Chúc mừng bạn đã hoàn thành công việc", star = 20, type = 2, special = false },
        new CfrEvaluateModel { id = Mongo.RandomId(), name = "Bạn hỗ trợ đồng đội tuyệt vời", star = 30, type = 2, special = false },
        new CfrEvaluateModel { id = Mongo.RandomId(), name = "Bạn đã có sự nỗ lực đáng ghi nhận", star = 40, type = 2, special = false },
        new CfrEvaluateModel { id = Mongo.RandomId(), name = "Bạn thật sự là đồng đội tuyệt vời mà tôi có", star = 60, type = 2, special = false },
        new CfrEvaluateModel { id = Mongo.RandomId(), name = "Bạn thật đẳng cấp, cách làm việc của bạn có ảnh hưởng đến sự phát triển của team", star = 100, type = 2, special = false },
        new CfrEvaluateModel { id = Mongo.RandomId(), name = "WOW, Thật tuyệt vời, bạn đã vượt xa kỳ vọng của tôi!", star = 150, type = 2, special = false }
      };
      foreach (var evaluate in evaluates)
      {
        await DbCfrEvaluate.Create(companyId, evaluate);
      }
      return false;
    }
  }
}