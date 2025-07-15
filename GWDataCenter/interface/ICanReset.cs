﻿// Copyright (c) 2004-2025 Shenzhen Ganwei Software Technology Co., Ltd
namespace GWDataCenter
{
    interface ICanReset
    {
        bool ResetWhenDBChanged(params object[] o);
    }
}
