mergeInto(LibraryManager.library, {
  SetEventListenerGameObject: function (eventListenerGameObject) {
    window.web3gl.eventListenerGameObject = UTF8ToString(eventListenerGameObject);
  },

  SendRequest: function(json) {
    window.web3gl.handleRequest(JSON.parse(UTF8ToString(json)));
  }
});
