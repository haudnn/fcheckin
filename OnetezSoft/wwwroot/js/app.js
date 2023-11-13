var colorArray = [
  "#DC787E",
  "#F59E6C",
  "#986CF5",
  "#6CB4F5",
  "#F5D76C",
  "#485fc7",
  "#3e8ed0",
  "#48c78e",
  "#ffe08a",
  "#f14668",
  "#FF6633",
  "#FFB399",
  "#FF33FF",
  "#FFFF99",
  "#00B3E6",
  "#E6B333",
  "#3366E6",
  "#999966",
  "#99FF99",
  "#B34D4D",
  "#80B300",
  "#809900",
  "#E6B3B3",
  "#6680B3",
  "#66991A",
  "#FF99E6",
  "#CCFF1A",
  "#FF1A66",
  "#E6331A",
  "#33FFCC",
  "#66994D",
  "#B366CC",
  "#4D8000",
  "#B33300",
  "#CC80CC",
  "#66664D",
  "#991AFF",
  "#E666FF",
  "#4DB3FF",
  "#1AB399",
  "#E666B3",
  "#33991A",
  "#CC9999",
  "#B3B31A",
  "#00E680",
  "#4D8066",
  "#809980",
  "#E6FF80",
  "#1AFF33",
  "#999933",
  "#FF3380",
  "#CCCC00",
  "#66E64D",
  "#4D80CC",
  "#9900B3",
  "#E64D66",
  "#4DB380",
  "#FF4D4D",
  "#99E6E6",
  "#6666FF",
];
const API_KEY_GOOGLE_MAP = 'AIzaSyD5-kgedsaAbUYepGcZC2JwZBLp_3In2LI';
const END_POINT_APPLE = 'https://mdmenrollment.apple.com/devices';

const dotnetReference = {
  obj: null,
  method: null,
}

window.SetDotNetReference = (obj, method) => {
  dotnetReference.obj = obj;
  dotnetReference.method = method;
}

// define a hotkey handler
function hotkey(e) {

  if (e.keyCode == 27 || e.code === "Escape") {
    var ele = document.querySelector("input:focus");
    if (!ele)
      ele = document.querySelector("textarea:focus");

    if (ele)
      ele.blur();
  }

  if (e.ctrlKey && e.code === 'Slash') {
    var ele = document.querySelector(".header_main .search_bar .input");
    if (ele) {
      var id = ele.getAttribute("id");
      setFocus(id);
    }
  }

  if (e.shiftKey && e.code === 'KeyN') {
    var ele = document.querySelector("input:focus");
    if (!ele)
      ele = document.querySelector("textarea:focus");

    if (ele)
      return;

    if (dotnetReference.obj && dotnetReference.method) {
      dotnetReference.obj.invokeMethodAsync(dotnetReference.method);
    }
  }
}
// register the handler 
document.addEventListener('keyup', hotkey, false);

function windowHeight() {
  const doc = document.documentElement;
  doc.style.setProperty('--window-height', `${document.documentElement.clientHeight}px`)
}
window.addEventListener('resize', windowHeight);
windowHeight();

function checkCookie() {
  var cookieEnabled = navigator.cookieEnabled;

  if (!cookieEnabled) {
    tagline(false, "Cookie đã bị vô hiệu hoá. Hệ thống sẽ không thể hoạt động ổn định cho đến khi Cookie được mở lại.");
  }

  return cookieEnabled;
}

function setCookie(key, value) {
  if (checkCookie()) {
    let expires = new Date();
    expires.setTime(expires.getTime() + 30 * 24 * 60 * 60 * 1000);
    document.cookie =
      key + "=" + value + ";expires=" + expires.toUTCString() + ";path=/";
  }
}

function setCookieDevice(key, value) {
  if (checkCookie()) {
    let expires = new Date();
    expires.setTime(expires.getTime() + 365 * 24 * 60 * 60 * 1000);
    document.cookie =
      key + "=" + value + ";expires=" + expires.toUTCString() + ";path=/";
  }
}

window.getUA = () => {
  let device = "Unknown";
  const ua = {
    "Android": /Android/i,
    "iPad": /iPad/i,
    "iPhone": /iPhone/i,
  }
  Object.keys(ua).map(v => navigator.userAgent.match(ua[v]) && (device = v));
  return device;
}

function getCookie(key) {
  if (checkCookie()) {
    var keyValue = document.cookie.match("(^|;) ?" + key + "=([^;]*)(;|$)");
    return keyValue ? keyValue[2] : null;
  }
  return null;
}

function deleteCookie(key) {
  if (checkCookie()) {
    document.cookie = key + "=;expires=Thu, 01 Jan 1970 00:00:00;path=/";
  }
}

window.SetFocusToElement = (element) => {
  element.focus();
};

window.SetOutFocusToElement = (id) => {
  if (!id)
    return;
  var ele = document.getElementById(id);

  if (!ele)
    return;

  ele.blur();
};

