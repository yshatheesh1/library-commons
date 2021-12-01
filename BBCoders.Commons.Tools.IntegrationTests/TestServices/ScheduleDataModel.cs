//------------------------------------------------------------------------------
// <auto-generated>
//
// Manual changes to this file may cause unexpected behavior in your application.
// Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------
using System;

namespace BBCoders.Example.DataServices
{
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
    public class GetSheduleRequestModel
    {
        public Byte[] id { get; set; }
    }
    public class GetSheduleResponseModel
    {
        public Int64 ScheduleId { get; set; }
        public Int64? ScheduleActionId { get; set; }
        public Int64 ScheduleCreatedById { get; set; }
        public DateTime ScheduleCreatedDate { get; set; }
        public Int64? ScheduleFingerPrintId { get; set; }
        public Int64 ScheduleLastUpdatedById { get; set; }
        public DateTime ScheduleLastUpdatedDate { get; set; }
        public DateTime ScheduleScheduleDate { get; set; }
        public Byte[] ScheduleScheduleId { get; set; }
        public Int64 ScheduleScheduleSiteId { get; set; }
        public Int64 ScheduleSiteId { get; set; }
        public Boolean ScheduleSiteIsActive { get; set; }
        public String ScheduleSiteName { get; set; }
        public Byte[] ScheduleSiteScheduleSiteId { get; set; }
    }
}