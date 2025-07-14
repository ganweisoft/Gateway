﻿// Copyright (c) 2004 Shenzhen Ganwei Software Technology Co., Ltd
namespace GWDataCenter
{
    public interface IPlanItem
    {
        bool bStopPlanItem
        {
            get;
            set;
        }
        bool bWorkState
        {
            get;
            set;
        }
    }
}