window.getScreenRatio = () => {
  return [window.innerWidth, window.innerHeight];
}

window.getUserAgent = () => {
  return navigator.userAgent;
};

window.getDeviceName = function () {
  return navigator.userAgent;
};

window.copyToClipboard = function (text) {
  navigator.clipboard
    .writeText(text)
    .then(function () {
      console.log("Text copied to clipboard");
    })
    .catch(function (error) {
      console.error("Failed to copy text: ", error);
    });
};

window.getIpAddress = () => {
  return fetch("https://api.ipify.org/")
    .then((response) => response.text())
    .then((data) => {
      return data;
    });
};

// GET location from gg map
const getLocationAPI = async () => {
  try {
    const response = await fetch(`https://www.googleapis.com/geolocation/v1/geolocate?key=${API_KEY_GOOGLE_MAP}`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify({}),
    });

    const data = await response.json();
    return data.location;

  } catch (error) {
    return null;
  }
};

let getLocationPromise = () => {
  return new Promise(function (resolve, reject) {
    navigator.geolocation.getCurrentPosition(
      (position) => resolve(position),
      (error) => reject(error),
      { enableHighAccuracy: true }
    );
  });
};

// Lấy vị trí từ browserư
window.getLocation = async (obj) => {
  var dotNet = obj;
  try {
    const { coords } = await getLocationPromise();
    const value = [coords.latitude, coords.longitude];
    if (dotNet) dotNet.invokeMethodAsync("GetLocation", value);
    return value;
  } catch (error) {
    if (dotNet) dotNet.invokeMethodAsync("GetLocation", []);
    return null;
  }
};

// Lấy vị trí từ map => nếu không có call lại browser
window.getLocationGGMAP = async (obj) => {
  var dotNet = obj;
  try {
    const locationFromAPI = await getLocationAPI();
    if (locationFromAPI === null) {
      const { coords } = await getLocationPromise();
      const value = [coords.latitude, coords.longitude];
      if (dotNet) dotNet.invokeMethodAsync("GetLocation", value);
      return value;
    } else {
      const value = [locationFromAPI.lat, locationFromAPI.lng];
      if (dotNet) dotNet.invokeMethodAsync("GetLocation", value);
      return locationFromAPI;
    }
  } catch (error) {
    if (dotNet) dotNet.invokeMethodAsync("GetLocation", []);
    return null;
  }
};

function setFocus(id) {
  setTimeout(function () {
    const element = document.getElementById(id);
    if (element != null) {
      element.click();
      element.focus();
    }
  }, 100);
}

function showClock() {
  const clock = document.getElementById("clock");
  if (clock !== null) {
    const date = new Date();
    let h = date.getHours(); // 0 - 23
    let m = date.getMinutes(); // 0 - 59
    let s = date.getSeconds(); // 0 - 59

    h = h < 10 ? "0" + h : h;
    m = m < 10 ? "0" + m : m;
    s = s < 10 ? "0" + s : s;

    var time = h + ":" + m + ":" + s;
    clock.innerText = time;
    setTimeout(showClock, 1000);
  }
}

function scrollDiv(id, top) {
  setTimeout(function () {
    const element = document.getElementById(id);
    if (element != null) {
      element.scrollTo({
        top: top,
        left: 0,
        behavior: "smooth",
      });
    }
  }, 100);
}

function scrollGantt(value) {
  setTimeout(function () {
    const element = document.querySelector(".list_task_gantt");
    if (element != null) {
      element.scrollTo({
        top: 0,
        left: value,
        behavior: "smooth",
      });
    }
  }, 500);
}

function dragScroll(elementId) {
  setTimeout(function () {
    const element = document.getElementById(elementId);
    if (element == null) return;
    element.style.cursor = "grab";

    let pos = { top: 0, left: 0, x: 0, y: 0 };

    const mouseDownHandler = function (e) {
      element.style.cursor = "grabbing";
      element.style.userSelect = "none";

      pos = {
        left: element.scrollLeft,
        top: element.scrollTop,
        // Get the current mouse position
        x: e.clientX,
        y: e.clientY,
      };

      document.addEventListener("mousemove", mouseMoveHandler);
      document.addEventListener("mouseup", mouseUpHandler);
    };

    const mouseMoveHandler = function (e) {
      // How far the mouse has been moved
      const dx = e.clientX - pos.x;
      const dy = e.clientY - pos.y;

      // Scroll the element
      element.scrollTop = pos.top - dy;
      element.scrollLeft = pos.left - dx;
    };

    const mouseUpHandler = function () {
      element.style.cursor = "grab";
      element.style.removeProperty("user-select");

      document.removeEventListener("mousemove", mouseMoveHandler);
      document.removeEventListener("mouseup", mouseUpHandler);
    };

    // Attach the handler
    element.addEventListener("mousedown", mouseDownHandler);
  }, 500);
}

