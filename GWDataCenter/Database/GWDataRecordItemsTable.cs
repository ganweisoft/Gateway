﻿// Copyright (c) 2025 Shenzhen Ganwei Software Technology Co., Ltd
using System.ComponentModel.DataAnnotations.Schema;
namespace GWDataCenter.Database
{
    [Table("GWDataRecordItems")]
    public class GWDataRecordItemsTableRow
    {
        public int equip_no { get; set; }
        public string data_type { get; set; }
        public int ycyx_no { get; set; }
        public string data_name { get; set; }
    }
}
