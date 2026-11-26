(function () {
    var compose = document.getElementById("compose");
    if (compose) {
        compose.onkeydown = function (event) {
            event = event || window.event;
            if (event.keyCode === 13 && !event.shiftKey && !event.isComposing && /\S/.test(compose.value)) {
                if (event.preventDefault) event.preventDefault();
                compose.form.submit();
                return false;
            }
        };
    }
    var thread = document.getElementById("thread");
    if (!thread || !window.XMLHttpRequest || !window.JSON) return;
    var busy = false;
    window.setInterval(function () {
        if (busy || document.hidden || (compose && compose.value)) return;
        var request = new XMLHttpRequest();
        request.open("GET", thread.getAttribute("data-check"), true);
        request.onreadystatechange = function () {
            if (request.readyState !== 4) return;
            busy = false;
            if (request.status !== 200 || (compose && compose.value)) return;
            try {
                var last = JSON.parse(request.responseText).id;
                if (typeof last === "number" && last !== Number(thread.getAttribute("data-last"))) window.location.reload();
            } catch (ignore) {}
        };
        busy = true;
        request.send(null);
    }, 10000);
}());
