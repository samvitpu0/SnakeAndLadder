using PsypherLibrary.SupportLibrary.Utils.Generics;
#if ENABLE_NUTILS
using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using PsypherLibrary.SupportLibrary.Extensions;
using PsypherLibrary.SupportLibrary.Managers;
using SA.Android.App;
using UnityEngine;

namespace PsypherLibrary.SupportLibrary.Utils.Native.Notification
{
    public class LocalNotificationHelper : GenericManager<LocalNotificationHelper>
    {
        #region Supports

        private struct NotificationInfo
        {
            public string Title;
            public string Message;
            public TimeSpan TriggerTime;
            public string UID;

            public NotificationInfo(string title, string message, TimeSpan triggerTime, string uId = "")
            {
                Title = title;
                Message = message;
                TriggerTime = triggerTime;
                UID = uId;
            }
        }

        #endregion

        #region Fields and Properties

        private int _currentNotificationScheduleCue = 0;

        private List<NotificationInfo> _notificationData = new List<NotificationInfo>();

        #endregion

        #region Initialization

        private void OnEnable()
        {
            LocalDataManager.OnAppFocus += OnAppFocus;
        }

        private void OnDisable()
        {
            LocalDataManager.OnAppFocus -= OnAppFocus;
        }

        #endregion

        #region Actions

        void CancelAllNotifications()
        {
            AN_NotificationManager.UnscheduleAll();
            AN_NotificationManager.CancelAll();
            _currentNotificationScheduleCue = 0;

            Debug.Log("Cancelling all scheduled local notifications");
        }

        void ScheduleAllNotification()
        {
            _notificationData.ForEach(x => ScheduleNotification(x.Title, x.Message, x.TriggerTime));
        }

        /// <summary>
        /// requesting for schedule notification, please supply UID so that notification are not repeated
        /// </summary>
        /// <param name="title"></param>
        /// <param name="message"></param>
        /// <param name="triggerTime"></param>
        /// <param name="uId"></param>
        public void RequestNotificationSchedule(string title, string message, TimeSpan triggerTime, [CanBeNull] string uId = null)
        {
            if (uId == null)
            {
                _notificationData.Add(new NotificationInfo(title, message, triggerTime));
            }
            else
            {
                _notificationData.AddUnique(new NotificationInfo(title, message, triggerTime, uId), x => x.UID != uId);
            }
        }

        /// <summary>
        /// Schedules local notification
        /// </summary>
        /// <param name="title"></param>
        /// <param name="message"></param>
        /// <param name="triggerTime"></param>
        /// <param name="customIconName"></param>
        public void ScheduleNotification(string title, string message, TimeSpan triggerTime, string customIconName = "rg_push_small_icon")
        {
            var builder = new AN_NotificationCompat.Builder();
            builder.SetContentTitle(title);
            builder.SetContentText(message);

            //setting icon for notifications
            builder.SetSmallIcon(customIconName);

            var trigger = new AN_AlarmNotificationTrigger();
            trigger.SetDate(triggerTime);

            var notificationId = _currentNotificationScheduleCue;
            var nRequest = new AN_NotificationRequest(notificationId, builder, trigger);

            Debug.Log("Local Notification Scheduled in: " + triggerTime + ", invoking: " + title + ", current queue ID: " + _currentNotificationScheduleCue);

            //increase the queue ID
            _currentNotificationScheduleCue++;

            AN_NotificationManager.Schedule(nRequest);
        }

        #endregion

        #region Events

        private void OnAppFocus(bool focus)
        {
            if (focus)
            {
                CancelAllNotifications();
            }
            else
            {
                ScheduleAllNotification();
                PlayerPrefs.Save(); //so that notification schedule ids are properly saved
            }
        }

        #endregion
    }
}
#endif