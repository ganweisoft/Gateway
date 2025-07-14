﻿// Copyright (c) 2004 Shenzhen Ganwei Software Technology Co., Ltd
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace GWDataCenter.Database
{
    [Table("GWProcTimeTList")]
    public class GWProcTimeTListTableRow
    {
        [Key]
        public int TableID { get; set; }
        [Required]
        public string TableName { get; set; }
        public string Comment { get; set; }
    }
}