function dragScrollX() {
  setTimeout(function () {
    const element = document.getElementById("scrollbox");
    if (element == null) return;

    element.style.cursor = "grab";

    let pos = { top: 0, left: 0, x: 0, y: 0 };

    const mouseDownHandler = function (e) {
      element.style.cursor = "grabbing";
      element.style.userSelect = "none";

      pos = {
        left: element.scrollLeft,
        top: element.scrollTop,
        // Get the current mouse position
        x: e.clientX,
        y: e.clientY,
      };

      document.addEventListener("mousemove", mouseMoveHandler);
      document.addEventListener("mouseup", mouseUpHandler);
    };

    const mouseMoveHandler = function (e) {
      // How far the mouse has been moved
      const dx = e.clientX - pos.x;

      // Scroll the element
      element.scrollLeft = pos.left - dx;
    };

    const mouseUpHandler = function () {
      element.style.cursor = "grab";
      element.style.removeProperty("user-select");

      document.removeEventListener("mousemove", mouseMoveHandler);
      document.removeEventListener("mouseup", mouseUpHandler);
    };

    // Attach the handler
    element.addEventListener("mousedown", mouseDownHandler);
  }, 500);
}

function dragScrollById(id) {
  setTimeout(function () {
    const element = document.getElementById(id);
    if (element == null) return;

    element.style.cursor = "grab";

    let pos = { top: 0, left: 0, x: 0, y: 0 };

    const mouseDownHandler = function (e) {
      element.style.cursor = "grabbing";
      element.style.userSelect = "none";

      pos = {
        left: element.scrollLeft,
        top: element.scrollTop,
        // Get the current mouse position
        x: e.clientX,
        y: e.clientY,
      };

      document.addEventListener("mousemove", mouseMoveHandler);
      document.addEventListener("mouseup", mouseUpHandler);
    };

    const mouseMoveHandler = function (e) {
      // How far the mouse has been moved
      const dx = e.clientX - pos.x;

      // Scroll the element
      element.scrollLeft = pos.left - dx;
    };

    const mouseUpHandler = function () {
      element.style.cursor = "grab";
      element.style.removeProperty("user-select");

      document.removeEventListener("mousemove", mouseMoveHandler);
      document.removeEventListener("mouseup", mouseUpHandler);
    };

    // Attach the handler
    element.addEventListener("mousedown", mouseDownHandler);
  }, 500);
}

function toggleText(item) {
  if (item.className.indexOf("is-show") === -1) item.classList.add("is-show");
  else item.classList.remove("is-show");
}

function textAutoSize(id) {
  setTimeout(function () {
    const text = document.getElementById(id);
    if (text == null) return;
    text.addEventListener("input", setAutoHeight);
    setTextHeight(text);
  }, 100);
}

function setAutoHeight() {
  setTextHeight(this);
}

function setTextHeight(text) {
  text.style.height = "auto";
  text.style.height = text.scrollHeight + "px";
  text.style.overflow = "hidden";
}

// await JSRuntime.InvokeVoidAsync("dropdownClose");
function dropdownClose() {
  const dropdown = document.querySelectorAll(".dropdown.is-active");
  if (dropdown.length == 0) {
    return;
  }
  dropdown.forEach((x) => x.classList.remove("is-active"));
}

function clickBtn(id) {
  const btn = this.document.getElementById(id);
  if (btn != null) btn.click();
}

function newTab(link) {
  window.open(link);
}

function goBack() {
  history.back();
}

function runChartBarMulti(labels, dataLabel, dataValue) {
  const chart = Chart.getChart("chartBarMulti");
  if (chart !== undefined) chart.destroy();

  let datasets = [];
  for (let i = 0; i < dataValue.length; i++) {
    datasets.push({
      label: dataLabel[i],
      data: dataValue[i].split(","),
      borderColor: [colorArray[i]],
      backgroundColor: [colorArray[i]],
    });
  }

  new Chart("chartBarMulti", {
    type: "bar",
    data: {
      labels: labels,
      datasets: datasets,
    },
    options: {
      maintainAspectRatio: false,
      scales: {
        yAxes: [
          {
            stacked: true,
            gridLines: {
              display: true,
              color: "rgba(255,99,132,0.2)",
            },
          },
        ],
        xAxes: [
          {
            gridLines: {
              display: false,
            },
          },
        ],
      },
      plugins: {
        legend: {
          position: "top",
          labels: {
            fontColor: "#333",
            usePointStyle: true,
          },
        },
      },
    },
  });
}

function runChartLine(label, labels, datas, id = "chartLine") {
  const chart = Chart.getChart(id);
  if (chart !== undefined) chart.destroy();

  console.log(label, labels, datas);

  new Chart("chartLine", {
    type: "line",
    data: {
      labels: labels,
      datasets: [
        {
          label: label,
          data: datas,
          borderColor: ["#3273dc"],
          backgroundColor: ["#3273dc"],
        },
      ],
    },
    options: {
      maintainAspectRatio: false,
      scales: {
        y: {
          stacked: true,
          grid: {
            display: true,
            color: "rgba(255,99,132,0.2)",
          },
        },
        x: {
          grid: {
            display: false,
          },
        },
      },
      plugins: {
        legend: {
          display: false,
        },
      },
    },
  });
}

