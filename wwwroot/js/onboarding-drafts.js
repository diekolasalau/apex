window.onboardingDrafts = {
  save: function (key, json) {
    if (!key) {
      return;
    }

    localStorage.setItem(key, json || "");
  },

  load: function (key) {
    if (!key) {
      return null;
    }

    return localStorage.getItem(key);
  },

  clear: function (key) {
    if (!key) {
      return;
    }

    localStorage.removeItem(key);
  }
};
