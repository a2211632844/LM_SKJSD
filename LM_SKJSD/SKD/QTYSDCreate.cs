using Kingdee.BOS.WebApi.Client;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LM_SKJSD.SKD
{
    public class QTYSDCreate
    {
        //生成其他应收单
        public static string JsonAdd2(string jsonArrayText, string user, string psw, string dbid, string apiurl)
        {

            string strresult = string.Empty;
            JObject jo = (JObject)JsonConvert.DeserializeObject(jsonArrayText);
            //金蝶云组件
            K3CloudApiClient client = new K3CloudApiClient(apiurl);
            var loginResult = client.Login(
                   dbid,
                   user,
                   psw,
                   2052);

            string result = "登录失败，请检查与站点地址、数据中心Id，用户名及密码！";

            if (loginResult == true)
            {
                JObject jsonRoot = new JObject();


                // Model: 单据详细数据参数
                JObject model = new JObject();
                jsonRoot.Add("Model", model);
                // 单据主键：必须填写，系统据此判断是新增还是修改单据；新增单据，填0
                model.Add("FID", 0);

                // 普通字段

                // 基础资料，填写编码

                //model.Add("FCONTACTUNIT", new JObject() { { "FNumber", jo["FCONTACTUNIT"] } });//客户


                // 开始构建单据体参数：集合参数JArray
                JArray entryRows = new JArray();
                // 把单据体行集合，添加到model中，以单据体Key为标识
                string entityKey = "FEntity";
                JObject entitymodel = new JObject();


                //单据名称单据编号
                entitymodel.Add("FNOTAXAMOUNTFOR", jo["FNOTAXAMOUNTFOR"]);//金额

                entryRows.Add(entitymodel);
                model.Add(entityKey, entryRows);



                // 调用Web API接口服务，
                result = client.Execute<string>(
                    "Kingdee.BOS.WebApi.ServicesStub.DynamicFormService.Draft",
                    new object[] { "AR_OtherRecAble", jsonRoot.ToString() });
            }
            return result;
        }
    }
}
