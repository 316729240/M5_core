using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace MWMS.Template
{
    /// <summary>
    /// 域名映射
    /// </summary>
    public class UrlMapping
    {
        public string Path = "";
        public string PcDomain = "";
        public string MobileDomain = "";
    }
    /// <summary>
    /// 当前请求地址信息
    /// </summary>
    public class RequestUrl
    {
        /// <summary>
        /// 实际路径
        /// </summary>
        public string RealPath = "";
        /// <summary>
        /// 当前访问地址
        /// </summary>
        public string AccessUrl = "";
        /// <summary>
        /// 手机访问地址
        /// </summary>
        public string MobileUrl = "";
        /// <summary>
        /// 是否为手机浏览器访问
        /// </summary>
        public bool IsMobileBrowser = false;
        /// <summary>
        /// 当前访问地址是否为手机地址
        /// </summary>
        public bool IsMobileUrl = false;

    }
    public class WebUri
    {
        /// <summary>
        /// 虚拟路径
        /// </summary>
        string virtualPath = @"/";
        /// <summary>
        /// 是否为手机访问
        /// </summary>
        bool IsMobile = false;
        /// <summary>
        /// 自定义手机地址
        /// </summary>
        public string MainMobileUrl = "";
        /// <summary>
        /// 是否手机地址
        /// </summary>
        public bool IsMobileUrl = false;
        List<UrlMapping> domainList = new List<UrlMapping>();
        string NewUrl = "";
        /// <summary>
        /// 是否手机访问
        /// </summary>
        bool IsMobileAccess(string agent)
        {
            string u = agent;
            Regex b = new Regex(@"android.+mobile|avantgo|bada\/|blackberry|blazer|compal|elaine|fennec|hiptop|iemobile|ip(hone|od)|iris|kindle|lge |maemo|midp|mmp|netfront|opera m(ob|in)i|palm( os)?|phone|p(ixi|re)\/|plucker|pocket|psp|symbian|treo|up\.(browser|link)|vodafone|wap|windows (ce|phone)|xda|xiino", RegexOptions.IgnoreCase | RegexOptions.Multiline);
            Regex v = new Regex(@"1207|6310|6590|3gso|4thp|50[1-6]i|770s|802s|a wa|abac|ac(er|oo|s\-)|ai(ko|rn)|al(av|ca|co)|amoi|an(ex|ny|yw)|aptu|ar(ch|go)|as(te|us)|attw|au(di|\-m|r |s )|avan|be(ck|ll|nq)|bi(lb|rd)|bl(ac|az)|br(e|v)w|bumb|bw\-(n|u)|c55\/|capi|ccwa|cdm\-|cell|chtm|cldc|cmd\-|co(mp|nd)|craw|da(it|ll|ng)|dbte|dc\-s|devi|dica|dmob|do(c|p)o|ds(12|\-d)|el(49|ai)|em(l2|ul)|er(ic|k0)|esl8|ez([4-7]0|os|wa|ze)|fetc|fly(\-|_)|g1 u|g560|gene|gf\-5|g\-mo|go(\.w|od)|gr(ad|un)|haie|hcit|hd\-(m|p|t)|hei\-|hi(pt|ta)|hp( i|ip)|hs\-c|ht(c(\-| |_|a|g|p|s|t)|tp)|hu(aw|tc)|i\-(20|go|ma)|i230|iac( |\-|\/)|ibro|idea|ig01|ikom|im1k|inno|ipaq|iris|ja(t|v)a|jbro|jemu|jigs|kddi|keji|kgt( |\/)|klon|kpt |kwc\-|kyo(c|k)|le(no|xi)|lg( g|\/(k|l|u)|50|54|\-[a-w])|libw|lynx|m1\-w|m3ga|m50\/|ma(te|ui|xo)|mc(01|21|ca)|m\-cr|me(di|rc|ri)|mi(o8|oa|ts)|mmef|mo(01|02|bi|de|do|t(\-| |o|v)|zz)|mt(50|p1|v )|mwbp|mywa|n10[0-2]|n20[2-3]|n30(0|2)|n50(0|2|5)|n7(0(0|1)|10)|ne((c|m)\-|on|tf|wf|wg|wt)|nok(6|i)|nzph|o2im|op(ti|wv)|oran|owg1|p800|pan(a|d|t)|pdxg|pg(13|\-([1-8]|c))|phil|pire|pl(ay|uc)|pn\-2|po(ck|rt|se)|prox|psio|pt\-g|qa\-a|qc(07|12|21|32|60|\-[2-7]|i\-)|qtek|r380|r600|raks|rim9|ro(ve|zo)|s55\/|sa(ge|ma|mm|ms|ny|va)|sc(01|h\-|oo|p\-)|sdk\/|se(c(\-|0|1)|47|mc|nd|ri)|sgh\-|shar|sie(\-|m)|sk\-0|sl(45|id)|sm(al|ar|b3|it|t5)|so(ft|ny)|sp(01|h\-|v\-|v )|sy(01|mb)|t2(18|50)|t6(00|10|18)|ta(gt|lk)|tcl\-|tdg\-|tel(i|m)|tim\-|t\-mo|to(pl|sh)|ts(70|m\-|m3|m5)|tx\-9|up(\.b|g1|si)|utst|v400|v750|veri|vi(rg|te)|vk(40|5[0-3]|\-v)|vm40|voda|vulc|vx(52|53|60|61|70|80|81|83|85|98)|w3c(\-| )|webc|whit|wi(g |nc|nw)|wmlb|wonu|x700|yas\-|your|zeto|zte\-", RegexOptions.IgnoreCase | RegexOptions.Multiline);
            if (u.Length > 4 && (b.IsMatch(u) || v.IsMatch(u.Substring(0, 4))))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        /// <summary>
        /// url映射处理
        /// </summary>
        /// <param name="accessUrl"></param>
        /// <param name="agent"></param>
        RequestUrl UrlMappingHandle(RequestUrl request)
        {
            Uri accessUrl = new Uri(request.AccessUrl);
            foreach (UrlMapping mapping in domainList)
            {
                string pcRootUrl= new Uri(accessUrl, mapping.PcDomain).ToString();
                string mobileRootUrl = new Uri(accessUrl, mapping.MobileDomain).ToString();
                if (Regex.IsMatch(request.AccessUrl, "^" + pcRootUrl, RegexOptions.IgnoreCase))
                {
                    request.RealPath = Regex.Replace(request.AccessUrl, "^" + pcRootUrl, mapping.Path, RegexOptions.IgnoreCase);
                    if (mapping.MobileDomain != "")
                    { //使用手机访问
                        request.MobileUrl  = mobileRootUrl+ request.RealPath;
                    }
                    return request;
                }
                if (Regex.IsMatch(request.AccessUrl, "^" + mobileRootUrl, RegexOptions.IgnoreCase))
                {
                    request.RealPath = Regex.Replace(request.AccessUrl, "^" + mobileRootUrl, mapping.Path, RegexOptions.IgnoreCase);
                    request.IsMobileUrl = true;
                    return request;
                }
            }
            return request;
        }
        void UrlMappingHandle1(Uri accessUrl, string agent)
        {
            string url = this.NewUrl;
            foreach (UrlMapping mapping in domainList)
            {
                string _accessUrl = accessUrl.ToString().ToLower();
                string pcRootUri = new Uri(accessUrl, mapping.PcDomain).ToString().ToLower();
                if (_accessUrl.IndexOf(pcRootUri) == 0)
                {
                    if (mapping.Path == "")
                    {
                        this.NewUrl = mapping.Path + url;
                    }
                    else
                    {
                        this.NewUrl = "/" + mapping.Path + url;
                    }
                    if (mapping.MobileDomain != "" && this.IsMobileAccess(agent))
                    { //使用手机访问
                        this.NewUrl = "http://" + mapping.MobileDomain + url;
                        IsMobile = true;
                    }
                    continue;
                }
                if (String.Compare(mapping.PcDomain, accessUrl.Host, true) == 0)//pc域名访问
                {
                    this.NewUrl = "/" + mapping.Path + url;
                    if (mapping.MobileDomain != "" && this.IsMobileAccess(agent))
                    { //使用手机访问
                        this.NewUrl = "http://" + mapping.MobileDomain + url;
                        IsMobile = true;
                    }
                    continue;
                }
                else if (String.Compare(mapping.MobileDomain, accessUrl.Host, true) == 0)//手机域名转换
                {
                    this.NewUrl = "/" + mapping.Path + url;
                    this.IsMobile = true;
                    continue;
                }
                /*
                else if (Regex.IsMatch(url, "^/" + mapping.Url + "/", RegexOptions.IgnoreCase))//访问是原路径
                {
                    //this.virtualPath = "/" + mapping.PcDomain + "/";
                    url = "/" + Regex.Replace(url, "^/" + mapping.Url + "/", "", RegexOptions.IgnoreCase);
                    if (this.IsMobileAccess(agent))
                    {
                        if (mapping.MobileDomain != "")
                        {
                            this.NewUrl = "http://" + mapping.MobileDomain + url;
                            this.IsMobile = true;

                        }
                    }
                    else
                    {
                        if (mapping.PcDomain != "")
                        {
                            this.NewUrl = "http://" + mapping.PcDomain + url;
                        }
                    }

                }*/
            }
        }
        /// <summary>
        /// 虚拟路径处理
        /// </summary>
        /// <param name="accessUrl"></param>
        /// <param name="agent"></param>
        void VirtualPathHandle(Uri accessUrl, string agent)
        {
            if (this.MainMobileUrl != "")
            {
                if (this.MainMobileUrl.IndexOf("http") > -1)
                {
                    this.IsMobile = Regex.IsMatch(accessUrl.AbsoluteUri, "^" + this.MainMobileUrl);
                }
                else
                {
                    this.IsMobile = Regex.IsMatch(this.NewUrl, "^" + this.virtualPath + this.MainMobileUrl);
                }
            }
            if (this.IsMobile)
            {

                if (this.MainMobileUrl.IndexOf("http") > -1)
                {
                    //isMobilePage = Regex.IsMatch(newUrl, "^" + BaseConfig.mobileUrl);
                }
                else
                {
                    this.NewUrl = Regex.Replace(this.NewUrl, "^" + this.virtualPath + this.MainMobileUrl, this.virtualPath);
                }
            }
        }
        public RequestUrl Build(Uri accessUrl, string agent)
        {
            RequestUrl request = new RequestUrl()
            {
                RealPath = accessUrl.AbsolutePath,
                AccessUrl = accessUrl.ToString(),
                MobileUrl = accessUrl.ToString(),
                IsMobileBrowser = IsMobileAccess(agent + ""),
                IsMobileUrl = false
            };
            if (domainList != null)
            {
                request=this.UrlMappingHandle(request);
            }
            //if (!this.IsMobile)
            //{
            //    this.VirtualPathHandle(accessUrl, agent);
            //}
            return request;
        }
        public void AddMapping(UrlMapping mapping)
        {
            this.domainList.Add(mapping);
        }
    }
}