function runChartLineAdmin(label, labels, datas, id = "chartLine") {
  const chart = Chart.getChart(id);
  if (chart !== undefined) chart.destroy();

  new Chart(id, {
    type: "line",
    data: {
      labels: labels,
      datasets: [
        {
          label: label,
          data: datas,
          borderColor: ["#3273dc"],
          backgroundColor: ["#3273dc"],
        },
      ],
    },
    options: {
      maintainAspectRatio: false,
      animation: false,
      scales: {
        y: {
          stacked: true,
          grid: {
            display: true,
            color: "rgba(255,99,132,0.2)",
          },
        },
        x: {
          display: false,
          grid: {
            display: false,
          },
        },
      },
      plugins: {
        legend: {
          display: false,
        },
      },
    },
  });
}

function chartDoughnut(chartId, labels, datas, colors, position) {
  position = position === undefined ? "right" : position;
  setTimeout(function () {
    const chart = Chart.getChart(chartId);
    if (chart !== undefined) chart.destroy();

    new Chart(chartId, {
      type: "doughnut",
      data: {
        labels: labels,
        datasets: [
          {
            label: "# OKRs",
            data: datas,
            borderWidth: 0,
            backgroundColor: colors.split(","),
          },
        ],
      },
      options: {
        responsive: false,
        plugins: {
          legend: {
            position: position,
            labels: {
              fontColor: "#333",
              usePointStyle: true,
              padding: 10
            },
          },
        },
      },
    });
  }
    , 500);
}

//tagline(true, 'Đây là đoạn nội dung mẫu để kiểm tra giao diện');
function tagline(success, message, override = false) {
  if (!message) return;

  taglineHide();
  const notify = document.createElement("div");
  notify.id = "notify";
  notify.innerHTML = `
    <div class="notification is-${success ? "success" : "danger"}">
      <a class="delete" onclick="taglineHide()"></a>
      <p>${message}</p>
    </div>`;
  document.querySelector("body").appendChild(notify);

  if (success)
    console.warn("[Tagline]" + message);
  else
    console.error("[Tagline]" + message);

  if (!override) {
    setTimeout(function () {
      if (notify !== null) notify.remove();
    }, 8000);
  }
}

function taglineHide() {
  const notify = document.querySelector("#notify");
  if (notify !== null) notify.remove();
}


//tagline(true, 'Đây là đoạn nội dung mẫu để kiểm tra giao diện');
function popupConnect(type, text, isAutoHide = false, success = false) {
  popupHide();
  const connect = document.createElement("div");
  connect.id = "connect";
  connect.classList.add("modal");
  connect.classList.add("is-active");
  if (success) connect.classList.add("is_success");

  connect.innerHTML = `
   <section class="modal-card is-medium">
      <div class="modal-card-body">
      <div class="icon-text">
         <span class="icon">
         ${type == "loading" ?
      '<div class="lds-ring"><div></div><div></div><div></div><div></div></div>'
      : type == "false" ? '<i class="material-icons-outlined">wifi_off</i>'
        : '<i class="material-icons-outlined">check</i>'}
         </span>
         <span>${text}</span>
      </div>
      </div>
   </section>`;

  document.querySelector("body").appendChild(connect);

  if (isAutoHide) {
    setTimeout(function () {
      popupHide();
    }, 3000);
  }
}

function popupHide() {
  const connect = document.querySelector("#connect");
  if (connect !== null) {
    connect.classList.remove("is-active");
    setTimeout(connect.remove(), 1000);
  };
}

function notification(title, content, link) {
  if ("Notification" in window) {
    let ask = Notification.requestPermission();
    ask.then((permission) => {
      if (permission === "granted") {
        let msg = new Notification(title, {
          body: content,
          icon: "/images/favicon.png",
        });
        msg.addEventListener("click", (event) => {
          location.href = link;
        });
      }
    });
  }
}

function stickyHeaderMobile() {
  const stickyNav = function () {
    const header = document.querySelector(".header-sticky");
    if (header == null) return;

    if (window.scrollY > 52) {
      header.classList.add("sticked");
    } else {
      header.classList.remove("sticked");
    }
  };
  stickyNav();
  document.addEventListener("scroll", (e) => {
    stickyNav();
  });
}

