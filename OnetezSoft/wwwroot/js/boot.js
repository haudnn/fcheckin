var reconnectStart = false;
var windowIsFocus = true;

window.addEventListener("offline", () => {
  popupConnect("false", "Bạn đang ngoại tuyến.", false, false);
});

window.addEventListener("online", () => {
  if (!reconnectStart) {
    popupHide();
  }
});

function DisposeObject(obj, method) {
  if (!obj || !method) return;

  window.addEventListener("beforeunload", () => {
    if (!windowIsFocus) {
      obj.invokeMethodAsync(method);
    }
  })
}

let blazor = null;

document.addEventListener("DOMContentLoaded", () => {
  blazor = new InitialBlazor();
  blazor.start();
  setTimeout(function () {
    window.scrollTo(0, 1);
  }, 0);
});

window.addEventListener("load", function() {
  setTimeout(function () {
    window.scrollTo(0, 1);
  }, 0);
});

window.onblur = function () {
  windowIsFocus = false;
};
window.onfocus = function () {
  windowIsFocus = true;
};

class InitialBlazor {
  constructor() {
    this.retryIntervalMilliseconds = 3000;
    this.miliseconds = 0;
    this.isCanceled = false;
    this.currentReconnectionProcess = null;
  }

  start() {
    Blazor.start({
      configureSignalR: function (builder) {
        let c = builder.build();
        c.serverTimeoutInMilliseconds = 60000;
        c.keepAliveIntervalInMilliseconds = 15000;
        builder.build = () => {
          return c;
        };
      },
      reconnectionHandler: {
        onConnectionDown: () => {
          if (!reconnectStart) {
            this.currentReconnectionProcess = this.startReconnectProcess();
          }
        },
        onConnectionUp: () => {
          if (this.currentReconnectionProcess) {
            this.currentReconnectionProcess.cancel();
          }
          this.currentReconnectionProcess = null;
        },
      },
    });
  }

  startReconnectProcess() {
    console.log("start reconnect");
    reconnectStart = true;
    this.isCanceled = false;
    this.miliseconds = 0;

    if (window.navigator.onLine) {
      popupConnect("loading", "Hệ thống đang tự kết nối lại...", false, false, false);
    }
    (async () => {
      while (this.isCanceled == false) {

        await new Promise(resolve => setTimeout(resolve, this.retryIntervalMilliseconds));

        try {
          const result = await Blazor.reconnect();

          if (!result) {
            popupConnect("loading", "Kết nối hết hạn. Làm mới trang trong 3 giây.", false, false);
            setTimeout(location.reload(), 3000);
            return;
          }

          // Successfully reconnected to the server.
          console.log("Client successfully reconnected!");
          popupConnect("success", "Kết nối thành công!", true, true);
          this.isCanceled = true;
          reconnectStart = false;
          return;
        }
        catch (e) {
          // Didn't reach the server; try again.
          if (!window.navigator.onLine) {
            popupConnect("false", "Bạn đang ngoại tuyến.", false, false);
          }
          else {
            if (this.miliseconds >= 60000 * 60 * 6) {
              popupConnect("loading", "Kết nối hết hạn. Làm mới trang trong 3 giây.", false, false);
              this.isCanceled = true;
              setTimeout(location.reload(), 3000);
              return;
            }
            else {
              console.log("Client didn't reached the server!");
              popupConnect("loading", "Máy chủ không phản hồi, đang thử lại", false, false);
            }
          }
        }
        this.miliseconds += this.retryIntervalMilliseconds;
      }
    })();


    return {
      cancel: () => {
        this.isCanceled = true;
        reconnectStart = false;
      },
    };
  }
}