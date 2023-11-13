using DocumentFormat.OpenXml.EMMA;
using OnetezSoft.Data;
using OnetezSoft.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OnetezSoft.Services
{
  public class KpisService
  {
    public static string CheckDate(long time)
    {
      string result = "";

      if (new DateTime(time).Date.Ticks - DateTime.Today.Ticks > 0)
        result = $"Còn {(new DateTime(time).Date - DateTime.Today).Days} ngày";
      else if (new DateTime(time).Date.Ticks - DateTime.Today.Ticks == 0)
        result = "Sắp hết hạn";
      else
        result = "Hết hạn";

      return result;
    }

    public static List<string> GetViewerNoti(Dictionary<string, List<int>> viewers)
    {
      var result = new List<string>();
      foreach (var item in viewers)
      {
        if (item.Value.Contains(2))
        {
          result.Add(item.Key);
        }
      }
      return result;
    }

    #region chỉ số
    public static List<KpisMetricModel> GetMetricsByKpis(List<KpisModel> dataKpis)
    {
      var metrics = new List<KpisMetricModel>();
      foreach (var kpis in dataKpis)
      {
        if (kpis != null)
        {
          var metricCheck = metrics.Select(i => i.id).ToList();
          var metric = kpis.metrics.FirstOrDefault();
          if (metric != null && !metricCheck.Contains(metric.id))
          {
            var metricTemp = Handled.Shared.Clone(metric);
            metricTemp.value = 0;
            metricTemp.target = 0;
            metrics.Add(metricTemp);
          }
        }
      }

      return metrics;
    }

    public static List<KpisMetricModel> GetAllMetricByKpis(List<KpisModel> dataKpis, List<KpisMetricModel> dataMetric)
    {
      var result = dataMetric.Where(i => true).ToList();
      var metricByTree = GetMetricsByKpis(dataKpis);
      result.RemoveAll(x => metricByTree.Select(i => i.id).Contains(x.id));
      result.AddRange(metricByTree);
      result = result.DistinctBy(i => i.id).ToList();
      return result;
    }

    public static void ChangeMetricKpis(KpisModel model, List<KpisMetricModel> dataMetrics, List<KpisModel> database)
    {
      if (model.parent != model.root)
      {
        // đồng bộ chỉ số giữa KPIs cha và chỉ số
        var parentKpis = database.Find(i => i.id == model.parent);
        if (parentKpis != null && parentKpis.metrics.FirstOrDefault() != null)
        {
          parentKpis = Handled.Shared.Clone(parentKpis);
          dataMetrics.RemoveAll(i => i.id == parentKpis.metrics.FirstOrDefault().id);
          dataMetrics.Insert(0, parentKpis.metrics.FirstOrDefault());

          if (model.metrics.FirstOrDefault() == null)
          {
            var childMetric = parentKpis.metrics.FirstOrDefault();
            if (childMetric != null)
            {
              model.metrics.Clear();
              childMetric.value = 0;
              model.metrics.Add(childMetric);
            }
          }
        }
      }
    }
    #endregion

    #region Tree KPIs
    /// <summary>
    /// Lấy danh sách con KPIs
    /// </summary>
    public static List<KpisModel> GetChilds(List<KpisModel> database, string parent)
    {
      return database.Where(x => x.parent == parent).ToList();
    }

    public static List<KpisModel> GetAllChild(List<KpisModel> dataList, List<KpisModel> resultList, KpisModel parent)
    {
      var result = new List<KpisModel>();
      var checkList = dataList.Where(i => true).ToList();
      foreach (var item in resultList)
      {
        var check = checkList.Find(i => i.parent == item.id);
        if (check != null)
          result.Add(check);
      }

      checkList.RemoveAll(i => result.Contains(i));

      if (result.Any())
      {
        resultList.AddRange(result);
        GetAllChild(checkList, resultList, parent);
      }

      resultList.RemoveAll(i => i.id == parent.id);
      return resultList;
    }

    public static List<KpisModel> GetAllParent(List<KpisModel> dataList, List<KpisModel> resultList, KpisModel child)
    {
      var result = new List<KpisModel>();
      var checkList = dataList.Where(i => true).ToList();
      foreach (var item in resultList)
      {
        var check = checkList.Find(i => i.id == item.parent);
        if (check != null)
          result.Add(check);
      }

      checkList.RemoveAll(i => result.Contains(i));

      if (result.Any())
      {
        resultList.AddRange(result);
        GetAllParent(checkList, resultList, child);
      }

      resultList.RemoveAll(i => i.id == child.id);
      return resultList;
    }

    /// <summary>Thay đổi metric</summary>
    public static List<KpisMetricModel> ChangeMetric(List<KpisMetricModel> database, string id)
    {
      var result = new List<KpisMetricModel>();
      var item = database.Find(i => i.id == id);
      if (item != null)
      {
        result.Add(item);
      }
      return result;
    }

    public static async Task<(bool, string, bool)> UpdateKpis(KpisModel modelEdit, KpisModel kpisRoot, List<KpisModel> database, string companyId, string userId, KpisCycleModel cycle, double target, string name, string filterUserName, bool autoCheckin = false)
    {
      bool status = false;
      bool checkReload = false;
      string mess = "";

      if (modelEdit.start_date == 0 || modelEdit.start_date == 0)
        mess = $"Bạn chưa nhập Thời hạn!";
      else if (new DateTime(modelEdit.start_date).Date.Ticks >= new DateTime(modelEdit.end_date).Date.Ticks)
        mess = $"Ngày kết thúc phải lớn hơn Ngày bắt đầu!";
      else if (modelEdit.metrics.FirstOrDefault() == null)
        mess = $"Bạn chưa chọn Chỉ số KPIs!";
      else if (modelEdit.metrics.FirstOrDefault() != null && target == 0)
        mess = $"Mục tiêu mong muốn phải khác 0!";
      else if (modelEdit.type == 1 && (Handled.Shared.IsEmpty(modelEdit.user) || Handled.Shared.IsEmpty(filterUserName)))
        mess = $"Bạn chưa nhập Người thực hiện!";
      else if (modelEdit.type == 2 && (Handled.Shared.IsEmpty(modelEdit.user) || Handled.Shared.IsEmpty(filterUserName)))
        mess = $"Bạn chưa nhập Người giám sát!";
      else
      {
        var metric = modelEdit.metrics.FirstOrDefault();
        if (Handled.Shared.IsEmpty(name))
          name = $"{metric.name} {Handled.Shared.ConvertCurrencyDouble(target)} {metric.units}";

        var check = await DbKpis.Get(companyId, modelEdit.id);

        status = true;
        modelEdit.cycle = cycle.id;
        modelEdit.name = name;
        modelEdit.metrics.FirstOrDefault().target = target;
        modelEdit.root = kpisRoot.id;

        var checkList = new List<KpisModel>();
        var dataUpdateCheckin = new List<KpisCheckinModel>();
        if (check != null)
        {
          if (modelEdit.type == 2)
          {
            // TH: KPIs tự động
            // lấy danh sách con
            List<KpisModel> childList = GetChilds(database, modelEdit.id);

            checkList = childList.Where(i => i.type == 1 && i.user == modelEdit.user).ToList();
            dataUpdateCheckin = await DbKpisCheckin.GetListUpdate(companyId, cycle.id, checkList.Select(i => i.id).ToList());


            if (dataUpdateCheckin.Any() && !autoCheckin)
            {
              return (false, "", false);
            }
          }

          mess = $"Đã cập nhật KPIs thành công!";

          if (autoCheckin)
          {
            checkReload = true;

            if (dataUpdateCheckin.Any())
            {
              // Cập nhật dữ liệu cha
              database.RemoveAll(i => i.id == modelEdit.id);
              database.Add(modelEdit);

              foreach (var item in dataUpdateCheckin)
              {
                var kpis = database.FirstOrDefault(i => i.id == item.kpis);
                var kpisUpdate = Handled.Shared.Clone(kpis);
                if (kpisUpdate != null)
                {
                  item.status_checkin = 2;
                  item.date_confirm = DateTime.Now.Ticks;
                  item.user_confirm = item.user_create;
                  await DbKpisCheckin.Update(companyId, item);

                  // Update KPIs
                  kpisUpdate.status_checkin = item.status_checkin;
                  kpisUpdate.date_confirm = DateTime.Now.Ticks;

                  if (kpisUpdate.metrics.FirstOrDefault() != null)
                    kpisUpdate.metrics.FirstOrDefault().value = item.value;

                  await SyncData(companyId, database, cycle.id, kpisRoot, kpisUpdate);
                }
              }

              // cập nhật dữ liệu cha hiện tại
              var parentNew = await DbKpis.Get(companyId, modelEdit.id);
              if (parentNew != null)
              {
                modelEdit.metrics = parentNew.metrics;
              }
            }
          }

          await DbKpis.Update(companyId, modelEdit);
        }
        else
        {
          await DbKpis.Create(companyId, modelEdit);
          mess = $"Đã tạo KPIs thành công!";
        }

        // Gửi thông báo
        var treeUser = cycle.managers.Where(i => true).ToList();
        treeUser.AddRange(kpisRoot.managers);
        treeUser.AddRange(GetViewerNoti(kpisRoot.viewerList));
        treeUser = treeUser.Distinct().ToList();

        var userPerform = new List<string>() { modelEdit.user };

        if (check == null)
        {
          // Thông báo cho người xem và quản lý
          await SendNotify(1006, treeUser, modelEdit, kpisRoot, companyId, userId, cycle);

          // Thông báo cho người giám sát hoặc người thực hiện
          if (modelEdit.type == 2)
            await SendNotify(1007, userPerform, modelEdit, kpisRoot, companyId, userId, cycle);
          else if (modelEdit.type == 1)
            await SendNotify(1008, userPerform, modelEdit, kpisRoot, companyId, userId, cycle);
        }
        else
        {
          // Thông báo cho người xem và quản lý
          await SendNotify(1009, treeUser, modelEdit, kpisRoot, companyId, userId, cycle);

          // Thông báo cho người giám sát hoặc người thực hiện
          if (modelEdit.type == 2)
            await SendNotify(1010, userPerform, modelEdit, kpisRoot, companyId, userId, cycle);
          else if (modelEdit.type == 1)
            await SendNotify(1011, userPerform, modelEdit, kpisRoot, companyId, userId, cycle);
        }

        // lưu lịch sử kpis gốc
        await UpdateHistoryKpisRoot(kpisRoot, companyId);
      }

      return (status, mess, checkReload);
    }

    public static double GetProgress(List<KpisModel> database, KpisModel model, string kpisRootId)
    {
      double result = 0;
      if (model.id == kpisRootId)
      {
        // TH: KPIs quản lý hoặc KPIs tổng
        var childList = database.Where(i => !Handled.Shared.IsEmpty(i.name) && i.parent == model.id).ToList();

        childList = childList.Where(i => i.metrics.FirstOrDefault() != null).ToList();
        if (childList.Any())
        {
          double resultChild = 0;
          foreach (var item in childList)
          {
            if (item.type == 2)
            {
              resultChild += GetProgress(database, item, kpisRootId);
            }
            else
            {
              if (item.metrics.FirstOrDefault() != null)
              {
                var average = item.metrics.FirstOrDefault().value / item.metrics.FirstOrDefault().target;

                average = average * 100;

                if (average < 0)
                  average = 0;
                else if (average > 100)
                  average = 100;

                resultChild += average;
              }
            }
          }

          result = resultChild / childList.Count;
        }
      }
      else
      {
        //if (model.metrics.FirstOrDefault() != null)
        //  result = model.metrics.FirstOrDefault().value / model.metrics.FirstOrDefault().target;
        var metric = model.metrics.FirstOrDefault();

        if (metric != null)
        {
          if (metric.target > 0)
          {
            if (metric.value <= 0)
              result = 0;
            else
              result = metric.value / metric.target;
          }
          else if (metric.target < 0)
          {
            if (metric.value == 0)
              result = 0;
            else if (metric.value >= metric.target)
              result = 1;
            else
              result = metric.target / metric.value;
          }
        }

        result = result * 100;

        if (result < 0)
          result = 0;
        else if (result > 100)
          result = 100;
      }

      return Math.Round(result, 1);
    }

    public static double GetProgressCheckin(double value, double target)
    {
      double result = 0;

      if (target > 0)
      {
        if (value <= 0)
          result = 0;
        else
          result = value / target;
      }
      else if (target < 0)
      {
        if (value == 0)
          result = 0;
        else if (value >= target)
          result = 1;
        else
          result = target / value;
      }

      result = result * 100;

      if (result < 0)
        result = 0;
      else if (result > 100)
        result = 100;

      return Math.Round(result, 1);
    }
    #endregion

    #region Checkin KPIs
    public static async Task<(bool, string)> UpdateCheckin(KpisCheckinModel modelEdit, KpisModel kpis, KpisModel kpisRoot, List<KpisModel> dataKpis, KpisCycleModel cycle, string companyId, string userId, bool checkAutoCheckin = false)
    {
      bool status = false;
      string mess = "";

      if (checkAutoCheckin)
      {
        modelEdit.date_confirm = DateTime.Now.Ticks;
        modelEdit.status_checkin = 2;
        modelEdit.user_confirm = userId;
      }

      if (Handled.Shared.IsEmpty(modelEdit.comment))
        mess = $"Bạn chưa nhập Nhận xét!";
      else
      {
        status = true;
        modelEdit.kpis = kpis.id;
        modelEdit.kpis_root = kpisRoot.id;
        modelEdit.cycle = kpis.cycle;
        modelEdit.user_create = userId;

        if (!checkAutoCheckin)
          modelEdit.status_checkin = 1;

        var parent = dataKpis.FirstOrDefault(i => i.id == kpis.parent);

        if (parent != null)
        {
          if (Handled.Shared.IsEmpty(modelEdit.id))
          {
            modelEdit.id = Mongo.RandomId();
            await DbKpisCheckin.Create(companyId, modelEdit);

            if (!checkAutoCheckin)
            {
              await SendNotify(1015, new List<string>() { parent.user }, kpis, kpisRoot, companyId, userId, cycle);
              mess = "Đã tạo bản check-in thành công!";
            }
            else
              mess = "Đã cập nhật bản dữ liệu check-in lên cây KPIs!";
          }
          else
          {
            await DbKpisCheckin.Update(companyId, modelEdit);
            mess = "Đã cập nhật bản check-in thành công!";
            await SendNotify(1016, new List<string>() { parent.user }, kpis, kpisRoot, companyId, userId, cycle);
          }

          // Update KPIs
          kpis.status_checkin = modelEdit.status_checkin;
          if (kpis.status_checkin == 2)
            kpis.date_confirm = DateTime.Now.Ticks;
          else if (kpis.status_checkin == 1)
            kpis.date_checkin = DateTime.Now.Ticks;

          await DbKpis.Update(companyId, kpis);
        }
      }

      return (status, mess);
    }

    /// <summary>Đồng bộ dữ liệu</summary>
    public static async Task SyncData(string companyId, List<KpisModel> dataList, string cycleId, KpisModel kpisRoot, KpisModel model, bool isDelete = false)
    {
      var database = new List<KpisModel>();
      if (!isDelete)
        database = await DbKpis.GetList(companyId, cycleId);
      else
        database = dataList;

      var listUpdate = new List<KpisModel>();
      // TH: xoá KPIs
      listUpdate.AddRange(GetParent(database, model, isDelete, true));

      // Lưu lịch sử checkin kpis tự động
      await UpdateCheckinAutoKpis(listUpdate, kpisRoot, companyId);

      if (!isDelete)
      {
        // TH: checkin KPIs
        listUpdate.Add(model);
      }

      listUpdate = listUpdate.DistinctBy(i => i.id).ToList();
      foreach (var item in listUpdate)
      {
        await DbKpis.Update(companyId, item);
      }

      // lưu lịch sử kpis gốc
      await UpdateHistoryKpisRoot(kpisRoot, companyId);
    }

    private static List<KpisModel> GetParent(List<KpisModel> database, KpisModel child, bool isDelete, bool isFirstParent)
    {
      var result = new List<KpisModel>();

      var checkParent = database.FirstOrDefault(i => i.id == child.parent);
      if (checkParent != null)
      {
        var parent = Handled.Shared.Clone(checkParent);
        var metricParent = parent.metrics.FirstOrDefault();

        if (isFirstParent)
        {
          double oldValue = 0;
          var oldItem = database.FirstOrDefault(i => i.id == child.id);
          if (oldItem != null && oldItem.metrics.FirstOrDefault() != null)
            oldValue = oldItem.metrics.FirstOrDefault().value;

          if (metricParent != null && child.metrics.FirstOrDefault() != null)
          {
            if (metricParent.id == child.metrics.FirstOrDefault().id)
            {
              metricParent.value = metricParent.value - oldValue + (isDelete ? 0 : child.metrics.FirstOrDefault().value);

              parent.metrics.Clear();
              parent.metrics.Add(metricParent);
              result.Add(parent);
            }
          }
        }
        else
        {
          var childList = GetChilds(database, checkParent.id);
          childList.RemoveAll(i => i.id == child.id);
          childList.Add(child);
          if (metricParent != null && child.metrics.FirstOrDefault() != null)
          {
            metricParent.value = 0;
            foreach (var item in childList)
            {
              var metricChild = item.metrics.FirstOrDefault();
              if (metricChild != null)
              {
                if (metricParent.id == metricChild.id)
                {
                  metricParent.value += metricChild.value;

                  parent.metrics.Clear();
                  parent.metrics.Add(metricParent);
                  result.Add(parent);
                }
              }
            }
          }
        }
        database.RemoveAll(i => i.id == parent.id);
        database.Add(parent);
        result.AddRange(GetParent(database, parent, isDelete, false));
      }
      return result;
    }

    /// <summary>
    /// Cập nhật lịch sử checkin (giá trị thực đạt) của kpis tự động
    /// </summary>
    private static async Task UpdateCheckinAutoKpis(List<KpisModel> dataList, KpisModel kpisRoot, string companyId)
    {
      var autoKpisList = dataList.DistinctBy(i => i.id).ToList();
      foreach (var item in autoKpisList)
      {
        if (item.metrics.FirstOrDefault() != null)
        {
          var itemCheckin = new KpisCheckinModel()
          {
            cycle = item.cycle,
            date_confirm = DateTime.Today.Ticks,
            kpis = item.id,
            kpis_root = kpisRoot.id,
            status_checkin = 2,
            value = item.metrics.FirstOrDefault().value
          };

          var checkExist = await DbKpisCheckin.GetByKpisIdAndDate(companyId, item.id, itemCheckin.date_confirm);
          if (checkExist == null)
            await DbKpisCheckin.Create(companyId, itemCheckin);
          else
          {
            itemCheckin.id = checkExist.id;
            await DbKpisCheckin.Update(companyId, itemCheckin);
          }
        }
      }
    }

    public static List<KpisCheckinModel> GetDataConfirmCheckin(KpisModel model, List<KpisCheckinModel> dataCheckinConfirm, List<KpisModel> kpisList)
    {
      var dataCheckin = new List<KpisCheckinModel>();
      if (model.type == 1)
        dataCheckin = dataCheckinConfirm.Where(i => i.kpis == model.id).ToList();
      else if (model.type == 2 && model.metrics.FirstOrDefault() != null)
      {
        var childList = GetAllChild(kpisList, new List<KpisModel>() { model }, model);
        childList = childList.DistinctBy(i => i.id).ToList();

        childList = childList.Where(i => i.metrics.FirstOrDefault() != null
                                         && i.metrics.FirstOrDefault().id == model.metrics.FirstOrDefault().id).ToList();

        dataCheckin = dataCheckinConfirm.Where(i => childList.Select(x => x.id).Contains(i.kpis)).ToList();
      }

      dataCheckin.RemoveAll(i => Handled.Shared.IsEmpty(i.user_confirm) && i.status_checkin == 2);

      return dataCheckin;
    }
    #endregion

    #region update nhiều kpis
    public static List<KpisModel> GetListRootKpis(List<KpisModel> database, KpisCycleModel cycle, string userId, bool checkManagerCycle = true)
    {
      return database.Where(i =>
      {
        if (i.parent == i.cycle)
          if (cycle.managers.Contains(userId) && checkManagerCycle)
            return true;
          else if (i.managers.Contains(userId) || new List<string>(i.viewerList.Keys).Contains(userId))
            return true;

        return false;

      }).ToList();
    }
    #endregion

    #region bình luận
    public static List<MemberModel> GetMemberComment(List<MemberModel> userList, List<KpisModel> database, KpisModel model, KpisModel kpisRoot, KpisCycleModel cycle)
    {
      var result = new List<MemberModel>();
      if (cycle != null && model != null && kpisRoot != null)
      {
        var userIdList = new List<string>();
        // người quản lý chu kỳ
        userIdList.AddRange(cycle.managers);
        // người quản lý và xem cây KPIs
        userIdList.AddRange(kpisRoot.managers);
        userIdList.AddRange(GetViewerNoti(kpisRoot.viewerList));
        // người quản lý phía trên
        var parentList = GetAllParent(database, new List<KpisModel>() { model }, model);
        userIdList.AddRange(parentList.Select(i => i.user).ToList());
        // người chủ sở hữ KPIs
        userIdList.Add(model.user);

        userIdList = userIdList.Where(i => !Handled.Shared.IsEmpty(i)).ToList();
        userIdList = userIdList.Distinct().ToList();

        foreach (var userId in userIdList)
        {
          var user = userList.FirstOrDefault(i => i.id == userId);
          if (user != null)
            result.Add(user);
        }
      }
      return result;
    }

    public static async Task SendNotifyComment(List<string> userMentions, List<string> userComment, List<KpisModel> database, KpisModel model, KpisModel kpisRoot, string companyId, string userId, KpisCycleModel cycle)
    {
      if (model != null)
        userComment.Add(model.user);

      userComment = userComment.Distinct().ToList();

      var parentList = GetAllParent(database, new List<KpisModel>() { model }, model);
      var managerList = parentList.Select(i => i.user).ToList();

      // gửi bình luận cho người đã tag
      foreach (var user in userMentions)
      {
        var target = new List<string>() { user };
        int type = -1;
        if (model.user == user)
        {
          // chủ sở hữu
          if (model.type == 1)
            type = 1027;
          else if (model.type == 2)
            type = 1028;
        }
        else if (managerList.Contains(user))
        {
          // quản lý phía trên
          type = 1029;
        }
        else if (cycle.managers.Contains(user) || kpisRoot.managers.Contains(user) || GetViewerNoti(kpisRoot.viewerList).Contains(user))
        {
          // quản lý cây kpis, quản lý chu kỳ, người xem
          type = 1030;
        }
        await SendNotify(type, target, model, kpisRoot, companyId, userId, cycle);
      }

      var otherUser = userComment.Where(i => !userMentions.Contains(i)).ToList();
      foreach (var user in otherUser)
      {
        var target = new List<string>() { user };
        int type = -1;
        if (model.user == user)
        {
          // chủ sở hữu
          if (model.type == 1)
            type = 1023;
          else if (model.type == 2)
            type = 1024;
        }
        else if (managerList.Contains(user))
        {
          // quản lý phía trên
          type = 1025;
        }
        else if (cycle.managers.Contains(user) || kpisRoot.managers.Contains(user) || GetViewerNoti(kpisRoot.viewerList).Contains(user))
        {
          // quản lý cây kpis, quản lý chu kỳ, người xem
          type = 1026;
        }
        await SendNotify(type, target, model, kpisRoot, companyId, userId, cycle);
      }
    }
    #endregion

    #region thống kê
    public static async Task UpdateHistoryKpisRoot(KpisModel kpisRoot, string companyId)
    {
      var database = await DbKpis.GetList(companyId, kpisRoot.cycle);
      var progress = GetProgress(database, kpisRoot, kpisRoot.id);
      var report = new KpisRootReportModel()
      {
        date = DateTime.Today.Ticks,
        cycle = kpisRoot.cycle,
        kpis_root = kpisRoot.id,
        value = progress
      };
      var checkReport = await DbKpisRootReport.GetByKpisIdAndDate(companyId, kpisRoot.id, report.date);
      if (checkReport == null)
        await DbKpisRootReport.Create(companyId, report);
      else
      {
        report.id = checkReport.id;
        await DbKpisRootReport.Update(companyId, report);
      }
    }
    #endregion

    #region thông báo chuông 
    public static async Task SendNotify(int notifyType, List<string> targetList, KpisModel model, KpisModel kpisRoot, string companyId, string userId, KpisCycleModel cycle)
    {
      if (model != null && kpisRoot != null && cycle != null)
      {
        var content = $"<b>{model.name}</b>@@@{model.id}@@@<b>{kpisRoot.name}</b>@@@{kpisRoot.id}@@@<b>{cycle.name}</b>@@@{cycle.id}";
        // Gửi thông báo chuông
        foreach (var item in targetList)
          await DbNotify.Create(companyId, notifyType, content, item, userId);
      }
    }
    #endregion
  }
}
