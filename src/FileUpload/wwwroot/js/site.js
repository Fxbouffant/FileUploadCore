// Write your Javascript code.
function uploadFiles(inputId) {
    var input = document.getElementById(inputId);
    var files = input.files;
    var formData = new FormData();

    for (var i = 0; i !== files.length; i++) {
        formData.append("files", files[i]);
    }

    startUpdatingProgressIndicator();
    $.ajax(
      {
          url: "/Home",
          data: formData,
          processData: false,
          contentType: false,
          type: "POST",
          success: function (data) {
              clearInterval(intervalId);
              $("#progress").hide();
              $("#upload-status").show();
          }
      }
    );
}

var intervalId;

function startUpdatingProgressIndicator() {
    $("#progress").show();
    $("#upload-status").hide();

    intervalId = setInterval(
      function () {
          $.post(
            "/Home/Progress",
            function (progress) {
                $(".progress-bar").css("width", progress + "%").attr("aria-valuenow", progress);
                $(".progress-bar").html(progress + "%");
            }
          );
      },
      1000
    );
}