using Microsoft.Ajax.Utilities;
using OSS.Common.Resp;
using OSS.PaySdk.Wx;
using OSS.PaySdk.Wx.Pay;
using OSS.PaySdk.Wx.Pay.Mos;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace PayDemo.Controllers
{
    public class HomeController : Controller
    {

        public HomeController()
        {

        }

        public static string _callBackDomain = "http://www.baidu.com";//回调地址
        static WxPayCenterConfig config = new WxPayCenterConfig()
        {
            AppId = "wx9b431729cbc61a32",
            MchId = "1517045981",
            CertPassword = "1517045981",//证书密码 默认商户号
            Key = "ikk1yrlzycov4bnxpkvtbqbqefn5e0sd",//Key
            CertPath = "",//证书路径 
        };

        private static readonly WxPayTradeApi _api = new WxPayTradeApi(config);

        public ActionResult Index()
        {
            return View();
        }

        // 获取App支付参数 必须为异步否则有bug
        public async Task<ActionResult> GetJsPayInfo(string orderId)
        {
            var order = GetUniorder(orderId, "APP");

            WxAddPayOrderResp orderRes = null;

            orderRes = await _api.AddUniOrderAsync(order);


            if (!orderRes.IsSuccess()) return new JsonResult()
            {

                Data = orderRes,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };

            var jsPara = _api.GetAppClientParaResp(orderRes.prepay_id);
            return new JsonResult()
            {

                Data = jsPara,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };


        }


        private static WxAddPayUniOrderReq GetUniorder(string orderId, string tradeType)
        {
            return new WxAddPayUniOrderReq
            {
                notify_url = _callBackDomain,//回调地址
                body = "OSSPay-测试商品",//商品描述信息
                device_info = "APP", //设备号可为空
                out_trade_no = orderId,//商户订单号

                spbill_create_ip = "114.242.25.208",//用户终端ip
                total_fee = 1,//支付总金额 单位为分
                trade_type = tradeType//交易类型
            };
        }

        /// <summary>
        /// 支付回调
        /// </summary>
        /// <returns></returns>
        public ActionResult receive()
        {
            string strPayResult;
            using (var streamReader = new StreamReader(Request.InputStream))
            {
                strPayResult = streamReader.ReadToEnd();
            }
            var wxPayRes = _api.DecryptPayResult(strPayResult);
            //wxPayRes.result_code//支付结果

            //处理业务逻辑

            //  do something with wxPayRes
            var returnXml = _api.GetCallBackReturnXml(new Resp());
            return Content(returnXml);
        }

        //  退款示例
        private static readonly WxPayRefundApi _refundApi = new WxPayRefundApi(config);


        /// <summary>
        /// 退款
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        public async Task<ActionResult> refund(string orderId)
        {

            var refundRes = await _refundApi.RefundOrderAsync(new WxPayRefundReq()
            {
                out_trade_no = orderId,//商户订单号
                total_fee = 1,//订单金额
                refund_fee = 1,//退款金额
                out_refund_no = orderId //退款订单号
            });

            return new JsonResult()
            {

                Data = refundRes,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }


        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}