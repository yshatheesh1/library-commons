//------------------------------------------------------------------------------
// <auto-generated>
//
// Manual changes to this file may cause unexpected behavior in your application.
// Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------
using System;
using System.Collections.Generic;

namespace BBCoders.Example.DataModels
{
    public class GetFingerprintsByIdResponseModel
    {
        public GetFingerprintsByIdResponseModel()
        {
            Fingerprint = new FingerprintModel();
        }
        public FingerprintModel Fingerprint { get; set; }
        public class FingerprintModel
        {
            public Int64 Id { get; set; }
            public Guid FingerprintId { get; set; }
            public Boolean IsActive { get; set; }
            public FingerprintModel()
            {
            }
        }
    }
}