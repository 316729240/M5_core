using M5.Common;
using Microsoft.AspNetCore.Mvc;
using MWMS;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace M5.Main.Manager
{
    [Route("manage/app/[controller]/[action]")]
    public class ManagerBase : Controller
    {
        public LoginInfo loginInfo = null;
        public  ManagerBase()
        {
            string sessionId = PageContext.Current.Request.Cookies["M5_SessionId"];
            loginInfo = new LoginInfo(sessionId);
        }
        public ReturnValue cardPermissions()
        {
            return new ReturnValue(loginInfo.value.isAdministrator);
        }
    }
 
}