//window.addEventListener("keydown", function (e) {
//  if (e.code === "F8") {
//    console.log("F8 = Draft");
//    const btn = this.document.getElementById("btn_draft");
//    if (btn != null) btn.click();
//  } else if (e.code === "F9") {
//    console.log("F9 = Update");
//    const btn = this.document.getElementById("btn_update");
//    if (btn != null) btn.click();
//  } else if (e.code === "F10") {
//    console.log("F10 = Create");
//    const btn = this.document.getElementById("btn_create");
//    if (btn != null) btn.click();
//  } else if (e.code === "Escape") {
//    console.log("Escape = Close");
//    const btn = this.document.getElementById("btn_close");
//    if (btn != null) btn.click();
//  } else if (e.code === "F5") {
//    const btn = this.document.getElementById("btn_refresh");
//    if (btn != null) btn.click();
//  }
//});

function scrollDivX(id, left) {
  setTimeout(function () {
    const element = document.getElementById(id);
    if (element != null) {
      element.scrollTo({
        top: 0,
        left: left,
        behavior: "smooth",
      });
    }
  }, 100);
}

async function TimeList(obj) {
  var dotNetObject = obj;
  var table = document.getElementById("scrollbox");
  if (table == null) return;

  if (!table || !dotNetObject) {
    return;
  }

  var month = 0;

  table.addEventListener("mouseup", UpdatePreMonth);
  table.addEventListener("scroll", UpdatePreMonth);
  function UpdatePreMonth() {
    var list = document.querySelectorAll("#scrollbox .table .month");
    if (list.length == 0) {
      return;
    }

    var value = 0;

    for (let i = 0; i < list.length; i++) {
      let ele = list[i];
      var left = ele.getBoundingClientRect().left - 160 - 100;

      if (left < 0) {
        if (i == list.length - 1) {
          value = parseInt(ele.getAttribute("data-month"));
          break;
        } else {
          continue;
        }
      } else {
        value = parseInt(ele.getAttribute("data-month")) - 1;
        break;
      }
    }

    if (value != month) {
      month = value;
      dotNetObject.invokeMethodAsync("UpdatePreMonthOnScroll", [month]);
    }
  }
}


document.addEventListener("DOMContentLoaded", () => {
  setInterval(checkConnect, 5000);
  if ("Notification" in window) {
    let ask = Notification.requestPermission();
    console.log(ask);
  }
});

function checkConnect() {
  const reconnect = document.querySelector(
    "#components-reconnect-modal button"
  );
  if (reconnect != null) {
    const title = document.querySelector(
      "#components-reconnect-modal h5"
    ).textContent;
    console.warn("Reconnect failed, page reloading...");
    console.log(title);
  } else {
    const error = document.querySelector("#blazor-error-ui");
    if (error != null && error.style.display === "block") {
      console.warn("Server disconnected , page reloading....");
    }
  }
}

function scrollToBottom(id) {
  setTimeout(function () {
    const element = document.getElementById(id);
    if (element != null) {
      element.scrollTop = element.scrollHeight;
      element.behavior = "smooth";
    }
  }, 100);

}

async function initializeMap(lat, long, accessToken, obj) {
  var ele = document.getElementById("map");
  var MapBoxLocation = [];
  var dotNetObject = obj;
  var ll = null;
  mapboxgl.accessToken = accessToken;

  if (!ele)
    return;

  if (lat == null || long == null || (lat == 0 && long == 0)) {
    await window.getLocation().then(res => {
      long = res[0];
      lat = res[1];
    }).finally(() => {
      long = 108.20451311683854; //Default long
      lat = 16.06038501894902; // Default lat
    });
    MapBoxLocation = [long, lat];
    callbackToDotNet();
  }
  ll = new mapboxgl.LngLat(long, lat);

  let map = new mapboxgl.Map({
    container: "map", // container id
    style: "mapbox://styles/mapbox/streets-v12",
    center: ll, // starting position
    zoom: 15, // starting zoom
    doubleClickZoom: false,
  });

  let marker = new mapboxgl.Marker({
    draggable: true,
  });

  marker.setLngLat(ll);
  marker.addTo(map);
  marker.on("dragend", onDragEnd);

  function onDragEnd() {
    const lngLat = marker.getLngLat();
    MapBoxLocation = [lngLat.lng, lngLat.lat];
    callbackToDotNet();
  }

  map.on("dblclick", e => {
    ll = new mapboxgl.LngLat(e.lngLat.lng, e.lngLat.lat);
    marker.setLngLat(ll);
    MapBoxLocation = [e.lngLat.lat, e.lngLat.lng];
    callbackToDotNet();
  })

  function callbackToDotNet() {
    dotNetObject.invokeMethodAsync('SetLocation', MapBoxLocation);
  }
}

async function getLocationMapBox() {
  var coords = {
    latitude: 0,
    longitude: 0,
  }

  return [coords.latitude, coords.longitude];
}

