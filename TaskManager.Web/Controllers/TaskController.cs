﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using Common.Logging;
using Quartz;
using Quartz.Spi;
using TaskManager.Common.Log;
using TaskManager.Common.Log.Entity;
using TaskManager.Task;
using TaskManager.Task.Entities;
using TaskManager.Task.Services;
using TaskManager.Web.Filters;

namespace TaskManager.Web.Controllers
{
    [ExceptionFilter]
    public class TaskController : Controller
    {
        private TaskService _taskService = new TaskService();

        /// <summary>
        /// 任务列表
        /// </summary>
        /// <returns></returns>
        public ActionResult LocalTaskList()
        {
            var list = this._taskService.GetAll() as IList<TaskDetailEntity>;
            //throw new Exception("异常测试");
            return View(list);
        }

        public ActionResult TaskDetailAddOrEdit(int id = 0)
        {
            TaskDetailEntity entity = new TaskDetailEntity();
            if (id > 0)
            {
                entity = this._taskService.Get(id);
            }
            return View(entity);
        }
        /// <summary>
        /// 任务编辑
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult TaskDetailAddOrEdit(TaskDetailEntity entity)
        {
            try
            {
                if (entity.Id > 0)
                {
                    this._taskService.Update(entity);
                }
                else
                {
                    this._taskService.Insert(entity);
                }
                return Json(new { st = 1, msg = "保存成功！" });
            }
            catch (Exception e)
            {
                return Json(new { st = 0, msg = "保存失败，" + e.Message });
            }
        }


        #region 任务执行操作
        /// <summary>
        /// 是否停用任务
        /// </summary>
        /// <param name="id"></param>
        /// <param name="enabled"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult ChangeEnabled(int id, bool enabled)
        {
            this._taskService.ChangeEnabled(id, enabled);
            var entity = this._taskService.Get(id);
            return this.PartialView("_TaskDetailRow", entity);
        }
        /// <summary>
        /// 运行单个任务
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult RunTask(int id)
        {
            var taskScheduler = TaskSchedulerFactory.GetScheduler();
            taskScheduler.Run(id);
            return Json(new { st = 1, msg = "任务执行完成！" });
        }
        /// <summary>
        /// 重启所有任务
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult ResumeAllTasks()
        {
            try
            {
                throw new Exception("Exception测试");
            }
            catch (Exception e)
            {
                LogHelper<TaskMonitorEntity>.Error(new TaskMonitorEntity { TaskId = 1999, Message = "消息内容测试" }, e);
                throw new Exception(typeof(TaskMonitorEntity).FullName + "系统错误测试");
            }

            return new EmptyResult();
            var taskScheduler = TaskSchedulerFactory.GetScheduler();
            taskScheduler.ResumeAll();
            var list = this._taskService.GetAll() as IList<TaskDetailEntity>;
            return View("LocalTaskList", list);
        }
        #endregion

        public ActionResult QuartzCron()
        {
            return View();
        }
        /// <summary>
        /// 获取任务在未来周期内哪些时间会运行
        /// </summary>
        /// <param name="cronExpression">Cron表达式</param>
        /// <returns></returns>
        public ActionResult GetTaskeFireTime(string cronExpression)
        {
            //时间表达式
            ITrigger trigger = TriggerBuilder.Create().WithCronSchedule(cronExpression).Build();
            IList<DateTimeOffset> dates = TriggerUtils.ComputeFireTimes(trigger as IOperableTrigger, null, 5);
            List<string> list = new List<string>();
            foreach (DateTimeOffset dtf in dates)
            {
                list.Add(TimeZoneInfo.ConvertTimeFromUtc(dtf.DateTime, TimeZoneInfo.Local).ToString());
            }
            return Json(list,JsonRequestBehavior.AllowGet);
        }
    }
}