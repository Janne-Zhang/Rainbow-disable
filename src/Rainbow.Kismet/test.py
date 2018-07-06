import clr

app.default(role="all", cache=300)

@app.action(name="login")
def action_login(context):
    #请求
    username = context.request.username
    password = context.request.password

    #返回
    context.response.content("test", "utf-8")
    context.response.status(200)
    context.response.structure(obj, "json")

    #下载
    context.response.download("文件路径", "[文件名]")
    context.response.download("文件流(stream)", "[文件名]")
    

    #跳转
    context.redirect("")

    #服务注入
    txt = context.service.testservice

    #session
    context.session
    #cookie
    context.cookie
    

@app.action(name="logout")
def action_logout(context):
    log = context.service.logservice
    lang = context.service.langservice

    lang.text("");
    log.print("");