async function InitialMention(obj, list, id) {
  if (obj == null) return;

  var dotNet = obj;
  var input = document.getElementById(id);
  var send;
  let timer;

  var selected = [];

  if (input == null) return;

  send = input.nextElementSibling;
  const processChange = debounce((option, content) => updateToDotNet(option, content));

  textAutoSize(input);

  var component = new Mention({
    input: document.getElementById(id),
    button: send,
    options: list,
    update: (option, content) => {
      if (option != null) {
        if (selected.length == 0)
          selected.push(option);
        else {
          if (selected.filter(x => x.id_string == option.id_string).length == 0) {
            selected.push(option);
          }
        }
      }
      processChange(option, content);
    },
    send: (empty) => {
      if (!empty) {
        selected.length = 0;
        input.value = "";
      };
    },
    template: function (option) {
      return `<div class="icon-text">
                    <span class="image is-rounded is-24x24">
                      <img src="${option.icon}" alt="AVT">
                    </span>
                    <span class="ml-2" style="flex-grow: 1">${option.name}</span>
                  </div>`;
    },
  })

  function updateToDotNet(option, content) {
    var list = [];

    if (selected.length > 0) {
      selected.forEach(x => {
        var name = x.name;
        var exist = false;
        while (name != "") {
          exist = content.search(name);
          if (exist == false) {
            if (name != "") name = name.substring(0, name.lastIndexOf(" "));
          }
          else {
            list.push(x);
            break;
          }
        }
      })
    }

    dotNet.invokeMethodAsync("SetMentions", list, content);
  }

  function debounce(func, timeout = 300) {
    let timer;
    return (...args) => {
      clearTimeout(timer);
      timer = setTimeout(() => { func.apply(this, args); }, timeout);
    };
  }
}

function clearTextArea(id) {
  let ele = document.getElementById(id);
  if (!ele) return;

  ele.value = "";
}

function ToggleOverflowHtml(over = false) {
  var html = document.documentElement;

  if (html == null) return;

  if (over == false)
    html.style.overflow = "";
  else
    html.style.overflow = "hidden";
}

function CustomDatepicker(obj, id) {
  if (!obj) return;
  if (!id) return;

  let ele = document.querySelector(`.custom_datepicker.${id}`);
  let menu = ele.querySelector(".custom_datepicker_calendar");

  if (!ele) return;

  var method = SetDropdownPosition(`.custom_datepicker.${id}`, ".custom_datepicker_calendar");

  if (!method) return;

  document.body.addEventListener("click", Close);

  function Close(e) {
    var target = e.target;

    if (target != ele && !ele.contains(target) && menu != ele && !menu.contains(target)) {
      try {
        obj.invokeMethodAsync("Close");
        obj.destroy();
        document.body.removeEventListener("click", Close);
      }
      catch {
        document.body.removeEventListener("click", Close);
      }
    }
  }
}

function SetDropdownPosition(id, menuSelector, obj = null, method = null, isSetLeft = false) {
  let ele = document.querySelector(id);
  if (!ele) return false;

  let menu = ele.querySelector(menuSelector);
  if (!menu) return false;

  var eleRec = ele.getBoundingClientRect();
  let menuRec = menu.getBoundingClientRect();
  let screenWidth = Math.round(document.body.offsetWidth, 0);
  let screenHeight = Math.round(document.body.offsetHeight, 0);

  // Nếu dropdown menu tràn màng hình phải
  if (Math.round(menuRec.right, 0) > screenWidth) {
    let offsetLeft = Math.round(Math.round(menuRec.right, 0) - screenWidth, 0);

    menu.style.left = -offsetLeft + "px";
  }

  // Nếu dropdown menu tràn màn hình trái
  if (Math.round(menuRec.x, 0) < 0) {
    menu.style.right = "auto";
    menu.style.left = Math.abs(Math.round(menuRec.x, 0)) + "px";
  }


  // Nếu dropdown tràn bên dưới
  if (Math.round(menuRec.bottom, 0) > screenHeight - 24) {
    // Nếu không đủ không gian bên trên, đổ xuống dưới
    if (ele.getBoundingClientRect().top < Math.round(menuRec.height, 0)) {
      var offset = ele.getBoundingClientRect().top + Math.round(menuRec.height, 0) - screenHeight;
      menu.style.top = -offset + "px";
      menu.style.bottom = "";
      // Resize dropdown menu nếu tràn dưới
      var bottom = screenHeight - Math.round(ele.getBoundingClientRect().bottom, 0) - 16;
      if (bottom < Math.round(menuRec.height, 0)) {
        menu.style.height = Math.round(bottom - 36, 0) + "px";
      }
    }
    else {
      menu.style.top = "auto";
      menu.style.bottom = "calc(100% + 16px)";
    }
  }

  if (isSetLeft) {
    var content = menu.querySelector(".dropdown-content");
    if (content) {
      content.style.left = Math.round(eleRec.x - 100, 0) + "px";
    }
  }

  if (obj && method) {
    // Đóng menu khi click ra ngoài
    CloseOutsideDropdown(obj, method, ele.id);
  }

  return true;
}

