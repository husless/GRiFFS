$(document).ready(function () {
    $("#txtUserName").focus();
    $("#btnLogin").click(function () { doLogin(); });
    $("#ImgRandom").click(function () { changeValidateCode(); });

});

function doLogin() {
    var username = $.trim($("#txtUserName").val());
    var pwd = $.trim($("#txtPassword").val());
    var validateCode = $.trim($("#txtValidateCode").val());
    if (username == "" || username == null || username == undefined) { alert("请输入用户名"); $("#txtUserName").focus(); return false; }
    if (pwd == "" || pwd == null || pwd == undefined) { alert("请输入密码"); $("#txtPassword").focus(); return false; }


    $.ajax({
        url: 'WinderHandler.ashx?act=login', dataType: 'json', type: 'post',
        data: { 'uname': username, 'pwd': pwd },
        success: function (data) {
            if (data.flag === "NO") { alert(data.msg); return false; }
            else if (data.flag === "YES") { window.location = "main.html" };
        }
    });
}

//更换验证码
function changeValidateCode() {
    var imgRandom = $("#ImgRandom");
    imgRandom.attr("src", imgRandom.attr("src") + "?");
}