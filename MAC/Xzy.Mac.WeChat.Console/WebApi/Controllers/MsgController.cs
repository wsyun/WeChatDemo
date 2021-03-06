﻿using WebApi.Model;
using System;
using System.Collections.Generic;
using System.Web.Http;
using Newtonsoft.Json;
using System.Drawing;
using WebApi.Utils;
using System.Configuration;
using WebApi.MyWebSocket;
using System.Linq;
using System.IO;
using System.Threading;

namespace WebApi.Controllers
{
    /// <summary>
    /// 消息模块
    /// </summary>
    [RoutePrefix("api/msg")]
    [Error]
    public class MsgController : ApiController
    {
        /// <summary>
        /// 发送文字消息
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("sendtext")]
        public IHttpActionResult SendText(SendTextModel model)
        {
            ApiServerMsg result = new ApiServerMsg();
            try
            {
                if (XzyWebSocket._dicSockets.ContainsKey(model.uuid))
                {
                    var res = XzyWebSocket._dicSockets[model.uuid].weChatThread.Wx_SendMsg(model.wxid, model.text,model.atlist);
                    result.Success = true;
                    result.Context = res;
                    return Ok(result);
                }
                else
                {
                    result.Success = false;
                    result.Context = "不存在该websocket连接";
                    return Ok(result);
                }

            }
            catch (Exception e)
            {
                result.Success = false;
                result.ErrContext = e.Message;
                return Ok(result);
            }
        }

