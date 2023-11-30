using OnetezSoft.Data;
using OnetezSoft.Handled;
using OnetezSoft.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace OnetezSoft.Services
{
	public class EmailService
	{
		private static string MailSend = "noreply.fastdo@gmail.com";
		private static string MailPass = "sbhilhztyiawgzzu";
		private static string MailServer = "smtp.gmail.com";
		private static int MailPort = 587;

    #region Kiểm tra cho phép gửi email
    public static async Task<List<MemberModel>> GetAcceptReceiveEmail(List<MemberModel> list, string companyId)
    {
      var all = await DbUser.GetAll(companyId, null);
      var result = new List<MemberModel>();

      foreach (var item in list)
      {
        var user = all.FirstOrDefault(x => x.id == item.id);
        if (user != null)
        {
          if (user.custom.email_notification)
          {
            result.Add(item);
          }
        }
      }

      return result;
    }

    public static async Task<bool> GetAcceptReceiveEmail(MemberModel item, string companyId)
    {
      var user = await DbUser.Get(companyId, item.id, null);
      return user == null || !user.custom.email_notification ? false : true;
    }

    public static bool GetAcceptReceiveEmail(UserModel item, string companyId)
    {
      return item == null || !item.custom.email_notification ? false : true;
    }

    public static async Task<List<UserModel>> GetAcceptReceiveEmail(List<UserModel> list, string companyId)
    {
      var all = await DbUser.GetAll(companyId, null);
      var result = new List<UserModel>();

      foreach (var item in list)
      {
        var user = all.FirstOrDefault(x => x.id == item.id);
        if (user != null)
        {
          if (user.custom.email_notification)
          {
            result.Add(item);
          }
        }
      }

      return result;
    }
    #endregion

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
				msg = "Không thể gửi email: " + ex.ToString();
			}
			return false;
		}
    private static bool SendMail(List<string> emails, string title, string content, string[] bcc, out string msg)
    {
      try
      {
        if (emails.Count > 0)
        {
          emails = emails.Select(x => x.Trim()).ToList();

          MailMessage mailMessage = new MailMessage();
          mailMessage.From = new MailAddress(MailSend, "FASTDO");
          foreach (var email in emails)
          {
            mailMessage.To.Add(email);
          }
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
        msg = "Không thể gửi email: " + ex.ToString();
      }
      return false;
    }

    /// <summary>
    /// Gửi email xác nhận tạo tổ chức
    /// </summary>
    public static bool CompanyActive(string email, string code, string company, out string msg)
		{
			string title = "Thư xác nhận tạo tổ chức";
			string content = "Đường link truy cập: https://work.fastdo.vn";
			content += $"<br>Tên tổ chức: {company}";
			content += $"<br>Mã xác thực: {code}";

			return SendMail(email, title, content, null, out msg);
		}


		/// <summary>
		/// Gửi email xác thực quên mật khẩu
		/// </summary>
		public static bool ForgotPassword(string email, string code, out string msg)
		{
			string title = "Mã xác thực tài khoản";
			string content = $"<br>Tên đăng nhập: {email}";
			content += $"<br>Mã xác thực: {code}";

			return SendMail(email, title, content, null, out msg);
		}


		/// <summary>
		/// Gửi email thông báo tạo tài khoản
		/// </summary>
		public static bool UserInfo(string email, string password, string name, string company, out string msg)
		{
			string title = "Thư cung cấp tài khoản";
			string content = $"<p>Kính chào {name},</p>";
			content += "<p>";
			content += "Dưới đây chính là thông tin tài khoản đăng nhập.";
			content += "</p>";
			content += "<p>TÀI KHOẢN ĐƯỢC KÍCH HOẠT THÀNH CÔNG</p>";
			content += "<p>";
			content += "<strong>1. Thông tin tài khoản:</strong>";
			content += $"<br>Tên đăng nhập: {email}";
			content += $"<br>Mật khẩu: {password}";
			content += "</p>";
			content += "<p>";
			content += "<p>Trân trọng</p>";

			return SendMail(email, title, content, null, out msg);
		}

    public static bool UserListInfo(string email, string name, Dictionary<string, string> list, string company, out string msg)
		{
			string title = "FASTDO | Thư cung cấp tài khoản giải pháp Fastdo ";
			string content = $"<p>Kính chào {(string.IsNullOrEmpty(name) ? "quý khách" : name)},</p>";
			content += "<p>";
			content += $"Fastdo rất vui khi được cùng đồng hành trong quá trình chuyển đổi số của {company}. ";
			content += "Dưới đây chính là danh sách thông tin tài khoản đăng nhập bộ giải pháp quản trị Doanh nghiệp Fastdo.";
			content += "</p>";
			content += $"<p>DANH SÁCH TÀI KHOẢN ĐƯỢC KÍCH HOẠT THÀNH CÔNG ({list.Count} tài khoản)</p>";

			var count = 1;
			foreach (var row in list)
			{
				content += "<p>";
				content += $"<strong>{count}. Thông tin tài khoản:</strong>";
				content += $"<br>Tên đăng nhập: {row.Key}";
				content += $"<br>Mật khẩu: {row.Value}";
				content += "</p>";
				count++;
			}

			content += "<p>Liên kết đăng nhập: https://work.fastdo.vn </p>";
			content += "<strong>2. Tài nguyên đi kèm:</strong>";
			content += "<br>Hướng dẫn sử dụng hệ thống Fastdo: https://help.fastdo.vn";
			content += "<br>Trong trường hợp cần hỗ trợ gấp, Anh/Chị có thể liên hệ trực tiếp qua Hotline  Fastdo: 0905.852.933";
			content += "</p>";
			content += "<p>Trân trọng,<br>Fastdo team</p>";

			return SendMail(email, title, content, null, out msg);
		}

		/// <summary>
		/// Gửi email đơn từ
		/// </summary>
		public static bool FormMail(CompanyModel company, string email, string title, HrmFormModel form, string date, List<UserModel> users, string baseUri, out string msg)
		{
			var formAuthor = users.Find(i => i.id == form.user);
			if (formAuthor != null)
			{
				string content = $"<br>";
				content += $"• Ngày tạo: {new DateTime(form.created).ToString("dd'/'MM'/'yyyy '-' HH'h'mm")}";
				content += "<br>";
				content += $"• Họ và tên: {formAuthor.FullName}";
				content += "<br>";
				content += $"• Loại đơn từ: {form.form.name}";
				content += "<br>";
				content += $"• Ngày áp dụng: {date}";
				content += "<br>";
				content += $"• Tiến trình: ";
				content += "<br>";
				foreach (var item in form.confirm_users.OrderBy(i => i.pos).ToList())
				{
					var user = users.Find(i => i.id == item.user);
					if (user != null)
					{
						if (item.date > 0)
							content += "&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;" + $"+ {user.FullName} đã {DbHrmForm.GetConfirmStatusDetail(item.status).name} Ngày {new DateTime(item.date).ToString("dd'/'MM'/'yyyy '-' HH'h'mm")}";
						else
							content += "&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;" + $"+ {user.FullName} ...";

            content += "<br>";
          }
        }
        content += $"• Lý do: {form.reason}";
        content += "<br>";
        content += $"• Đường dẫn đơn từ: " + $"<a href=\"{baseUri}\" target=\"_blank\">Truy cập</a>";
        content += "<br>";
        if (ProductService.CheckStorage(company) && form.files.Any())
        {
          content += "File đính kèm: ";
          foreach (var link in form.files)
          {
            content += $"<a href=\"{link}\" target=\"_blank\">{Files.FileName(link, 20)}</a>";
            if (form.files.LastOrDefault() != link)
              content += " , ";
          }
        }
        return SendMail(email, title, content, null, out msg);
      }
      else
      {
        msg = "";
        return false;
      }
    }

    public static bool FormMail(CompanyModel company, string email, string title, HrmFormModel form, string date, List<MemberModel> users, string baseUri, out string msg)
    {
      var formAuthor = users.Find(i => i.id == form.user);
      if (formAuthor != null)
      {
        string content = $"<br>";
        content += $"• Ngày tạo: {new DateTime(form.created).ToString("dd'/'MM'/'yyyy '-' HH'h'mm")}";
        content += "<br>";
        content += $"• Họ và tên: {formAuthor.name}";
        content += "<br>";
        content += $"• Loại đơn từ: {form.form.name}";
        content += "<br>";
        content += $"• Ngày áp dụng: {date}";
        content += "<br>";
        content += $"• Tiến trình: ";
        content += "<br>";
        foreach (var item in form.confirm_users.OrderBy(i => i.pos).ToList())
        {
          var user = users.Find(i => i.id == item.user);
          if (user != null)
          {
            if (item.date > 0)
              content += "&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;" + $"+ {user.name} đã {DbHrmForm.GetConfirmStatusDetail(item.status).name} Ngày {new DateTime(item.date).ToString("dd'/'MM'/'yyyy '-' HH'h'mm")}";
            else
              content += "&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;" + $"+ {user.name} ...";

            content += "<br>";
          }
        }
        content += $"• Lý do: {form.reason}";
        content += "<br>";
        content += $"• Đường dẫn đơn từ: " + $"<a href=\"{baseUri}\" target=\"_blank\">Truy cập</a>";
        content += "<br>";
        if (ProductService.CheckStorage(company) && form.files.Any())
        {
          content += "File đính kèm: ";
          foreach (var link in form.files)
          {
            content += $"<a href=\"{link}\" target=\"_blank\">{Files.FileName(link, 20)}</a>";
            if (form.files.LastOrDefault() != link)
              content += " , ";
          }
        }
        return SendMail(email, title, content, null, out msg);
      }
      else
      {
        msg = "";
        return false;
      }
    }

  }
}