function CloseOutsideDropdown(obj, nameFuction, idDropdown) {
  if (!obj) return;

  let ele = document.getElementById(idDropdown);
  if (idDropdown == null) return;

  document.body.addEventListener("click", Close);

  function Close(e) {
    var target = e.target;

    if (target != ele && !ele.contains(target)) {
      obj.invokeMethodAsync(nameFuction);

      document.body.removeEventListener("click", Close);
    }
  }
}

function TodoCalendarPopup(date, popupCalendar, animation) {
  if (!date) return;


  let container = document.getElementById(date);
  let popup = document.getElementById(popupCalendar);

  if (!container || !popup) return;

  popup.classList.remove("is_slidein");

  let containerRec = container.getBoundingClientRect();
  let popupRec = popup.getBoundingClientRect();

  let width = popupRec.width;
  let height = popupRec.height;

  let screenWidth = document.body.offsetWidth;
  let screenHeight = document.body.offsetHeight;

  if (containerRec.right + width > screenWidth) {
    popup.style.right = "calc(100% + 4px)";
    popup.style.left = "auto";
  }
  else {
    popup.style.left = "calc(100% + 4px)";
    popup.style.right = "auto";
  }

  if (containerRec.y + height > screenHeight) {
    popup.style.top = "auto";
    popup.style.bottom = 0;
  }
  else {
    popup.style.top = 0;
    popup.style.bottom = "auto";
  }

  setTimeout(() => {
    popup.style.opacity = 1;
    popup.classList.add("is_slidein");
  }, 100);
}


function LineAnimateTabs() {
  const tabs = document.querySelectorAll(".tab-item-customize");
  const tabActive = document.querySelector(".tab-item-customize.is-active");
  const line = document.querySelector(".tabs-customize .line");
  requestIdleCallback(function () {
    line.style.left = tabActive.offsetLeft + "px";
    line.style.width = tabActive.offsetWidth + "px";
  });
  tabs.forEach((tab, index) => {
    tab.onclick = function () {
      document.querySelector(".tab-item-customize.is-active").classList.remove("is-active");

      line.style.left = this.offsetLeft + "px";
      line.style.width = this.offsetWidth + "px";

      this.classList.add("active");
    };
  });
}

function NavigateSuggestion(list, current, wrapper = null, obj) {
  if (list == null || list.length == 0 || !wrapper)
    return;

  var container = wrapper.parentElement;
  var newCurrent = current;

  function KeydownEvent(key) {
    if (key.keyCode == 27 || key.code === "Escape") {
      var ele = document.querySelector(".header_main .search_bar .input");
      if (ele)
        ele.blur();
      return;
    }

    if (key.keyCode === 38 || key.code === "ArrowUp") {
      if (!current)
        newCurrent = list[list.length - 1];
      else {
        var index = list.indexOf(current);
        if (index < 0)
          newCurrent = list[0];

        newCurrent = list[index == 0 ? list.length - 1 : index - 1];
      }
    }
    if (key.keyCode == 40 || key.code === "ArrowDown") {
      if (!current) {
        newCurrent = list[0];
      }
      else {
        var index = list.indexOf(current);
        if (index < 0)
          newCurrent = list[0];
        else
          newCurrent = list[index == list.length - 1 ? 0 : index + 1];
      }
    }
    current = newCurrent;

    var height = wrapper.offsetHeight;
    var currentScroll = wrapper.scrollTop;
    var active = wrapper.querySelector(`[data-value="${newCurrent}"]`);

    if (!active)
      return;

    // Active current Item
    wrapper.querySelectorAll(".rsitem").forEach(x => x.classList.remove("seleted"));
    active.classList.add("seleted");

    // Scroll to current Item
    var offsetTop = active.offsetTop;
    var activeHeight = active.offsetHeight;

    if ((offsetTop + activeHeight) > (height + currentScroll)) {
      wrapper.scrollTop = offsetTop - height + activeHeight;
    }
    else if (offsetTop < currentScroll) {
      wrapper.scrollTop = offsetTop;
    }

    // callback on Enter
    if (key.keyCode == 13 || key.code === "Enter") {
      obj.invokeMethodAsync("JsCallback", newCurrent);
      container.removeEventListener("keydown", KeydownEvent);
    }
  }

  if (container) {
    // Add Keydown handler
    if (container.getAttribute("listener") != "true") {
      container.addEventListener("keydown", KeydownEvent);
    }
  }
}

const titleCount = {
  character: 0,
  msg: "",
  currentTitle: "",
  counter: () => {
    let msg = titleCount.msg;
    if (!msg || !titleCount.currentTitle) {
      return;
    }
    let split = msg.split(" ");
    let character = titleCount.character;
    let currentStep = 0;
    titleCount.timer = setInterval(function () {
      document.title = msg.substring(character, msg.length) + msg.substring(0, character);
      if (currentStep < split.length) {
        character += split[currentStep].length + 1;
        currentStep++;
      }
      else {
        character += 2;
      }
      if (character > msg.length) {
        character = 0;
        currentStep = 0;
      }
    }, 100);
  },
  stop: () => {
    if ("timer" in titleCount)
      clearInterval(titleCount.timer);
  }
}

