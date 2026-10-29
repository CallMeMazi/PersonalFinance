(function () {
  function toEnglishDigits(value) {
    return String(value || "")
      .replace(/[۰-۹]/g, function (d) { return "۰۱۲۳۴۵۶۷۸۹".indexOf(d); })
      .replace(/[٠-٩]/g, function (d) { return "٠١٢٣٤٥٦٧٨٩".indexOf(d); })
      .replace(/٫/g, ".");
  }

  document.querySelectorAll("form").forEach(function (form) {
    form.addEventListener("submit", function () {
      form.querySelectorAll("input, textarea").forEach(function (input) {
        if (input.type === "hidden" || input.type === "checkbox" || input.type === "color") {
          return;
        }
        input.value = toEnglishDigits(input.value);
      });
    });
  });

  document.querySelectorAll(".js-confirm-delete").forEach(function (form) {
    form.addEventListener("submit", function (event) {
      var message = form.getAttribute("data-message") || "این مورد حذف شود؟";
      if (!window.confirm(message)) {
        event.preventDefault();
      }
    });
  });

  if (!window.Chart) {
    return;
  }

  Chart.defaults.color = "#93a0c0";
  Chart.defaults.borderColor = "rgba(148, 163, 184, 0.12)";
  Chart.defaults.font.family = "IRANSans, Tahoma, sans-serif";

  function parseJson(id) {
    var el = document.getElementById(id);
    if (!el) {
      return [];
    }
    try {
      return JSON.parse(el.textContent || "[]");
    } catch (e) {
      return [];
    }
  }

  var trendCanvas = document.getElementById("trendChart");
  if (trendCanvas) {
    new Chart(trendCanvas, {
      type: "line",
      data: {
        labels: parseJson("trendLabels"),
        datasets: [
          {
            label: "درآمد",
            data: parseJson("trendIncome"),
            borderColor: "#2ee6a6",
            backgroundColor: "rgba(46, 230, 166, 0.18)",
            tension: 0.35,
            fill: true
          },
          {
            label: "هزینه",
            data: parseJson("trendExpense"),
            borderColor: "#ff6b8a",
            backgroundColor: "rgba(255, 107, 138, 0.12)",
            tension: 0.35,
            fill: true
          }
        ]
      },
      options: {
        responsive: true,
        maintainAspectRatio: false,
        plugins: { legend: { labels: { usePointStyle: true } } },
        scales: { y: { beginAtZero: true } }
      }
    });
  }

  var pieCanvas = document.getElementById("categoryChart");
  if (pieCanvas) {
    var values = parseJson("categoryValues");
    new Chart(pieCanvas, {
      type: "doughnut",
      data: {
        labels: parseJson("categoryLabels"),
        datasets: [{
          data: values,
          backgroundColor: parseJson("categoryColors"),
          borderWidth: 0
        }]
      },
      options: {
        responsive: true,
        maintainAspectRatio: false,
        plugins: {
          legend: { position: "bottom" }
        },
        cutout: "62%"
      }
    });
  }
})();
