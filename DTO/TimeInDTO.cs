using Microsoft.AspNetCore.Mvc;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sample_DTR_API.DTO
{
    public class TimeInDTO
    { 
        public int UserId { get; set; }

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}")]
        public DateOnly? TimeInDate { get; set; }

        [DefaultValue("00:00:00")]
        [DisplayFormat(DataFormatString ="hh:mm tt")]
        public TimeOnly? TimeInAm { get; set; }

        [DefaultValue("00:00:00")]
        [DisplayFormat(DataFormatString = "hh:mm tt")]
        public TimeOnly? TimeOutAm { get; set; }

        [DefaultValue("00:00:00")]
        [DisplayFormat(DataFormatString = "hh:mm tt")]
        public TimeOnly? TimeInPm { get; set; }

        [DefaultValue("00:00:00")]
        [DisplayFormat(DataFormatString = "hh:mm tt")]
        public TimeOnly? TimeOutPm { get; set; }

        //public TimeSpan TimeCreated
        //{
        //    get
        //    {
        //        return this.timeCreated.HasValue
        //           ? this.timeCreated.Value
        //           : new TimeSpan(DateTime.Now.Hour, DateTime.Now.Minute, 0);
        //    }

        //    set { this.timeCreated = value; }
        //}

        //private TimeSpan? timeCreated = null;
    }
}