        /// <summary>
        /// 发送语音消息 mp3格式
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("sendvoice")]
        public IHttpActionResult SendVoice(SendMediaModel model) {
            ApiServerMsg result = new ApiServerMsg();
            try
            {
                if (XzyWebSocket._dicSockets.ContainsKey(model.uuid))
                {
                    string path = System.AppDomain.CurrentDomain.BaseDirectory + "files";
                    if (!Directory.Exists(path))//如果不存在则创建
                    {
                        Directory.CreateDirectory(path);
                    }
                    byte[] audioByteArray = Convert.FromBase64String(model.base64.Replace("data:audio/mp3;base64,",""));

                    //保存mp3
                    string filename = path+"\\"+Guid.NewGuid() + ".mp3";
                    string silkname = filename.Replace(".mp3", ".silk");
                    var mp3File = File.Create(filename, audioByteArray.Length);
                    mp3File.Write(audioByteArray, 0, audioByteArray.Length);
                    mp3File.Flush();
                    mp3File.Close();
                    //转换silk
                    bool isCovert = ffmpegUtils.GetInstance().ConvertMp3ToAmr(filename, silkname);
                    if (isCovert)
                    {
                        while (MyUtils.IsFileInUse(silkname))
                        {
                            Thread.Sleep(100);
                        }
                    }

                    var res = XzyWebSocket._dicSockets[model.uuid].weChatThread.Wx_SendVoice(model.wxid, silkname, 1);
                    result.Success = true;
                    result.Context = res;
                    return Ok(result);
                }
                else
                {
                    result.Success = false;
                    result.Context = "不存在该websocket连接";
                    return Ok(result);
                }
            }
            catch (Exception e)
            {
                result.Success = false;
                result.ErrContext = e.Message;
                return Ok(result);
            }
        }

        /// <summary>
        ///  发送语音消息 silk格式
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("sendvoicesilk")]
        public IHttpActionResult SendVoiceSilk(SendMediaModel model)
        {
            ApiServerMsg result = new ApiServerMsg();
            try
            {
                if (XzyWebSocket._dicSockets.ContainsKey(model.uuid))
                {
                    string path = System.AppDomain.CurrentDomain.BaseDirectory + "files";
                    if (!Directory.Exists(path))//如果不存在则创建
                    {
                        Directory.CreateDirectory(path);
                    }
                    byte[] audioByteArray = Convert.FromBase64String(model.base64.Replace("data:application/octet-stream;base64,", ""));

                    //保存mp3
                    string filename = path + "\\" + Guid.NewGuid() + ".silk";
                    var mp3File = File.Create(filename, audioByteArray.Length);
                    mp3File.Write(audioByteArray, 0, audioByteArray.Length);
                    mp3File.Flush();
                    mp3File.Close();
                   
                    var res = XzyWebSocket._dicSockets[model.uuid].weChatThread.Wx_SendVoice(model.wxid, filename, 1);
                    result.Success = true;
                    result.Context = res;
                    return Ok(result);
                }
                else
                {
                    result.Success = false;
                    result.Context = "不存在该websocket连接";
                    return Ok(result);
                }
            }
            catch (Exception e)
            {
                result.Success = false;
                result.ErrContext = e.Message;
                return Ok(result);
            }
        }

        /// <summary>
        /// 发送图片消息
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("sendimg")]
        public IHttpActionResult SendImg(SendMediaModel model)
        {
            ApiServerMsg result = new ApiServerMsg();
            try
            {
                if (XzyWebSocket._dicSockets.ContainsKey(model.uuid))
                {
                    Image img = ConvertUtils.GetImageFromBase64(model.base64);
                    var res = XzyWebSocket._dicSockets[model.uuid].weChatThread.Wx_SendImg(model.wxid, img);
                    result.Success = true;
                    result.Context = res;
                    return Ok(result);
                }
                else
                {
                    result.Success = false;
                    result.Context = "不存在该websocket连接";
                    return Ok(result);
                }

            }
            catch (Exception e)
            {
                result.Success = false;
                result.ErrContext = e.Message;
                return Ok(result);
            }

        }

        /// <summary>
        /// 发送链接消息
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("sendapp")]
        public IHttpActionResult SendApp(SendAppModel model)
        {
            ApiServerMsg result = new ApiServerMsg();
            try
            {
                if (XzyWebSocket._dicSockets.ContainsKey(model.uuid))
                {

                    string xml = App.AppMsgXml.
               Replace("$appid$", model.appid).
                Replace("$sdkver$", model.sdkver).
                 Replace("$title$", model.title).
                  Replace("$des$", model.des).
                   Replace("$url$", model.url).
                    Replace("$thumburl$", model.thumburl);
                    var res = XzyWebSocket._dicSockets[model.uuid].weChatThread.Wx_SendAppMsg(model.wxid, xml);
                    result.Success = true;
                    result.Context = res;
                    return Ok(result);
                }
                else
                {
                    result.Success = false;
                    result.Context = "不存在该websocket连接";
                    return Ok(result);
                }

            }
            catch (Exception e)
            {
                result.Success = false;
                result.ErrContext = e.Message;
                return Ok(result);
            }
        }

        /// <summary>
        /// 发送名片消息
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("sendshardcard")]
        public IHttpActionResult SendShareCard(ShardCardModel model)
        {
            ApiServerMsg result = new ApiServerMsg();
            try
            {
                if (XzyWebSocket._dicSockets.ContainsKey(model.uuid))
                {
                    var res = XzyWebSocket._dicSockets[model.uuid].weChatThread.Wx_EShareCard(model.user, model.wxid,model.title);
                    result.Success = true;
                    result.Context = res;
                    return Ok(result);
                }
                else
                {
                    result.Success = false;
                    result.Context = "不存在该websocket连接";
                    return Ok(result);
                }
            }
            catch (Exception e)
            {
                result.Success = false;
                result.ErrContext = e.Message;
                return Ok(result);
            }
        }

        /// <summary>
        /// 群发消息
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("sendmass")]
        public IHttpActionResult SendMass(SendMassModel model)
        {
            ApiServerMsg result = new ApiServerMsg();
            try
            {
                if (XzyWebSocket._dicSockets.ContainsKey(model.uuid))
                {
                    var res = XzyWebSocket._dicSockets[model.uuid].weChatThread.Wx_MassMessage(JsonConvert.SerializeObject(model.wxids), model.text);
                    result.Success = true;
                    result.Context = res;
                    return Ok(result);
                }
                else
                {
                    result.Success = false;
                    result.Context = "不存在该websocket连接";
                    return Ok(result);
                }

            }
            catch (Exception e)
            {
                result.Success = false;
                result.ErrContext = e.Message;
                return Ok(result);
            }

        }



        /// <summary>
        /// 获取图片消息
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("getimg")]
        public IHttpActionResult GetMsgImg(MessageModel model)
        {
            ApiServerMsg result = new ApiServerMsg();
            try
            {
                if (XzyWebSocket._dicSockets.ContainsKey(model.uuid))
                {
                    var res = XzyWebSocket._dicSockets[model.uuid].weChatThread.Wx_GetMsgImage(JsonConvert.SerializeObject( model.msg));
                    result.Success = true;
                    result.Context = res;
                    return Ok(result);
                }
                else
                {
                    result.Success = false;
                    result.Context = "不存在该websocket连接";
                    return Ok(result);
                }

            }
            catch (Exception e)
            {
                result.Success = false;
                result.ErrContext = e.Message;
                return Ok(result);
            }
        }

        /// <summary>
        /// 获取语音消息
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("getvoice")]
        public IHttpActionResult GetMsgVoice(MessageModel model)
        {
            ApiServerMsg result = new ApiServerMsg();
            try
            {
                if (XzyWebSocket._dicSockets.ContainsKey(model.uuid))
                {
                    var res = XzyWebSocket._dicSockets[model.uuid].weChatThread.Wx_GetMsgVoice(JsonConvert.SerializeObject(model.msg));
                    result.Success = true;
                    result.Context = res;
                    return Ok(result);
                }
                else
                {
                    result.Success = false;
                    result.Context = "不存在该websocket连接";
                    return Ok(result);
                }

            }
            catch (Exception e)
            {
                result.Success = false;
                result.ErrContext = e.Message;
                return Ok(result);
            }
        }

        /// <summary>
        /// 获取视频消息
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("getvideo")]
        public IHttpActionResult GetMsgVideo(MessageModel model)
        {
            ApiServerMsg result = new ApiServerMsg();
            try
            {
                if (XzyWebSocket._dicSockets.ContainsKey(model.uuid))
                {
                    var res = XzyWebSocket._dicSockets[model.uuid].weChatThread.Wx_GetMsgVideo(JsonConvert.SerializeObject(model.msg));
                    result.Success = true;
                    result.Context = res;
                    return Ok(result);
                }
                else
                {
                    result.Success = false;
                    result.Context = "不存在该websocket连接";
                    return Ok(result);
                }

            }
            catch (Exception e)
            {
                result.Success = false;
                result.ErrContext = e.Message;
                return Ok(result);
            }
        }

        /// <summary>
        /// 抢红包 ，sub_type == 49    CDATA[1002]
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("getreadpack")]
        public IHttpActionResult GetReadPack(MessageModel model)
        {
            ApiServerMsg result = new ApiServerMsg();
            try
            {
                if (XzyWebSocket._dicSockets.ContainsKey(model.uuid))
                {
                    if (model.msg.content.IndexOf("CDATA[1002]") != -1)//当sub_type=49，并且content内容包含“微信转账”时，表示这是一笔微信转账通知
                    {
                        var res = XzyWebSocket._dicSockets[model.uuid].weChatThread.RedpackOK(JsonConvert.SerializeObject(model.msg), model.msg.Timestamp);
                        result.Success = true;
                        result.Context = res;
                        return Ok(result);
                    }
                    else
                    {
                        result.Success = false;
                        result.Context = "消息错误";
                        return Ok(result);
                    }
                }
                else
                {
                    result.Success = false;
                    result.Context = "不存在该websocket连接";
                    return Ok(result);
                }

            }
            catch (Exception e)
            {
                result.Success = false;
                result.ErrContext = e.Message;
                return Ok(result);
            }
        }

        /// <summary>
        /// 接受转账 ，sub_type == 49   CDATA[微信转账]
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("gettransfer")]
        public IHttpActionResult GetTransfer(MessageModel model)
        {
            ApiServerMsg result = new ApiServerMsg();
            try
            {
                if (XzyWebSocket._dicSockets.ContainsKey(model.uuid))
                {
                    if (model.msg.content.IndexOf("CDATA[微信转账]") != -1)//当sub_type=49，并且content内容包含“微信转账”时，表示这是一笔微信转账通知
                    {
                        var res = XzyWebSocket._dicSockets[model.uuid].weChatThread.Wx_WXTransferOperation(JsonConvert.SerializeObject(model.msg));
                        result.Success = true;
                        result.Context = res;
                        return Ok(result);
                    }
                    else
                    {
                        result.Success = false;
                        result.Context = "消息错误";
                        return Ok(result);
                    }
                }
                else
                {
                    result.Success = false;
                    result.Context = "不存在该websocket连接";
                    return Ok(result);
                }

            }
            catch (Exception e)
            {
                result.Success = false;
                result.ErrContext = e.Message;
                return Ok(result);
            }
        }

        /// <summary>
        /// 接受群邀请，sub_type == 49  ,传msg.content 包含 “加入群聊”
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("intogroup")]
        public IHttpActionResult IntoGroup(IntoGroupModel model)
        {
            ApiServerMsg result = new ApiServerMsg();
            try
            {
                if (XzyWebSocket._dicSockets.ContainsKey(model.uuid))
                {
                    if (model.content.IndexOf("加入群聊") != -1)
                    {
                        var url = Utilities.GetMidStr(model.content, "<url><![CDATA[", "]]>");
                        XzyWebSocket._dicSockets[model.uuid].weChatThread.Wx_IntoGroup(url);
                        result.Success = true;
                        result.Context = "调用成功";
                        return Ok(result);
                    }
                    else
                    {
                        result.Success = false;
                        result.Context = "参数错误，请检查消息是否为进群消息";
                        return Ok(result);
                    }
                }
                else
                {
                    result.Success = false;
                    result.Context = "不存在该websocket连接";
                    return Ok(result);
                }

            }
            catch (Exception e)
            {
                result.Success = false;
                result.ErrContext = e.Message;
                return Ok(result);
            }
        }
    }
}
