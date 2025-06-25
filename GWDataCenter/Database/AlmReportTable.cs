﻿﻿// Copyright (c) 2025 Shenzhen Ganwei Software Technology Co., Ltd
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace GWDataCenter.Database
{
    [Table("AlmReport")]
    public class AlmReportTableRow
    {
        [Key]
        public int id { get; set; }
        public int? sta_n { get; set; }
        public int? group_no { get; set; }
        public string Administrator { get; set; }
    }
}
