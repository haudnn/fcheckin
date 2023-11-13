class TelegramBotSetup {
   constructor(token) {
      this.token = token;
      this.requestUrl = 'https://api.telegram.org/bot';
   }

   api(type, method, body) {
      return new Promise((resolve, reject) => {
         fetch(this.requestUrl + this.token + type, {
            method: method,
            body: body
         }).then(res => {
            resolve(res.json())
         }).catch(err => {
            reject(err)
         })
      })
   }
}

class Bot extends TelegramBotSetup {
   constructor(botToken, defaultChatID) {
      super(botToken);
      this.dcid = defaultChatID;
   }

   static start() {
      console.log("Send telegram message with JS\nDeveloper: https://manuchehr.me\nDocs: https://github.com/manuchekhr32/send-telegram-message-with-js");
   }

   async getUpdates() {
      try {
         const result = await this.api('/getUpdates', 'GET')
         return await result
      } catch (e) {
         return await e
      }
   }

   async getMe() {
      try {
         const result = await this.api('/getMe', 'GET')
         return await result
      } catch (e) {
         return await e
      }
   }

   async sendMessage(text, chatID, parseMode, disableNotification) {
      try {
         const result = await this.api(`/sendMessage?text=${text}&chat_id=${chatID ? chatID : this.dcid}&parse_mode=${parseMode ? parseMode : 'html'}&disable_notification=${disableNotification ? disableNotification : false}`, 'GET')
         return await result
      } catch (e) {
         return await e
      }
   }

}

Bot.start()

async function SendReport(type, client, description) {
   var token = "6433974997:AAHgmu2dnvc6xPhwEW4ZJZ55uz6ojP6Pk4k";
   var chatId = "-1001713582349";
   var d = new Date,
      dformat = [d.getMonth() + 1,
      d.getDate(),
      d.getFullYear()].join('/') + ' ' +
         [d.getHours(),
         d.getMinutes(),
         d.getSeconds()].join(':');

   var content = `
      [${type}]${dformat}<br>
      Client: ${client}<br>
      URL: ${window.location.pathname}<br>
      Content: ${description}
   `

   var data = {
      "chat_id": chatId,
      "text": content,
   }

   console.log(data);

   const bot = new Bot(token, "-1001713582349");

   await bot.sendMessage(data.text, chatId, "html", false);
}