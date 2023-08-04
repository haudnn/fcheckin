using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace OnetezSoft.Services
{
   public class HolidaysGenerator
   {
      private ChineseLunisolarCalendar _calendar = new();
      private int year = 0;
      private List<Holiday> HolidaysList = new();
      private bool LunarLeapYear = false;

      public class Holiday
      {
         public string name { get; set; }
         public long start { get; set; }
         public long end { get; set; }
      }
      public HolidaysGenerator()
      {
         this.year = DateTime.Today.Year;
         this.LunarLeapYear = CheckLunarLeepYear();
         GenerateDays();
      }

      public HolidaysGenerator(int year)
      {
         this.year = year;
         this.LunarLeapYear = CheckLunarLeepYear();
         GenerateDays();
      }

      private void GenerateDays()
      {
         this.HolidaysList.Clear();
         this.HolidaysList = new()
         {
            new()
            {
               name = $"Nghỉ tết dương lịch",
               start = GetTime(1,1),
               end = GetTime(1,1),
            },
            new()
            {
               name = $"Nghỉ tết âm lịch",
               start = GetLunarTime(1,1),
               end = GetLunarTime(5,1),
            },
            new()
            {
               name = $"Nghỉ giỗ tổ Hùng Vương",
               start = GetLunarTime(10,3),
               end = GetLunarTime(10,3),
            },
            new()
            {
               name = $"Nghỉ 30/4 & 1/5",
               start = GetTime(30,4),
               end = GetTime(1,5),
            },
            new()
            {
               name = $"Nghỉ lễ 2/9",
               start = GetTime(2,9),
               end = GetTime(2,9),
            },
            new()
            {
               name = $"Nghỉ lễ giáng sinh",
               start = GetTime(24,12),
               end = GetTime(24,12),
            }
         };
      }

      private long GetLunarTime(int day, int month)
      {
         if (month > 2 && LunarLeapYear)
         {
            return _calendar.ToDateTime(this.year, month, day, 0, 0, 0, 0).AddDays(29).Ticks;
         }
         else
            return _calendar.ToDateTime(this.year, month, day, 0, 0, 0, 0).Ticks;
      }

      public bool CheckLunarLeepYear()
      {
         int[] valid = new int[] { 0, 3, 6, 9, 11, 14, 17 };
         return valid.Contains(this.year % 19);
      }

      private long GetTime(int day, int month)
      {
         return new DateTime(this.year, month, day).Ticks;
      }

      public List<Holiday> GetList()
      {
         return this.HolidaysList;
      }
   }
}