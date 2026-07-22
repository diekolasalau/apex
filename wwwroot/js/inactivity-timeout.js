window.screenInactivity = (function () {
  let timeoutId = null;
  let dotNetRef = null;
  let timeoutMs = 300000;
  let active = false;

  const activityEvents = [
    "mousemove",
    "mousedown",
    "keydown",
    "scroll",
    "touchstart",
    "click"
  ];

  function clearTimer() {
    if (timeoutId) {
      clearTimeout(timeoutId);
      timeoutId = null;
    }
  }

  function startTimer() {
    clearTimer();

    if (!active || !dotNetRef) {
      return;
    }

    timeoutId = setTimeout(() => {
      if (dotNetRef) {
        dotNetRef.invokeMethodAsync("HandleInactivityTimeout");
      }
    }, timeoutMs);
  }

  function onActivity() {
    startTimer();
  }

  function addListeners() {
    activityEvents.forEach((eventName) => {
      window.addEventListener(eventName, onActivity, true);
    });
  }

  function removeListeners() {
    activityEvents.forEach((eventName) => {
      window.removeEventListener(eventName, onActivity, true);
    });
  }

  return {
    register: function (ref, milliseconds) {
      dotNetRef = ref;
      timeoutMs = typeof milliseconds === "number" && milliseconds > 0 ? milliseconds : 300000;
      active = true;
      addListeners();
      startTimer();
    },

    unregister: function () {
      active = false;
      clearTimer();
      removeListeners();
      dotNetRef = null;
    }
  };
})();
