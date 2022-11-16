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
  "##f14668",
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

window.SetFocusToElement = (element) => {
  element.focus();
};

window.getUserAgent = () => {
  return navigator.userAgent;
};

function setFocus(id) {
  setTimeout(function () {
    const element = document.getElementById(id);
    if (element != null) element.focus();
  }, 100);
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

function toggleText(item) {
  if (item.className.indexOf("is-show") === -1) item.classList.add("is-show");
  else item.classList.remove("is-show");
}

function textAutoSize(id) {
  setTimeout(function () {
    const text = document.getElementById(id);
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

function runChartLine(label, labels, datas) {
  const chart = Chart.getChart("chartLine");
  if (chart !== undefined) chart.destroy();

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

function chartDoughnut(chartId, labels, datas, colors) {
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
            position: "right",
            labels: {
              fontColor: "#333",
              usePointStyle: true,
            },
          },
        },
      },
    });
  }, 500);
}

var slideIndex = 0;
function showSlides(n) {
  slideIndex = n;
  var dots = document.getElementsByClassName("dot");
  var slides = document.getElementsByClassName("mySlides");
  for (let i = 0; i < slides.length; i++) {
    slides[i].style.display = "none";
    dots[i].className = dots[i].className.replace(" active", "");
  }
  slideIndex++;
  if (slideIndex > slides.length) {
    slideIndex = 1;
  }
  slides[slideIndex - 1].style.display = "block";
  dots[slideIndex - 1].className += " active";
  //setTimeout(showSlides, 10000);
}

function currentSlide(n) {
  var dots = document.getElementsByClassName("dot");
  var slides = document.getElementsByClassName("mySlides");
  for (let i = 0; i < slides.length; i++) {
    slides[i].style.display = "none";
    dots[i].className = dots[i].className.replace(" active", "");
  }
  slideIndex++;
  if (n > slides.length) {
    n = 1;
  }
  slides[n - 1].style.display = "block";
  dots[n - 1].className += " active";
}

//tagline(true, 'Đây là đoạn nội dung mẫu để kiểm tra giao diện');
function tagline(success, message) {
  taglineHide();
  const notify = document.createElement("div");
  notify.id = "notify";
  notify.innerHTML = `
    <div class="notification is-${success ? "success" : "danger"}">
      <a class="delete" onclick="taglineHide()"></a>
      <p>${message}</p>
    </div>`;
  document.querySelector("body").appendChild(notify);
  setTimeout(function () {
    if (notify !== null) notify.remove();
  }, 5000);
}

function taglineHide() {
  const notify = document.querySelector("#notify");
  if (notify !== null) notify.remove();
}

//notification("Workdo", "Đây là đoạn nội dung mẫu để kiểm tra giao diện", '/reports')
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

document.addEventListener("DOMContentLoaded", () => {
  if ("Notification" in window) {
    let ask = Notification.requestPermission();
    console.log(ask);
  }
});

window.addEventListener("keydown", function (e) {
  if (e.code === "F8") {
    console.log("F8 = Draft");
    const btn = this.document.getElementById("btn_draft");
    if (btn != null) btn.click();
  } else if (e.code === "F9") {
    console.log("F9 = Update");
    const btn = this.document.getElementById("btn_update");
    if (btn != null) btn.click();
  } else if (e.code === "F10") {
    console.log("F10 = Create");
    const btn = this.document.getElementById("btn_create");
    if (btn != null) btn.click();
  } else if (e.code === "Escape") {
    console.log("Escape = Close");
    const btn = this.document.getElementById("btn_close");
    if (btn != null) btn.click();
  }
});