function AnimationTitle(count, name) {
  if (count == 0)
    return;

  var msg = `👋 (${count}) thông báo mới ${name ? "từ " + name : ""}!`;
  if (msg.trim() == titleCount.msg.trim())
    return;

  titleCount.character = 0;
  titleCount.msg = msg + "  ";
  titleCount.currentTitle = document.title;
  titleCount.stop();
}
function addCustomDragImage(cont, wrapper) {
  if (!cont || !wrapper) return;

  var container = document.querySelectorAll(`.${cont.split(" ").join(".")}`);

  if (container.length == 0) return;

  [].forEach.call(container, item => {
    var className = wrapper
      .replace(/\s+/g, ' ').trim()
      .split(" ").join(".");

    var parent = container[0].closest(".drag-scroll");
    var rec = parent ? parent.getBoundingClientRect() : null;
    var eles = item.querySelectorAll(`.${className}:not(.plk-dd-noselect)`);
    var up = false;
    var down = false;

    if (parent) {
      parent.setAttribute("data-scrolling", "false");
    }
    if (eles.length > 0) {
      [].forEach.call(eles, ele => {
        var width = ele.offsetWidth;
        var height = ele.offsetHeight;

        function OnDragStart(e) {
          if (document.getElementById("drag_ghost"))
            document.getElementById("drag_ghost").remove();

          let crt = this.cloneNode(true);

          crt.id = "drag_ghost";
          crt.style.transform = "rotate(-0.5deg)";
          crt.style.width = `calc(${width}px + 1rem)`;
          crt.style.height = `calc(${height}px + 1rem)`;

          crt.style.left = "-100%";
          crt.style.top = "-100%";

          document.body.appendChild(crt);
          e.dataTransfer.setDragImage(new Image(), 0, 0);
        }

        function OnDrag(e) {
          if (document.getElementById("drag_ghost")) {
            let crt = document.getElementById("drag_ghost");

            var y = e.clientY;
            crt.style.left = e.pageX - 20 + "px";
            crt.style.top = e.pageY + 10 + "px";
            if (parent) {
              if (y <= rec.bottom && y >= rec.bottom - 40) {
                if (parent.getAttribute("data-scrolling") == "false") {
                  parent.setAttribute("data-scrolling", "true");
                  down = true;
                  up = false;
                  ScrollDown();
                }
              }
              else if (y >= rec.y && y <= rec.y + 40) {
                if (parent.getAttribute("data-scrolling") == "false") {
                  parent.setAttribute("data-scrolling", "true");
                  down = false;
                  up = true;
                  ScrollUp();
                }
              }
              else {
                parent.setAttribute("data-scrolling", "false");
                down = false;
                up = false;
              }
            }
          }
        }

        function ScrollDown() {
          parent.scrollTop += 20;
          if (parent.scrollTop < parent.scrollHeight && down) {
            setTimeout(() => {
              ScrollDown();
            }, 50);
          }
        }
        function ScrollUp() {
          parent.scrollTop -= 20;
          if (parent.scrollTop > 0 && up) {
            setTimeout(() => {
              ScrollUp();
            }, 50);
          }
        }

        function OnDragEnd() {
          if (document.getElementById("drag_ghost"))
            document.getElementById("drag_ghost").remove();
        }

        ele.removeEventListener("dragstart", OnDragStart, false);
        ele.removeEventListener("drag", OnDrag, false);
        ele.removeEventListener("dragend", OnDragEnd, false);

        ele.addEventListener("dragstart", OnDragStart, false);
        ele.addEventListener("drag", OnDrag, false);
        ele.addEventListener("dragend", OnDragEnd, false);
      })
    }
  });
}

function SetLayoutReportPosition(idReport, idChartCheck, checkList) {
  let ele = document.querySelector(idReport);
  if (!ele) return false;

  let menu = ele.querySelector(idChartCheck);
  if (!menu) return false;

  let list = ele.querySelector(checkList);
  if (!list) return false;

  let menuRec = menu.getBoundingClientRect();
  let listRec = list.getBoundingClientRect();
  let screenHeight = Math.round(document.body.offsetHeight, 0);


  // Nếu layout tràn bên dưới
  if (Math.round(menuRec.bottom, 0) > screenHeight - 24 || Math.round(listRec.bottom, 0) > screenHeight - 24 ) {
    if (ele.classList.contains("report-layout-6"))
      ele.classList.remove("report-layout-6");
    ele.classList.add("report-layout-4");
  }
  else {
    if (ele.classList.contains("report-layout-4"))
      ele.classList.remove("report-layout-4");
    ele.classList.add("report-layout-6");
  }

  return true;
}