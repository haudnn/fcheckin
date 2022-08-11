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
    private static string MailSend = "noreply.onetez@gmail.com";
    private static string MailPass = "iktlfiwuejuylfat";
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
    public static bool CompanyActive(string email, string code, out string msg)
    {
      string title = "Xác nhận tạo tổ chức";
      string content = "Mã xác thực: " + code;
      
      return SendMail(email, title, content, null, out msg);
    }


    /// <summary>
    /// Gửi email xác thực quên mật khẩu
    /// </summary>
    public static bool ForgotPassword(string email, string code, out string msg)
    {
      string title = "Xác thực tài khoản";
      string content = "Mã xác thực: " + code;
      
      return SendMail(email, title, content, null, out msg);
    }
  }
}