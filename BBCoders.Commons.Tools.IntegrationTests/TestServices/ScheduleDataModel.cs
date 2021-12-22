//------------------------------------------------------------------------------
// <auto-generated>
//
// Manual changes to this file may cause unexpected behavior in your application.
// Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------
using System;
using System.Collections.Generic;

namespace BBCoders.Example.DataServices
{
    public class ScheduleKey
    {
        public Int64 Id { get; set; }
    }
    public class ScheduleModel
    {
        public Int64 Id { get; set; }
        public Int64? ActionId { get; set; }
        public Int64 CreatedById { get; set; }
        public DateTime CreatedDate { get; set; }
        public Int64? FingerPrintId { get; set; }
        public Int64 LastUpdatedById { get; set; }
        public DateTime LastUpdatedDate { get; set; }
        public DateTime ScheduleDate { get; set; }
        public Byte[] ScheduleId { get; set; }
        public Int64 ScheduleSiteId { get; set; }
    }
    public class GetSheduleActionRequestModel
    {
        public Byte[] id { get; set; }
    }
    public class GetSheduleActionResponseModel
    {
        public ScheduleProjection Schedule { get; set; }
        public GetSheduleActionResponseModel()
        {
            Schedule = new ScheduleProjection();
        }
        public class ScheduleProjection
        {
            public Int64? action { get; set; }
            public Int64 id { get; set; }
            public Byte[] schedule_id { get; set; }
        }
    }
    public class GetSheduleRequestModel
    {
        public Byte[] id { get; set; }
    }
    public class GetSheduleResponseModel
    {
        public ScheduleProjection Schedule { get; set; }
        public ScheduleSiteProjection ScheduleSite { get; set; }
        public GetSheduleResponseModel()
        {
            Schedule = new ScheduleProjection();
            ScheduleSite = new ScheduleSiteProjection();
        }
        public class ScheduleProjection
        {
            public Int64 Id { get; set; }
            public Int64? ActionId { get; set; }
            public Int64 CreatedById { get; set; }
            public DateTime CreatedDate { get; set; }
            public Int64? FingerPrintId { get; set; }
            public Int64 LastUpdatedById { get; set; }
            public DateTime LastUpdatedDate { get; set; }
            public DateTime ScheduleDate { get; set; }
            public Byte[] ScheduleId { get; set; }
            public Int64 ScheduleSiteId { get; set; }
        }
        public class ScheduleSiteProjection
        {
            public Int64 Id { get; set; }
            public Boolean IsActive { get; set; }
            public String Name { get; set; }
            public Byte[] ScheduleSiteId { get; set; }
        }
    }
    public class GetScheduleActionAndLocationRequestModel
    {
        public Byte[] ActionId { get; set; }
        public Byte[] LocationId { get; set; }
    }
    public class GetScheduleActionAndLocationResponseModel
    {
        public ScheduleProjection Schedule { get; set; }
        public ActionProjection Action { get; set; }
        public ScheduleSiteProjection ScheduleSite { get; set; }
        public GetScheduleActionAndLocationResponseModel()
        {
            Schedule = new ScheduleProjection();
            Action = new ActionProjection();
            ScheduleSite = new ScheduleSiteProjection();
        }
        public class ScheduleProjection
        {
            public Int64 Id { get; set; }
            public Int64? ActionId { get; set; }
            public Int64 CreatedById { get; set; }
            public DateTime CreatedDate { get; set; }
            public Int64? FingerPrintId { get; set; }
            public Int64 LastUpdatedById { get; set; }
            public DateTime LastUpdatedDate { get; set; }
            public DateTime ScheduleDate { get; set; }
            public Byte[] ScheduleId { get; set; }
            public Int64 ScheduleSiteId { get; set; }
        }
        public class ActionProjection
        {
            public Int64 Id { get; set; }
            public Byte[] ActionId { get; set; }
            public String Name { get; set; }
        }
        public class ScheduleSiteProjection
        {
            public Int64 Id { get; set; }
            public Boolean IsActive { get; set; }
            public String Name { get; set; }
            public Byte[] ScheduleSiteId { get; set; }
        }
    }
    public class GetSheduleAsyncRequestModel
    {
        public Byte[] id { get; set; }
    }
    public class GetSheduleAsyncResponseModel
    {
        public ScheduleProjection Schedule { get; set; }
        public StateProjection State { get; set; }
        public GetSheduleAsyncResponseModel()
        {
            Schedule = new ScheduleProjection();
            State = new StateProjection();
        }
        public class ScheduleProjection
        {
            public Int64 Id { get; set; }
            public Int64 Test { get; set; }
        }
        public class StateProjection
        {
            public Int64? Id { get; set; }
            public String Name { get; set; }
            public Byte[] StateId { get; set; }
        }
        public Boolean StateActive { get; set; }
    }
    public class GetSheduleAsync2RequestModel
    {
    }
    public class GetSheduleAsync2ResponseModel
    {
        public GetSheduleAsync2ResponseModel()
        {
        }
        public Int32 Value_0 { get; set; }
    }
}
