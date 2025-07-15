﻿﻿// Copyright (c) 2004-2025 Shenzhen Ganwei Software Technology Co., Ltd
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace GWDataCenter.Database
{
    [Table("SpeAlmReport")]
    public class SpeAlmReportTableRow
    {
        [Key]
        public int id { get; set; }
        public int sta_n { get; set; }
        public int group_no { get; set; }
        public string Administrator { get; set; }
        public DateTime begin_time { get; set; }
        public DateTime end_time { get; set; }
        public string remark { get; set; }
        public string Color { get; set; }
        public string name { get; set; }
    }
}
