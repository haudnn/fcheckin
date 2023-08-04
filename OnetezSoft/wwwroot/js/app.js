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
function setCookie(key, value) {
	let expires = new Date();
	expires.setTime(expires.getTime() + 1 * 24 * 60 * 60 * 1000);
	document.cookie =
			key + "=" + value + ";expires=" + expires.toUTCString() + ";path=/";
}

function setCookieDevice(key, value) {
document.cookie = key + "=" + value + ";path=/";
}

function getCookie(key) {
	var keyValue = document.cookie.match("(^|;) ?" + key + "=([^;]*)(;|$)");
	return keyValue ? keyValue[2] : null;
}

function deleteCookie(key) {
	document.cookie = key + "=;expires=Thu, 01 Jan 1970 00:00:00;path=/";
}

window.SetFocusToElement = (element) => {
	element.focus();
};

window.getUserAgent = () => {
	return navigator.userAgent;
};

window.getDeviceName = function () {
	return navigator.userAgent;
};

window.copyToClipboard = function (text) {
navigator.clipboard.writeText(text).then(function () {
				console.log("Text copied to clipboard");
}).catch(function (error) {
				console.error("Failed to copy text: ", error);
});
}

window.getIpAddress = () => {
	return fetch("https://api.ipify.org/")
			.then((response) => response.text())
			.then((data) => {
					return data;
			});
};

window.getLocation = () => {
	return getLocationPromise()
			.then((res) => {
					// If promise get resolved
					const { coords } = res;
					return [coords.latitude, coords.longitude];
			})
			.catch((error) => {
					return null;
			});
};

let getLocationPromise = () => {
	return new Promise(function (resolve, reject) {
			// Promisifying the geolocation API
			navigator.geolocation.getCurrentPosition(
					(position) => resolve(position),
					(error) => reject(error)
			);
	});
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
			if (element == null)
					return;

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
	
	if(success)
			console.warn("[Tagline]" + message);
	else
			console.error("[Tagline]" + message);
	
	setTimeout(function () {
			if (notify !== null) notify.remove();
	}, 8000);
}

function taglineHide() {
	const notify = document.querySelector("#notify");
	if (notify !== null) notify.remove();
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
	} else if (e.code === "F5") {
			const btn = this.document.getElementById("btn_refresh");
			if (btn != null) btn.click();
	}
});

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
const reconnect = document.querySelector("#components-reconnect-modal button");
if (reconnect != null)
{
		const title = document.querySelector("#components-reconnect-modal h5").textContent;
		console.warn("Reconnect failed, page reloading...");
		console.log(title);
		//if(title.search('3') > 0) location.reload();
}
else
{
		const error = document.querySelector("#blazor-error-ui");
		if (error != null && error.style.display === "block") {
				console.warn("Server disconnected , page reloading....");
				//location.reload();
		}
}
}