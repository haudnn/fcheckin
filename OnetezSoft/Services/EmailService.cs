using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using OnetezSoft.Data;
using OnetezSoft.Models;

namespace OnetezSoft.Services
{
  public class EmailService
  {
    private static string MailSend = "noreply.fastdo@gmail.com";
    private static string MailPass = "sbhilhztyiawgzzu";
    private static string MailServer = "smtp.gmail.com";
    private static int MailPort = 587;


    /// <summary>
    /// Hàm gửi email
    /// </summary>
    private static bool SendMail(string email, string title, string content, string[] bcc, out string msg)
    {
      try
      {
        if (!string.IsNullOrEmpty(email) && !string.IsNullOrEmpty(email.Trim()))
        {
          email = email.Trim();

          MailMessage mailMessage = new MailMessage();
          mailMessage.From = new MailAddress(MailSend, "FASTDO");
          mailMessage.To.Add(email);
          mailMessage.Subject = title;
          mailMessage.Body = content;
          mailMessage.IsBodyHtml = true;

          if (bcc != null && bcc.Count() > 0)
          {
            foreach (string b in bcc)
            {
              if (!string.IsNullOrEmpty(b.Trim()))
                mailMessage.Bcc.Add(b.Trim());
            }
          }

          SmtpClient mailClient = new SmtpClient(MailServer, MailPort);
          mailClient.Timeout = 15000;
          mailClient.EnableSsl = true;
          mailClient.Credentials = new NetworkCredential(MailSend, MailPass);
          mailClient.Send(mailMessage);

          msg = "Đã gửi email thành công!";
          return true;
        }
        else
        {
          msg = "Không có email nhận";
        }
      }
      catch (Exception ex)
      {
        msg = "Không thể gửi email: " + ex.Message;
      }
      return false;
    }


    /// <summary>
    /// Gửi email xác nhận tạo tổ chức
    /// </summary>
    public static bool CompanyActive(string email, string code, string company, out string msg)
    {
      string title = "FASTDO | Thư xác nhận tạo tổ chức";
      string content = "Đường link truy cập: https://work.fastdo.vn";
      content += $"<br>Tên tổ chức: {company}";
      content += $"<br>Mã xác thực: {code}";
      content += "<br>Hướng dẫn sử dụng hệ thống Fastdo: https://help.fastdo.vn";
      
      return SendMail(email, title, content, null, out msg);
    }


    /// <summary>
    /// Gửi email xác thực quên mật khẩu
    /// </summary>
    public static bool ForgotPassword(string email, string code, out string msg)
    {
      string title = "FASTDO | Mã xác thực tài khoản";
      string content = $"<br>Tên đăng nhập: {email}";
      content += $"<br>Mã xác thực: {code}";
      
      return SendMail(email, title, content, null, out msg);
    }


    /// <summary>
    /// Gửi email thông báo tạo tài khoản
    /// </summary>
    public static bool UserInfo(string email, string password, string name, string company,  out string msg)
    {
      string title = "FASTDO | Thư cung cấp tài khoản giải pháp Fastdo ";
      string content = $"<p>Kính chào {name},</p>";
      content += "<p>";
      content += $"Fastdo rất vui khi được cùng đồng hành trong quá trình chuyển đổi số của {company}. ";
      content += "Dưới đây chính là thông tin tài khoản đăng nhập bộ giải pháp quản trị Doanh nghiệp Fastdo.";
      content += "</p>";
      content += "<p>TÀI KHOẢN ĐƯỢC KÍCH HOẠT THÀNH CÔNG</p>";
      content += "<p>";
      content += "<strong>1. Thông tin tài khoản:</strong>";
      content += "<br>Liên kết đăng nhập: https://work.fastdo.vn";
      content += $"<br>Tên đăng nhập: {email}";
      content += $"<br>Mật khẩu: {password}";
      content += "</p>";
      content += "<p>";
      content += "<strong>2. Tài nguyên đi kèm:</strong>";
      content += "<br>Hướng dẫn sử dụng hệ thống Fastdo: https://help.fastdo.vn";
      content += "<br>Trong trường hợp cần hỗ trợ gấp, Anh/Chị có thể liên hệ trực tiếp qua Hotline  Fastdo: 0905.852.933";
      content += "</p>";
      content += "<p>Trân trọng,<br>Fastdo team</p>";
      
      return SendMail(email, title, content, null, out msg);
    }
  }
}