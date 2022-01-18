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
    public class GetFingerprintByGuidsResponseModel
    {
        public GetFingerprintByGuidsResponseModel()
        {
            Fingerprint = new FingerprintModel();
        }
        public FingerprintModel Fingerprint { get; set; }
        public class FingerprintModel
        {
            public Int64 Id { get; set; }
            public Int64 CreatedById { get; set; }
            public DateTime CreatedDate { get; set; }
            public DateTime? ExpirationDate { get; set; }
            public Byte[] FingerprintId { get; set; }
            public Boolean IsActive { get; set; }
            public Int64 LastUpdatedById { get; set; }
            public Int64 NmlsId { get; set; }
            public DateTime? RenewalDate { get; set; }
            public Int64 StateId { get; set; }
            public DateTime UpdatedDate { get; set; }
            public FingerprintModel()
            {
            }
        }
    }
}
