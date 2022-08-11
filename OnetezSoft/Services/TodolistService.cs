using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OnetezSoft.Data;
using OnetezSoft.Models;

namespace OnetezSoft.Services
{
  public class TodolistService
  {
    // <summary>
    /// Hàm tự động xác nhận Todolist
    /// </summary>
    /// <param name="database">Dữ liệu Todolist</param>
    /// <param name="time_confirm">Thời gian tự động xác nhận</param>
    public static async Task AutoConfirm(List<TodolistModel> database, string time_confirm, string companyId)
    {
      var confirmList = database.Where(x => string.IsNullOrEmpty(x.user_confirm)).ToList();
      var timeConfirm = Convert.ToDateTime(DateTime.Now.ToShortDateString() + $", {time_confirm}:00");
      var today = Convert.ToDateTime(DateTime.Now.ToShortDateString());

      foreach (var item in confirmList)
      {
        bool autoConfirm = false;

        if (item.date < today.Ticks)
          autoConfirm = true;
        else if (item.date == today.Ticks && DateTime.Now > timeConfirm)
          autoConfirm = true;

        if (autoConfirm)
        {
          item.date_confirm = DateTime.Now.Ticks;
          item.user_confirm = "auto";
          foreach (var todo in item.todos)
            todo.confirm = true;
          await DbTodolist.Update(companyId, item);
        }
      }
    }
  }
}
