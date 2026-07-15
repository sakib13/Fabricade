mergeInto(LibraryManager.library, {
  WebGLPostJson: function (urlPtr, jsonPtr) {
    var url = UTF8ToString(urlPtr);
    var json = UTF8ToString(jsonPtr);
    fetch(url, {
      method: "POST",
      headers: { "Content-Type": "text/plain" },
      body: json,
      redirect: "follow"
    }).then(function (response) {
      console.log("Fabricade log upload: " + response.status);
    }).catch(function (error) {
      console.error("Fabricade log upload failed: " + error);
    });
  }
});
