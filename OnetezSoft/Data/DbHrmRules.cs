using MongoDB.Driver;
using OnetezSoft.Models;
using System.Threading.Tasks;

namespace OnetezSoft.Data
{
   public class DbHrmRules
   {
      private static string _collection = "hrm_rules";

      private static async Task<HrmRulesModel> Create(string companyId)
      {
         // Dữ liệu ban đầu
         bool IS_ACTIVE = true;
         bool IS_NOT_ACTIVE = false;
         bool HAS_SHIFT_WORK = true;
         bool HAS_NOT_SHIFT_WORK = false;
         string COLOR_DEFAULT = "#3c6d26";
         HrmRulesModel init = new()
         {
            id = companyId,
            check_in_out = new()
            {
               in_early = 120,
               is_in_early = true,
               in_late = 30,
               out_early = 30
            },
            forms = new()
                {
                    new() { id = Mongo.RandomId(), name = "Nghỉ không lương", color = "#873317", sign = "V", is_active = IS_ACTIVE, has_shift_work = false },
                    new() { id = Mongo.RandomId(), name = "Thai sản", color = COLOR_DEFAULT, sign = "T", is_active = IS_ACTIVE, has_shift_work = HAS_SHIFT_WORK },
                    new() { id = Mongo.RandomId(), name = "Nghỉ phép", color = COLOR_DEFAULT, sign = "P", is_active = IS_ACTIVE, has_shift_work = HAS_SHIFT_WORK },
                    new() { id = Mongo.RandomId(), name = "Cưới/ Tang", color = COLOR_DEFAULT, sign = "C", is_active = IS_ACTIVE, has_shift_work = HAS_SHIFT_WORK },
                    new() { id = Mongo.RandomId(), name = "Công tác", color = COLOR_DEFAULT, sign = "D", is_active = IS_ACTIVE, has_shift_work = HAS_SHIFT_WORK },
                    new() { id = Mongo.RandomId(), name = "Làm việc từ xa", color = COLOR_DEFAULT, sign = "O", is_active = IS_ACTIVE, has_shift_work = HAS_SHIFT_WORK },
                    new() { id = Mongo.RandomId(), name = "Ngày lễ", color = COLOR_DEFAULT, sign = "L", is_active = IS_ACTIVE, has_shift_work = HAS_SHIFT_WORK },
                    new() { id = Mongo.RandomId(), name = "Mẫu đơn từ 1", color = COLOR_DEFAULT, sign = "M1", is_active = IS_NOT_ACTIVE, has_shift_work = HAS_NOT_SHIFT_WORK },
                    new() { id = Mongo.RandomId(), name = "Mẫu đơn từ 2", color = COLOR_DEFAULT, sign = "M2", is_active = IS_NOT_ACTIVE, has_shift_work = HAS_NOT_SHIFT_WORK },
                    new() { id = Mongo.RandomId(), name = "Mẫu đơn từ 3", color = COLOR_DEFAULT, sign = "M3", is_active = IS_NOT_ACTIVE, has_shift_work = HAS_NOT_SHIFT_WORK }
                },
            overtime = new()
            {
               is_show = false,
               min_minutes = 30
            },
            register_shift = new()
            {
               has_register_shifts = false,
               users = new()
            }
         };
         var _db = Mongo.DbConnect("fastdo_" + companyId);
         var collection = _db.GetCollection<HrmRulesModel>(_collection);

         await collection.InsertOneAsync(init);
         return init;
      }


      public static async Task<HrmRulesModel> Update(string companyId, HrmRulesModel model)
      {
         var _db = Mongo.DbConnect("fastdo_" + companyId);

         var collection = _db.GetCollection<HrmRulesModel>(_collection);

         var option = new ReplaceOptions { IsUpsert = false };

         var result = await collection.ReplaceOneAsync(x => x.id.Equals(model.id), model, option);

         return model;
      }

      public static async Task<HrmRulesModel> Get(string companyId, string id)
      {
         var _db = Mongo.DbConnect("fastdo_" + companyId);

         var collection = _db.GetCollection<HrmRulesModel>(_collection);

         var result = await collection.FindAsync(x => x.id == id).Result.FirstOrDefaultAsync();

         if (result != null)
         {
            return result;
         }
         else
         {
            return await Create(companyId);
         }
      }

      public static async Task<bool> IsCheckDevice(string companyId)
      {
         var _db = Mongo.DbConnect("fastdo_" + companyId);

         var collection = _db.GetCollection<HrmRulesModel>(_collection);

         var result = await collection.FindAsync(x => x.id == companyId).Result.FirstOrDefaultAsync();

         return result != null ? result.is_check_device : false;
      }
   }

}

