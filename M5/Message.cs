using MWMS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Threading.Tasks;
using System.Xml;

namespace M5.Common
{
    public class Message
    {
        public static ReturnValue sendMail(string StrTo, string strSubject, string StrBody)
        {

            ReturnValue err = new ReturnValue();
            XmlNodeList root = Config.userConfig["mail"];
            if (root != null)
            {
                if (root.Count == 4)
                {
                    string strFrom = root[0].InnerText;
                    string strUserName = root[1].InnerText;
                    string strPwd = root[2].InnerText;
                    string strServer = root[3].InnerText;

                    MailMessage mailMessage = new MailMessage();
                    mailMessage.To.Add(StrTo);
                    mailMessage.From = new System.Net.Mail.MailAddress(strFrom);
                    mailMessage.Subject = strSubject;
                    mailMessage.Body = StrBody;
                    mailMessage.IsBodyHtml = true;
                    // mailMessage.en
                    //mailMessage.BodyEncoding = System.Text.Encoding.GetEncoding("GBK");
                    mailMessage.Priority = System.Net.Mail.MailPriority.Normal;

                    SmtpClient smtpClient = new SmtpClient();
                    smtpClient.Credentials = new System.Net.NetworkCredential(strUserName, strPwd);//设置发件人身份的票据  
                    smtpClient.DeliveryMethod = System.Net.Mail.SmtpDeliveryMethod.Network;
                    smtpClient.Host = strServer;
                    smtpClient.Send(mailMessage);

                }
                else
                {
                    err.errNo = -1;
                    err.errMsg = "系统邮箱设置不正确";
                    //                    throw new NullReferenceException("系统邮箱设置不正确");
                }
            }
            else
            {
                err.errNo = -1;
                err.errMsg = "没有找到邮箱配置文件";
                //throw new NullReferenceException("没有找到邮箱配置文件");
            }
            return err;
        }
    }
}
