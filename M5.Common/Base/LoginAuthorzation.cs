
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Mvc.Filters;
using MWMS;
using System;

namespace M5.Common
{
    public class LoginAuthorzation : Attribute, IAuthorizationFilter
    {
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            string sessionId = context.HttpContext.Request.Cookies["M5_SessionId"];
            LoginInfo loginInfo = new LoginInfo(sessionId);
            //context.HttpContext.SetLoginInfo(loginInfo);
            if (!loginInfo.checkManagerLogin()) { 
                context.Result = new JsonResult(ReturnValue.Err("没有登录",-1000));
            }
        }


    }
 
}
